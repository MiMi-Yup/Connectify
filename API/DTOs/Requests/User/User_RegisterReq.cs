using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.User
{
    public class User_RegisterReq
    {
        [Required]
        [StringLength(255)]
        public string UserName { get; set; } = default!;

        [StringLength(255)]
        public string? DisplayName { get; set; } = default!;

        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 8)]
        public string Password { get; set; } = default!;
    }
}
