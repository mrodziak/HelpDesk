using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models;

namespace HelpDesk.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Priority> Priorities { get; set; } = default!;
        public DbSet<Ticket> Tickets { get; set; } = default!;

    }
}
