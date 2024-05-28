using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.Contact
{
    public class Contact_CreateReq
    {
        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = default!;
    }
}
