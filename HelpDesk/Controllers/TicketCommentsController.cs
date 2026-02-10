using HelpDesk.Constants;
using HelpDesk.Data;
using HelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class TicketCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TicketCommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ticketId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", "Tickets", new { id = ticketId });

            var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            // uprawnienia: Admin/Support mogą zawsze, user tylko do swojego ticketu
            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole(Roles.Admin) && !User.IsInRole(Roles.Support))
            {
                if (ticket.UserId != currentUserId)
                    return Forbid();
            }

            var comment = new TicketComment
            {
                TicketId = ticketId,
                Content = content.Trim(),
                UserId = currentUserId!
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }
    }
}
