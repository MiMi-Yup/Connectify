using API.Atrributes;
using API.Core.Contracts;
using API.DTOs.Requests;
using API.DTOs.Responses;
using API.DTOs.Responses.Message;
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
    public class MessagesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public MessagesController(ILogger<MessagesController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ITokenService tokenService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType<APIPagingRes<MessageDto>>(200)]
        public async Task<IActionResult> Contact(APIPagingReq<Guid> obj)
        {
            var query = _unitOfWork.Messages.Find(item => item.IsDeleted == false
                && item.Contact != null
                && item.Contact.Id == obj.Data)
                .Include(item => item.Contact)
                .Include(item => item.Sender)
                .Include(item => item.Attachment);

            var totalCount = await query.CountAsync();
            var messages = await query.OrderByDescending(item => item.Timestamp)
                .Skip(obj.PageIndex * obj.PageSize)
                .Take(obj.PageSize)
                .ToListAsync();

            return Ok(new APIPagingRes<MessageDto>
            {
                Data = messages.Select(_mapper.Map<MessageDto>).ToList(),
                TotalCount = totalCount,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize
            });
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType<APIPagingRes<MessageDto>>(200)]
        public async Task<IActionResult> Group(APIPagingReq<Guid> obj)
        {
            var query = _unitOfWork.Messages.Find(item => item.IsDeleted == false
                && item.Group != null
                && item.Group.Id == obj.Data)
                .Include(item => item.Group)
                .Include(item => item.Sender)
                .Include(item => item.Attachment);

            var totalCount = await query.CountAsync();
            var messages = await query.OrderByDescending(item => item.Timestamp)
                .Skip(obj.PageIndex * obj.PageSize)
                .Take(obj.PageSize)
                .ToListAsync();

            return Ok(new APIPagingRes<MessageDto>
            {
                Data = messages.Select(_mapper.Map<MessageDto>).ToList(),
                TotalCount = totalCount,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize
            });
        }
    }
}
