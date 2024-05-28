using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.Group
{
    public class Group_SearchReq
    {
        [Required]
        [StringLength(255)]
        public string KeySearch { get; set; } = default!;
    }
}
