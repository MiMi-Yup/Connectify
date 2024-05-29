namespace API.DTOs.Requests.Message
{
    public class Message_CreateReq
    {
        public Guid? ContactId { get; set; }

        public Guid? GroupId { get; set; }

        public string Content { get; set; } = default!;
    }
}
