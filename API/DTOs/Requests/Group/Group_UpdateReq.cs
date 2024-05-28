using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.Group
{
    public class Group_UpdateReq
    {
        [StringLength(255)]
        public string Name { get; set; } = default!;
    }
}
