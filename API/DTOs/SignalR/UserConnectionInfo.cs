namespace API.DTOs.SignalR
{
    public class UserConnectionInfo
    {
        public UserConnectionInfo(Guid userId, string displayName, Guid groupId)
        {
            UserId = userId;
            DisplayName = displayName;
            GroupId = groupId;
        }

        public Guid UserId { get; set; }

        public string DisplayName { get; set; }

        public Guid GroupId { get; set; }
    }
}
