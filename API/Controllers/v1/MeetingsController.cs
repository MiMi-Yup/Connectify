using API.Atrributes;
using API.Core.Contracts;
using API.DTOs.Requests.Meeting;
using API.DTOs.Responses;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1
{
    [ApiController]
    [Route("/api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [ServiceFilter(typeof(LogUserActivityAttribute))]
    public class MeetingsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public MeetingsController(ILogger<MeetingsController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [ProducesResponseType<APIRes<Guid>>(200)]
        public async Task<IActionResult> Create(Meeting_CreateReq obj)
        {
            var group = await _unitOfWork.Groups.GetByID(obj.GroupId);
            if (group == null)
                return NotFound();

            var meeting = new Entities.Meeting { Group = group };
            await _unitOfWork.Meetings.Insert(meeting);
            await _unitOfWork.SaveAsync();

            return Ok(new APIRes<Guid> { Data = meeting.Id });
        }
    }
}
