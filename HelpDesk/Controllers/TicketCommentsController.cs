using System;
using System.Linq;
using System.Threading.Tasks;
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

        private bool IsAdminOrSupport()
            => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Support);

        private async Task NotifyAdminsOwnerAndAssignedAsync(
            Ticket ticket,
            string title,
            string message,
            string? url,
            string? actorUserId)
        {
            var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);

            var recipientIds = admins.Select(a => a.Id).ToList();

            if (!string.IsNullOrWhiteSpace(ticket.UserId))
                recipientIds.Add(ticket.UserId);

            if (!string.IsNullOrWhiteSpace(ticket.AssignedToUserId))
                recipientIds.Add(ticket.AssignedToUserId);

            var finalRecipients = recipientIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .Where(id => actorUserId == null || id != actorUserId);

            foreach (var rid in finalRecipients)
            {
                _context.Notifications.Add(new Notification
                {
                    RecipientUserId = rid!,
                    Title = title,
                    Message = message,
                    Url = url,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ticketId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", "Tickets", new { id = ticketId });

            // Musimy mieć AssignedToUserId + UserId + Title
            var ticket = await _context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            // uprawnienia: Admin/Support mogą zawsze, user tylko do swojego ticketu
            if (!IsAdminOrSupport())
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

            // 🔔 Powiadomienia: Admin + Owner + Assigned Support (bez autora)
            var author = await _userManager.FindByIdAsync(currentUserId!);
            var authorLogin = author?.Email?.Split('@')[0] ?? "użytkownik";
            var url = $"/Tickets/Details/{ticketId}";

            await NotifyAdminsOwnerAndAssignedAsync(
                ticket,
                title: "Nowy komentarz",
                message: $"{authorLogin} dodał(a) komentarz do zgłoszenia: \"{ticket.Title}\"",
                url: url,
                actorUserId: currentUserId
            );

            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }
    }
}
