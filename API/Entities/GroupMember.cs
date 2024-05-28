using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Index(nameof(IsDeleted))]
    public class GroupMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public Group Group { get; set; } = default!;

        [Required]
        public AppUser User { get; set; } = default!;

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime UpdatedDate { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
    }
}
