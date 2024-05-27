using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = default!;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime UpdatedDate { get; set; }

        public virtual ICollection<Connection> Connections { get; set; } = new List<Connection>();

        [Required]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
    }
}
