namespace API.DTOs.Responses.User
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = default!;

        public string DisplayName { get; set; } = default!;
    }
}
