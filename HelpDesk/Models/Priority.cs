using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Priority
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
