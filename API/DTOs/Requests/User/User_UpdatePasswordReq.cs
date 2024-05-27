using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.User
{
    public class User_UpdatePasswordReq
    {
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 8)]
        public string OldPassword { get; set; } = default!;

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 8)]
        public string NewPassword { get; set; } = default!;
    }
}
