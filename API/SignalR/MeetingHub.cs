using API.Core.Contracts;
using API.DTOs.Responses.User;
using API.Entities;
using API.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.SignalR
{
    [Authorize]
    public class MeetingHub : Hub
    {
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _presenceTracker;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MeetingHub(IUnitOfWork unitOfWork,
            PresenceTracker presenceTracker,
            IHubContext<PresenceHub> presenceHub,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _presenceTracker = presenceTracker;
            _presenceHub = presenceHub;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            if (!Guid.TryParse(httpContext?.Request.Query["meetingId"].ToString(), out var meetingId))
                throw new HubException("Invalid query param");

            if (Context.User == null)
                throw new HubException("401");

            var group = await _unitOfWork.Meetings.GetByID(meetingId);
            if (group == null)
                throw new HubException("Metting doesn't exists");

            var userId = Context.User.GetUserId();
            var displayName = Context.User.GetDisplayName();

            var userConnection = new DTOs.SignalR.UserConnectionInfo(userId, displayName, group.Id);

            await _presenceTracker.UserConnected(userConnection, Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, group.Id.ToString());//khi user click vao room se join vao
            await AddConnectionToGroup(group.Id); // luu db DbSet<Connection> de khi disconnect biet

            var temp = await _unitOfWork.Users.GetByID(userId, trackChanges: false);
            await Clients.Group(group.Id.ToString()).SendAsync("UserOnlineInGroup", _mapper.Map<UserDto>(temp));

            var currentUsers = await _presenceTracker.GetOnlineUsers(group.Id);
            group.CountMember = currentUsers.Length;
            await _unitOfWork.SaveAsync();

            var currentConnections = await _presenceTracker.GetConnectionsForUser(userConnection);
            await _presenceHub.Clients.AllExcept(currentConnections).SendAsync("CountMemberInGroup",
                   new { groupId = group.Id, countMember = currentUsers.Length });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User == null)
                throw new HubException("401");

            var userId = Context.User.GetUserId();
            var displayName = Context.User.GetDisplayName();
            var group = await RemoveConnectionFromGroup();

            if (group == null)
                throw new HubException("Room doesn't exists");

            var isOffline = await _presenceTracker.UserDisconnected(new DTOs.SignalR.UserConnectionInfo(userId, displayName, group.Id), Context.ConnectionId);

            if (isOffline)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.Id.ToString());
                var temp = await _unitOfWork.Users.GetByID(userId, trackChanges: false);
                await Clients.Group(group.Id.ToString()).SendAsync("UserOfflineInGroup", _mapper.Map<UserDto>(temp));

                var currentUsers = await _presenceTracker.GetOnlineUsers(group.Id);

                group.CountMember = currentUsers.Length;
                await _unitOfWork.SaveAsync();

                await _presenceHub.Clients.All.SendAsync("CountMemberInGroup",
                       new { roomId = group.Id, countMember = currentUsers.Length });
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string content)
        {
            if (Context.User == null)
                throw new HubException("401");

            var userId = Context.User.GetUserId();
            var displayName = Context.User.GetDisplayName();
            var group = await _unitOfWork.Meetings.Find(item => item.Connections.Any(c => c.ConnectionId == Context.ConnectionId))
                .Include(item => item.Connections)
                .FirstOrDefaultAsync();

            if (group == null)
                throw new HubException("group == null");

            if (group.BlockedChat)
                throw new HubException("Chat has been blocked by host");

            await Clients.Group(group.Id.ToString()).SendAsync("NewMessage", new
            {
                SenderUserID = userId,
                SenderDisplayName = displayName,
                Content = content,
                MessageSent = DateTime.Now
            });
        }

        public async Task BlockChat(bool block)
        {
            //Check permission
            if (Context.User == null)
                throw new HubException("401");

            var group = await _unitOfWork.Meetings.Find(item => item.Connections.Any(c => c.ConnectionId == Context.ConnectionId)
                && item.Group.Administrator.Id == Context.User.GetUserId()
                && item.Group.Administrator.IsDeleted == false
                && item.Group.Administrator.Locked == false
                && item.Group.IsDeleted == false)
                .Include(item => item.Connections)
                .Include(item => item.Group)
                .ThenInclude(item => item.Administrator)
                .FirstOrDefaultAsync();

            if (group != null)
            {
                group.BlockedChat = block;
                await _unitOfWork.SaveAsync();
                await Clients.Group(group.Id.ToString()).SendAsync("OnBlockChat", new { block });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        public async Task MuteMicro(bool muteMicro)
        {
            var group = await _unitOfWork.Meetings.Find(item => item.Connections.Any(c => c.ConnectionId == Context.ConnectionId), trackChanges: false)
                .Include(item => item.Connections)
                .FirstOrDefaultAsync();

            if (group != null)
            {
                if (Context.User == null)
                    throw new HubException("401");

                await Clients.Group(group.Id.ToString()).SendAsync("OnMuteMicro", new
                {
                    userId = Context.User.GetUserId(),
                    mute = muteMicro
                });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        public async Task MuteAllMicro(Guid userId, bool muteMicro)
        {
            //Check permission
            if (Context.User == null)
                throw new HubException("401");

            var group = await _unitOfWork.Meetings.Find(item => item.Connections.Any(c => c.ConnectionId == Context.ConnectionId)
                && item.Group.Administrator.Id == Context.User.GetUserId()
                && item.Group.Administrator.IsDeleted == false
                && item.Group.Administrator.Locked == false
                && item.Group.IsDeleted == false, trackChanges: false)
                .Include(item => item.Connections)
                .Include(item => item.Group)
                .ThenInclude(item => item.Administrator)
                .FirstOrDefaultAsync();

            if (group != null)
            {
                if (!group.Connections.Any(item => item.User.Id == userId))
                    throw new HubException("user_id not in current group");

                await Clients.Group(group.Id.ToString()).SendAsync("OnMuteAllMicro", new
                {
                    userId = userId,
                    mute = muteMicro
                });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        public async Task MuteCamera(bool muteCamera)
        {
            var group = await _unitOfWork.Meetings.Find(item => item.Connections.Any(c => c.ConnectionId == Context.ConnectionId), trackChanges: false)
                .Include(item => item.Connections)
                .FirstOrDefaultAsync();
            if (group != null)
            {
                if (Context.User == null)
                    throw new HubException("401");

                await Clients.Group(group.Id.ToString()).SendAsync("OnMuteCamera", new
                {
                    userId = Context.User.GetUserId(),
                    mute = muteCamera
                });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        #region Private
        private async Task<Meeting?> RemoveConnectionFromGroup()
        {
            var group = await _unitOfWork.Meetings.Find(item => item.Connections.Any(c => c.ConnectionId == Context.ConnectionId))
                .Include(item => item.Connections)
                .FirstOrDefaultAsync();
            var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (connection == null)
                throw new HubException("Room doesn't exists");

            _unitOfWork.Connections.Delete(connection);

            await _unitOfWork.SaveAsync();

            return group;
        }

        private async Task<Meeting?> AddConnectionToGroup(Guid meetingId)
        {
            if (Context.User == null)
                throw new HubException("401");

            var group = await _unitOfWork.Meetings.Find(item => item.Id == meetingId)
                .Include(item => item.Connections)
                .FirstOrDefaultAsync();
            var user = await _unitOfWork.Users.GetByID(Context.User.GetUserId());

            var connection = new Connection
            {
                ConnectionId = Context.ConnectionId,
                User = user
            };

            if (group != null)
            {
                group.Connections.Add(connection);
            }

            await _unitOfWork.SaveAsync();

            return group;
        }
        #endregion
    }
}
