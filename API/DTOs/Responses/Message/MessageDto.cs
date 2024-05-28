using API.DTOs.Responses.File;
using API.DTOs.Responses.User;

namespace API.DTOs.Responses.Message
{
    public class MessageDto
    {
        public Guid Id { get; set; }

        public UserDto Sender { get; set; } = default!;

        public FileDto? Attachment { get; set; }

        public string? Content { get; set; } = default!;

        public DateTime Timestamp { get; set; }

        public bool IsRead { get; set; }
    }
}
