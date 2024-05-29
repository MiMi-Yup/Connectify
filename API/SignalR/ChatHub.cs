using API.Core.Contracts;
using API.DTOs.Requests.Message;
using API.DTOs.SignalR;
using API.Entities;
using API.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.SignalR
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _presenceTracker;
        private bool IsContact = false;

        public ChatHub(ILogger logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IHubContext<PresenceHub> presenceHub,
            PresenceTracker presenceTracker)
        {
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _presenceHub = presenceHub;
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User == null)
                throw new HubException("401");

            var userId = Context.User.GetUserId();
            var displayName = Context.User.GetDisplayName();

            var user = await _unitOfWork.Users.Find(item => item.Id == userId && item.IsDeleted == false && item.Locked == false)
                .Include(item => item.ContactsAsParticipantA)
                .Include(item => item.ContactsAsParticipantB)
                .Include(item => item.GroupMembers).ThenInclude(item => item.Group)
                .FirstOrDefaultAsync();

            var contacts = user.ContactsAsParticipantA.Where(item => item.IsDeleted == false)
                .Union(user.ContactsAsParticipantB.Where(item => item.IsDeleted == false))
                .Distinct()
                .ToList();

            var groups = user.GroupMembers.Where(item => item.IsDeleted == false)
                .Select(item => item.Group)
                .Distinct()
                .ToList();

            Connection connection = new Connection { ConnectionId = Context.ConnectionId, User = user };
            await _unitOfWork.Connections.Insert(connection);

            var userConnection = new UserConnectionInfo(userId, displayName, userId);
            await _presenceTracker.UserConnected(userConnection, Context.ConnectionId);

            foreach (var item in contacts)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, item.Id.ToString());
                item.Connections.Add(connection);
            }

            foreach (var item in groups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, item.Id.ToString());
                item.Connections.Add(connection);
            }

            await _unitOfWork.SaveAsync();

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User == null)
                throw new HubException("401");

            var userId = Context.User.GetUserId();
            var displayName = Context.User.GetDisplayName();

            var user = await _unitOfWork.Users.Find(item => item.Id == userId && item.IsDeleted == false && item.Locked == false)
                .Include(item => item.ContactsAsParticipantA)
                .Include(item => item.ContactsAsParticipantB)
                .Include(item => item.GroupMembers).ThenInclude(item => item.Group)
                .FirstOrDefaultAsync();

            var contacts = user.ContactsAsParticipantA.Where(item => item.IsDeleted == false)
                .Union(user.ContactsAsParticipantB.Where(item => item.IsDeleted == false))
                .Distinct()
                .ToList();

            var groups = user.GroupMembers.Where(item => item.IsDeleted == false)
                .Select(item => item.Group)
                .Distinct()
                .ToList();

            var connection = await _unitOfWork.Connections.Find(item => item.ConnectionId == Context.ConnectionId
                && item.User.IsDeleted == false
                && item.User.Locked == false)
                .Include(item => item.User)
                .FirstOrDefaultAsync();

            if (connection != null)
                _unitOfWork.Connections.Delete(connection);

            var userConnection = new UserConnectionInfo(userId, displayName, userId);
            var isOffline = await _presenceTracker.UserDisconnected(userConnection, Context.ConnectionId);

            if (isOffline)
            {
                foreach (var item in contacts)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, item.Id.ToString());
                }

                foreach (var item in groups)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, item.Id.ToString());
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(Message_CreateReq obj)
        {
            if (Context.User == null)
                throw new HubException("401");

            if (obj.GroupId == null || obj.ContactId == null)
                throw new HubException("Invalid payload");

            var userId = Context.User.GetUserId();
            var displayName = Context.User.GetDisplayName();

            var user = await _unitOfWork.Users.GetByID(userId);
            var message = new Message { Content = obj.Content, Timestamp = DateTime.Now, Sender = user };

            if (obj.ContactId != null)
            {
                var contact = await _unitOfWork.Contacts.GetByID(obj.ContactId);
                contact?.Messages.Add(message);
            }
            else if (obj.GroupId != null)
            {
                var group = await _unitOfWork.Groups.GetByID(obj.ContactId);
                group?.Messages.Add(message);
            }

            await _unitOfWork.SaveAsync();

            await Clients.OthersInGroup((obj.GroupId ?? obj.ContactId.Value).ToString()).SendAsync("NewMessage", new
            {
                message.Id,
                SenderUserID = userId,
                SenderDisplayName = displayName,
                Content = obj.Content,
                MessageSent = DateTime.Now
            });
        }

        public async Task IsRead(Guid messageId)
        {
            if (Context.User == null)
                throw new HubException("401");

            var message = await _unitOfWork.Messages.GetByID(messageId);
            if (message != null)
                message.IsRead = true;

            await _unitOfWork.SaveAsync();
        }
    }
}
