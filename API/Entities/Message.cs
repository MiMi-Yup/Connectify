using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Index(nameof(IsDeleted))]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Contact? Contact { get; set; }

        public Group? Group { get; set; }

        [Required]
        public AppUser Sender { get; set; } = default!;

        public File? Attachment { get; set; }

        public string? Content { get; set; } = default!;

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsRead { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
    }
}
