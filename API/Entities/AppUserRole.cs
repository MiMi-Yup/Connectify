using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class AppUserRole : IdentityUserRole<Guid>
    {
        [Required]
        public AppUser User { get; set; } = default!;

        [Required]
        public AppRole Role { get; set; } = default!;
    }
}
