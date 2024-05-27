using API.Core.Contracts;
using API.DTOs.Requests.User;
using API.DTOs.Responses.User;
using API.Entities;
using API.Extensions;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.v1
{
    [ApiController]
    [Route("/api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ContactsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public ContactsController(ILogger<ContactsController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ITokenService tokenService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }
    }
}
