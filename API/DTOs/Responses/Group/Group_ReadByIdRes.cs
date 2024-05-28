using API.DTOs.Responses.User;

namespace API.DTOs.Responses.Group
{
    public class Group_ReadByIdRes
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;

        public ICollection<UserDto> Members { get; set; } = new List<UserDto>();
    }
}
