using API.DTOs.Responses.User;

namespace API.DTOs.Responses.Contact
{
    public class Contact_SearchRes
    {
        public Guid Id { get; set; }

        public UserDto User { get; set; } = default!;
    }
}
