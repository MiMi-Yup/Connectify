using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests.Meeting
{
    public class Meeting_CreateReq
    {
        [Required]
        public Guid GroupId { get; set; }
    }
}
