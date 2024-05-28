using API.Atrributes;
using API.Core.Contracts;
using API.DTOs.Requests;
using API.DTOs.Requests.Group;
using API.DTOs.Responses;
using API.DTOs.Responses.Group;
using API.DTOs.Responses.User;
using API.Entities;
using API.Extensions;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace API.Controllers.v1
{
    [ApiController]
    [Route("/api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [ServiceFilter(typeof(LogUserActivityAttribute))]
    public class GroupsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GroupsController(ILogger<GroupsController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("{groupId}")]
        [ProducesResponseType<APIRes<Group_ReadByIdRes>>(200)]
        public async Task<IActionResult> ReadById(Guid groupId)
        {
            var userId = User.GetUserId();

            var group = await _unitOfWork.Groups.Find(item => item.Id == groupId
                && item.IsDeleted == false
                && item.Administrator.IsDeleted == false
                && item.Administrator.Locked == false
                && item.Members.Any(m => m.IsDeleted == false
                    && m.User.Id == userId
                    && m.User.IsDeleted == false
                    && m.User.Locked == false), trackChanges: false)
                .Include(item => item.Administrator)
                .Include(item => item.Members).ThenInclude(item => item.User)
                .FirstOrDefaultAsync();
            if (group == null)
                return NotFound();

            return Ok(new APIRes<Group_ReadByIdRes>
            {
                Data = new Group_ReadByIdRes
                {
                    Id = group.Id,
                    Name = group.Name,
                    Members = group.Members.Select(_mapper.Map<UserDto>).ToList()
                }
            });
        }

        [HttpPost]
        [ProducesResponseType<APIRes<Guid>>(200)]
        public async Task<IActionResult> Create(Group_CreateReq obj)
        {
            var userId = User.GetUserId();

            var user = await _unitOfWork.Users.GetByID(userId);
            if (user == null)
                return Unauthorized();

            var existGroup = _unitOfWork.Groups.Find(item => item.Name == obj.Name
                && item.Administrator == user
                && item.Administrator.IsDeleted == false
                && item.Administrator.Locked == false
                && item.IsDeleted == false, trackChanges: false)
                .Include(item => item.Administrator)
                .FirstOrDefaultAsync();
            if (existGroup != null)
                return Conflict();

            var members = await _unitOfWork.Users.Find(item => obj.UserIds.Contains(item.Id)
                && item.IsDeleted == false
                && item.Locked == false)
                .Include(item => item.ContactsAsParticipantA)
                .Include(item => item.ContactsAsParticipantB)
                .ToListAsync();
            if (members.Count != obj.UserIds.Count)
                return BadRequest(new APIRes<object>
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    ErrorMessage = "Some users not found"
                });

            var contactCount = await _unitOfWork.Contacts.Find(c => c.IsDeleted == false
                && ((c.ParticipantAId == userId && members.Contains(c.ParticipantB))
                || (members.Contains(c.ParticipantA) && c.ParticipantBId == userId)), trackChanges: false)
                .Include(item => item.ParticipantA)
                .Include(item => item.ParticipantB)
                .CountAsync();
            if (contactCount != obj.UserIds.Count)
                return BadRequest(new APIRes<object>
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = "Add contacts before adding them to the group"
                });

            var group = new Group
            {
                Administrator = user,
                Name = obj.Name,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Members = members.Select(item => new GroupMember
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    User = item
                }).ToList(),
            };

            await _unitOfWork.Groups.Insert(group);

            return Ok(new APIRes<Guid>
            {
                Data = group.Id
            });
        }

        [Route("{groupId}")]
        [HttpPut]
        public async Task<IActionResult> Update(Guid groupId, Group_UpdateReq obj)
        {
            var userId = User.GetUserId();
            var group = await _unitOfWork.Groups.Find(item => item.IsDeleted == false 
                && item.Administrator.Id == userId 
                && item.Administrator.IsDeleted == false 
                && item.Administrator.Locked == false)
                .Include(item => item.Administrator)
                .FirstOrDefaultAsync();
            if (group == null) 
                return NotFound();

            group.Name = obj.Name;

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [Route("[action]")]
        [HttpPut]
        public async Task<IActionResult> AddMembers(Group_AddMembersReq obj)
        {
            var userId = User.GetUserId();

            var group = await _unitOfWork.Groups.GetByID(obj.GroupId);
            var members = await _unitOfWork.Users.Find(item => item.IsDeleted == false && item.Locked == false && obj.UserIds.Contains(item.Id)).ToListAsync();
            if (members.Count != obj.UserIds.Count)
                return BadRequest(new APIRes<object>
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    ErrorMessage = "Some users not found"
                });

            var contactCount = await _unitOfWork.Contacts.Find(c => c.IsDeleted == false
                && ((c.ParticipantAId == userId && members.Contains(c.ParticipantB))
                || (members.Contains(c.ParticipantA) && c.ParticipantBId == userId)), trackChanges: false)
                .Include(item => item.ParticipantA)
                .Include(item => item.ParticipantB)
                .CountAsync();
            if (contactCount != obj.UserIds.Count)
                return BadRequest(new APIRes<object>
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = "Add contacts before adding them to the group"
                });

            foreach (var user in members)
            {
                await _unitOfWork.GroupMembers.Insert(new GroupMember
                {
                    GroupId = obj.GroupId,
                    User = user,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [Route("[action]/{groupId}")]
        [HttpPut]
        public async Task<IActionResult> Leave(Guid groupId)
        {
            var userId = User.GetUserId();

            var groupMember = await _unitOfWork.GroupMembers.Find(item => item.IsDeleted == false
                && item.Group.Id == groupId
                && item.User.Id == userId
                && item.User.IsDeleted == false
                && item.User.Locked == false)
                .Include(item => item.Group)
                .Include(item => item.User)
                .FirstOrDefaultAsync();
            if (groupMember == null)
                return NotFound();

            groupMember.IsDeleted = false;
            groupMember.UpdatedDate = DateTime.Now;

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [Route("[action]")]
        [HttpPut]
        public async Task<IActionResult> RemoveMembers(Group_RemoveMembersReq obj)
        {
            var userId = User.GetUserId();

            var group = await _unitOfWork.Groups.Find(item => item.IsDeleted == false
                && item.Administrator.Id == userId
                && item.Administrator.IsDeleted == false
                && item.Administrator.Locked == false)
                .Include(item => item.Administrator)
                .FirstOrDefaultAsync();
            if (group == null)
                return NotFound();

            var members = await _unitOfWork.GroupMembers.Find(item => item.GroupId == obj.GroupId 
                && obj.UserIds.Contains(item.User.Id))
                .Include(item => item.User)
                .ToListAsync();
            foreach (var member in members)
            {
                member.IsDeleted = true;
                member.UpdatedDate = DateTime.Now;
            }

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [Route("{groupId}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid groupId)
        {
            var userId = User.GetUserId();

            var group = await _unitOfWork.Groups.Find(item => item.Id == groupId
                && item.IsDeleted == false
                && item.Administrator.Id == userId
                && item.Administrator.IsDeleted == false)
                .Include(item => item.Administrator)
                .Include(item => item.Members)
                .Include(item => item.Messages)
                .FirstOrDefaultAsync();

            if (group == null)
                return NotFound();

            group.IsDeleted = true;
            group.UpdatedDate = DateTime.Now;

            foreach (var item in group.Messages)
            {
                item.IsDeleted = true;
            }

            foreach (var item in group.Members)
            {
                item.IsDeleted = true;
                item.UpdatedDate = DateTime.Now;
            }

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType<APIPagingRes<Group_SearchRes>>(200)]
        public async Task<IActionResult> Search(APIPagingReq<Group_SearchReq> obj)
        {
            if (obj.Data == null)
                return BadRequest();

            var userAId = User.GetUserId();

            var query = _unitOfWork.Groups.Find(item => item.IsDeleted == false
                && item.Name.ToLower().Contains(obj.Data.KeySearch.ToLower()), trackChanges: false)
                .Include(item => item.Messages)
                .Include(item => item.Members);

            var totalCount = await query.CountAsync();

            var groups = await query.OrderByDescending(item => item.Messages.Max(m => m.Timestamp))
                .Skip(obj.PageIndex * obj.PageSize).Take(obj.PageSize)
                .ToListAsync();

            var result = groups.Select(item => new Group_SearchRes
            {
                Id = item.Id,
                MemberCount = item.Members.Count
            }).ToList();

            return Ok(new APIPagingRes<Group_SearchRes>
            {
                TotalCount = totalCount,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                Data = result
            });
        }
    }
}
