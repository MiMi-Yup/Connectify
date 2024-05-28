namespace API.DTOs.Responses.User
{
    public class User_RenewTokenRes
    {
        public string AccessToken { get; set; } = default!;

        public string? RefreshToken { get; set; }
    }
}
