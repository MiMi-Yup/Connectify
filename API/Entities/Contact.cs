using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Index(nameof(IsDeleted))]
    public class Contact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ParticipantAId { get; set; }

        [Required]
        public Guid ParticipantBId { get; set; }

        [Required]
        public AppUser ParticipantA { get; set; } = default!;

        [Required]
        public AppUser ParticipantB { get; set; } = default!;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime UpdatedDate { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        public Connection? Connection { get; set; }
    }
}
