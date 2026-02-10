using HelpDesk.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.ViewComponents
{
    public class NotificationsBellViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsBellViewComponent(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return View(0);

            var userId = _userManager.GetUserId(HttpContext.User);

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.RecipientUserId == userId && !n.IsRead);

            return View(unreadCount);
        }
    }
}
