using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string RecipientUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(RecipientUserId))]
        public IdentityUser? RecipientUser { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Url { get; set; } // np. /Tickets/Details/5

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
