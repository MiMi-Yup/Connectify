using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.Contact
{
    public class Contact_SearchReq
    {
        [Required]
        [StringLength(255)]
        public string KeySearch { get; set; } = default!;
    }
}
