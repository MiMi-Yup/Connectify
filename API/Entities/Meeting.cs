using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    public class Meeting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [DefaultValue(0)]
        public int CountMember { get; set; }

        [DefaultValue(false)]
        public bool BlockedChat { get; set; }

        public ICollection<Connection> Connections { get; set; } = new List<Connection>();

        [Required]
        public Group Group { get; set; } = default!;
    }
}
