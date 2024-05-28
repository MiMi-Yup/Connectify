using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    [Index(nameof(IsDeleted))]
    public class AppUser : IdentityUser<Guid>
    {
        public DateTime LastActive { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string DisplayName { get; set; } = default!;

        public bool Locked { get; set; } = false;

        public virtual ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();

        public virtual ICollection<Contact> ContactsAsParticipantA { get; set; } = new List<Contact>();

        public virtual ICollection<Contact> ContactsAsParticipantB { get; set; } = new List<Contact>();

        public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

        [Required]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;
    }
}
