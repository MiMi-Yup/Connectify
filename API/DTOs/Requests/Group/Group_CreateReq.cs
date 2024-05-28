using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.Group
{
    public class Group_CreateReq
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = default!;

        [Required]
        [MinLength(1)]
        public ICollection<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
