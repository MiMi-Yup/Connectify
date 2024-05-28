using API.Atrributes;
using API.Core.Contracts;
using API.DTOs.Requests;
using API.DTOs.Requests.Contact;
using API.DTOs.Responses;
using API.DTOs.Responses.Contact;
using API.DTOs.Responses.User;
using API.Entities;
using API.Extensions;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.v1
{
    [ApiController]
    [Route("/api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [ServiceFilter(typeof(LogUserActivityAttribute))]
    public class ContactsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ContactsController(ILogger<ContactsController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /*[HttpGet("{contactId}")]
        public async Task<IActionResult> ReadById(Guid contactId)
        {
            var contact = await _unitOfWork.Contacts.Find(item => item.Id == contactId 
                && item.IsDeleted == false 
                && item.ParticipantA.IsDeleted == false 
                && item.ParticipantB.IsDeleted == false, trackChanges: false)
                .Include(item => item.ParticipantA)
                .Include(item => item.ParticipantB)
                .FirstOrDefaultAsync();
        }*/

        [HttpPost]
        [ProducesResponseType<APIRes<Guid>>(200)]
        public async Task<IActionResult> Create(Contact_CreateReq obj)
        {
            var userAId = User.GetUserId();

            var userA = await _unitOfWork.Users.GetByID(userAId);
            if (userA == null)
                return Unauthorized();

            var userB = await _unitOfWork.Users.Find(item => item.IsDeleted == false
                && item.Locked == false
                && item.PhoneNumber == obj.PhoneNumber).FirstOrDefaultAsync();
            if (userB == null)
                return NotFound();

            var contact = new Contact
            {
                ParticipantA = userA,
                ParticipantB = userB,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _unitOfWork.Contacts.Insert(contact);

            await _unitOfWork.SaveAsync();

            return Ok(new APIRes<Guid>
            {
                Data = contact.Id
            });
        }

        [Route("{contactId}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid contactId)
        {
            var userAId = User.GetUserId();

            var contact = await _unitOfWork.Contacts.Find(item => item.Id == contactId
                && item.IsDeleted == false
                && (item.ParticipantAId == userAId || item.ParticipantBId == userAId))
                .Include(item => item.ParticipantA)
                .Include(item => item.ParticipantB)
                .Include(item => item.Messages)
                .FirstOrDefaultAsync();

            if (contact == null)
                return NotFound();

            contact.IsDeleted = true;
            contact.UpdatedDate = DateTime.Now;
            foreach (var item in contact.Messages)
            {
                item.IsDeleted = true;
            }

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType<APIPagingRes<Contact_SearchRes>>(200)]
        public async Task<IActionResult> Search(APIPagingReq<Contact_SearchReq> obj)
        {
            if (obj.Data == null)
                return BadRequest();

            var userAId = User.GetUserId();

            var query = _unitOfWork.Contacts.Find(item => item.IsDeleted == false
                && item.ParticipantA.IsDeleted == false && item.ParticipantA.Locked == false
                && item.ParticipantB.IsDeleted == false && item.ParticipantB.Locked == false
                && (item.ParticipantAId == userAId || item.ParticipantBId == userAId)
                && ((obj.Data.KeySearch.Length == 10 && item.ParticipantA.PhoneNumber == obj.Data.KeySearch)
                    || (obj.Data.KeySearch.Length == 10 && item.ParticipantB.PhoneNumber == obj.Data.KeySearch)
                    || item.ParticipantA.DisplayName.ToLower().Contains(obj.Data.KeySearch.ToLower())
                    || item.ParticipantB.DisplayName.ToLower().Contains(obj.Data.KeySearch.ToLower())), trackChanges: false)
                .Include(item => item.ParticipantA)
                .Include(item => item.ParticipantB)
                .Include(item => item.Messages);

            var totalCount = await query.CountAsync();

            var contacts = await query.OrderByDescending(item => item.Messages.Max(m => m.Timestamp))
                .Skip(obj.PageIndex * obj.PageSize).Take(obj.PageSize)
                .ToListAsync();

            var result = contacts.Select(item =>
            {
                if (item.ParticipantAId == userAId)
                    return new Contact_SearchRes
                    {
                        Id = item.Id,
                        User = _mapper.Map<UserDto>(item.ParticipantB)
                    };
                else
                    return new Contact_SearchRes
                    {
                        Id = item.Id,
                        User = _mapper.Map<UserDto>(item.ParticipantA)
                    };
            }).ToList();

            return Ok(new APIPagingRes<Contact_SearchRes>
            {
                TotalCount = totalCount,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                Data = result
            });
        }
    }
}
