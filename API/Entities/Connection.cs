using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Connection
    {
        [Key]
        [Required]
        public string ConnectionId { get; set; } = default!;

        [Required]
        public AppUser User { get; set; } = default!;
    }
}
