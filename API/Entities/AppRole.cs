using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class AppRole : IdentityRole<Guid>
    {
        [Required]
        public virtual ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
    }
}
