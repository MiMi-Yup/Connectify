using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.Group
{
    public class Group_RemoveMembersReq
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        [MinLength(1)]
        public ICollection<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
