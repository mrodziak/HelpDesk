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

        private string CurrentUserId() => _userManager.GetUserId(User)!;

        private bool IsAdmin() => User.IsInRole(Roles.Admin);
        private bool IsSupport() => User.IsInRole(Roles.Support);
        private bool IsAdminOrSupport() => IsAdmin() || IsSupport();

        private bool CanSupportEditTicket(Ticket t)
            => IsSupport() && t.AssignedToUserId == CurrentUserId();

        private bool CanEditStatusAndPriority(Ticket t)
            => IsAdmin() || CanSupportEditTicket(t);

        // GET: Tickets
        // filter: all | mine | unassigned
        public async Task<IActionResult> Index(string filter = "all")
        {
            var userId = CurrentUserId();

            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.User)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            if (!IsAdminOrSupport())
            {
                // zwykły user widzi tylko swoje
                query = query.Where(t => t.UserId == userId);
                filter = "all";
            }
            else
            {
                // admin/support: filtry
                filter = (filter ?? "all").ToLowerInvariant();

                if (filter == "mine")
                    query = query.Where(t => t.AssignedToUserId == userId);
                else if (filter == "unassigned")
                    query = query.Where(t => t.AssignedToUserId == null);
                // else "all" -> bez warunku
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.Filter = filter;

            // do dropdownów
            ViewBag.Priorities = await _context.Priorities.OrderBy(p => p.Id).ToListAsync();

            if (IsAdmin())
            {
                var supports = await _userManager.GetUsersInRoleAsync(Roles.Support);
                ViewBag.SupportUsers = supports
                    .OrderBy(u => u.Email)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = (u.Email ?? u.UserName ?? u.Id)
                    })
                    .ToList();
            }

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
                .Include(t => t.AssignedTo)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            // dostęp: admin/support zawsze, user tylko swoje
            if (!IsAdminOrSupport())
            {
                if (ticket.UserId != CurrentUserId())
                    return Forbid();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,CategoryId")] Ticket ticket)
        {
            ticket.UserId = CurrentUserId();
            ticket.CreatedAt = DateTime.Now;
            ticket.Status = "Nowe";

            // domyślny priorytet: Średni
            var defaultPriority = await _context.Priorities.FirstOrDefaultAsync(p =>
                p.Name == "Średni" ||
                p.Name == "Sredni" ||
                p.Name.ToLower() == "średni" ||
                p.Name.ToLower() == "sredni");

            if (defaultPriority == null)
                defaultPriority = await _context.Priorities.OrderBy(p => p.Id).FirstOrDefaultAsync();

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
                _context.Tickets.Add(ticket);
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
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            // user może edytować tylko swoje
            if (!IsAdminOrSupport() && ticket.UserId != CurrentUserId())
                return Forbid();

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

            if (!IsAdminOrSupport() && ticket.UserId != CurrentUserId())
                return Forbid();

            if (ModelState.IsValid)
            {
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
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            if (!IsAdminOrSupport() && ticket.UserId != CurrentUserId())
                return Forbid();

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            if (!IsAdminOrSupport() && ticket.UserId != CurrentUserId())
                return Forbid();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- STATUS / PRIORITY: Admin zawsze, Support tylko jeśli przypisany ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Admin + "," + Roles.Support)]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            if (!CanEditStatusAndPriority(ticket))
                return Forbid();

            ticket.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Admin + "," + Roles.Support)]
        public async Task<IActionResult> ChangePriority(int id, int priorityId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            if (!CanEditStatusAndPriority(ticket))
                return Forbid();

            var exists = await _context.Priorities.AnyAsync(p => p.Id == priorityId);
            if (!exists) return BadRequest();

            ticket.PriorityId = priorityId;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // --- PRZYPISANIA ---

        // Admin przypisuje dowolnemu supportowi
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> AssignToSupport(int id, string supportUserId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            // upewnij się, że to user z rolą Support
            var user = await _userManager.FindByIdAsync(supportUserId);
            if (user == null) return BadRequest();

            var isSupport = await _userManager.IsInRoleAsync(user, Roles.Support);
            if (!isSupport) return BadRequest();

            ticket.AssignedToUserId = supportUserId;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Support bierze nieprzypisany ticket (lub zostawia, jeśli już jest jego)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Support)]
        public async Task<IActionResult> Take(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            var me = CurrentUserId();

            if (ticket.AssignedToUserId == null)
            {
                ticket.AssignedToUserId = me;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // jeśli przypisane do kogoś innego -> nie wolno
            if (ticket.AssignedToUserId != me)
                return Forbid();

            return RedirectToAction(nameof(Index));
        }
    }
}
