using System;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Constants;
using HelpDesk.Data;
using HelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TicketsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsAdminOrSupport()
            => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Support);

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.User)
                .AsQueryable();

            // Admin i Support widzą wszystko, reszta tylko swoje
            if (!IsAdminOrSupport())
            {
                query = query.Where(t => t.UserId == userId);
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.Priorities = await _context.Priorities.OrderBy(p => p.Id).ToListAsync();

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            // Zwykły user nie może oglądać cudzych zgłoszeń
            if (!IsAdminOrSupport())
            {
                var userId = _userManager.GetUserId(User);
                if (ticket.UserId != userId) return Forbid();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            // User wybiera tylko kategorię (priorytet ustawiamy automatycznie na "Średni")
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,CategoryId")] Ticket ticket)
        {
            // Ustawienia automatyczne (user NIC z tego nie podaje)
            ticket.UserId = _userManager.GetUserId(User)!;
            ticket.CreatedAt = DateTime.Now;
            ticket.Status = "Nowe";

            // Priorytet domyślny: "Średni" (obsłuż też wersję bez polskich znaków)
            var defaultPriority = await _context.Priorities.FirstOrDefaultAsync(p =>
                p.Name == "Średni" ||
                p.Name == "Sredni" ||
                p.Name.ToLower() == "średni" ||
                p.Name.ToLower() == "sredni"
            );

            if (defaultPriority == null)
            {
                // awaryjnie: bierz pierwszy priorytet, jeśli istnieje
                defaultPriority = await _context.Priorities.OrderBy(p => p.Id).FirstOrDefaultAsync();
            }

            if (defaultPriority == null)
            {
                ModelState.AddModelError("", "Brak priorytetów w systemie. Dodaj priorytety jako Admin.");
            }
            else
            {
                ticket.PriorityId = defaultPriority.Id;
            }

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ticket.CategoryId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            // Zwykły user może edytować tylko swoje zgłoszenie
            if (!IsAdminOrSupport())
            {
                var userId = _userManager.GetUserId(User);
                if (ticket.UserId != userId) return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ticket.CategoryId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CategoryId")] Ticket edited)
        {
            if (id != edited.Id) return NotFound();

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            // Zwykły user może edytować tylko swoje zgłoszenie
            if (!IsAdminOrSupport())
            {
                var userId = _userManager.GetUserId(User);
                if (ticket.UserId != userId) return Forbid();
            }

            if (ModelState.IsValid)
            {
                // Aktualizujemy tylko pola, które user ma prawo zmieniać
                ticket.Title = edited.Title;
                ticket.Description = edited.Description;
                ticket.CategoryId = edited.CategoryId;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", edited.CategoryId);
            return View(edited);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            // Zwykły user nie może usuwać cudzych zgłoszeń
            if (!IsAdminOrSupport())
            {
                var userId = _userManager.GetUserId(User);
                if (ticket.UserId != userId) return Forbid();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            // Zwykły user nie może usuwać cudzych zgłoszeń
            if (!IsAdminOrSupport())
            {
                var userId = _userManager.GetUserId(User);
                if (ticket.UserId != userId) return Forbid();
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin + "," + Roles.Support)]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin + "," + Roles.Support)]
        public async Task<IActionResult> ChangePriority(int id, int priorityId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var exists = await _context.Priorities.AnyAsync(p => p.Id == priorityId);
            if (!exists) return BadRequest();

            ticket.PriorityId = priorityId;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
