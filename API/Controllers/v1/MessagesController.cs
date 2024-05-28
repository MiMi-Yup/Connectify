using API.Atrributes;
using API.Core.Contracts;
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

        [HttpGet("{groupId}")]
        public async Task<IActionResult> ReadById(Guid groupId)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> NewGroup()
        {
            return Ok();
        }

        [Route("{groupId}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveGroup(Guid groupId)
        {
            return Ok();
        }

        [Route("{groupId}")]
        [HttpPut]
        public async Task<IActionResult> LeaveGroup(Guid groupId)
        {
            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Search()
        {
            return Ok();
        }
    }
}
