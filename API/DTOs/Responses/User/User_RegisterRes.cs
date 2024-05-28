namespace API.DTOs.Responses.User
{
    public class User_RegisterRes
    {
        public UserDto User { get; set; } = default!;

        public string AccessToken { get; set; } = default!;

        public string RefreshToken { get; set; } = default!;
    }
}
