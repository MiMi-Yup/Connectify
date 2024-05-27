using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.User
{
    public class User_LoginReq
    {
        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 8)]
        public string Password { get; set; } = default!;
    }
}
