using API.Atrributes;
using API.Core.Contracts;
using API.DTOs.Requests.User;
using API.DTOs.Responses;
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
    [ServiceFilter(typeof(LogUserActivityAttribute))]
    public class UsersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public UsersController(ILogger<UsersController> logger,
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IMapper mapper,
            ITokenService tokenService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType<APIRes<User_RegisterRes>>(200)]
        public async Task<IActionResult> Register(User_RegisterReq obj)
        {
            var user = _mapper.Map<AppUser>(obj);

            var foundUser = await _unitOfWork.Users.Find(item => (item.PhoneNumber == user.PhoneNumber || item.UserName == user.UserName)
                && item.IsDeleted == false
                && item.Locked == false, trackChanges: false).FirstOrDefaultAsync();
            if (foundUser != null)
                return NotFound($"Phone {obj.PhoneNumber} exists");

            var registerResult = await _userManager.CreateAsync(user, obj.Password);
            if (!registerResult.Succeeded)
                return BadRequest(registerResult.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            var login = await _signInManager.CheckPasswordSignInAsync(user, obj.Password, false);
            if (!login.Succeeded)
                return Unauthorized();

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(new APIRes<User_RegisterRes>
            {
                Data = new User_RegisterRes
                {
                    User = userDto,
                    AccessToken = await _tokenService.CreateTokenAsync(user, TimeSpan.FromMinutes(10), "access_token"),
                    RefreshToken = await _tokenService.CreateTokenAsync(user, TimeSpan.FromHours(2), "refresh_token")
                }
            });
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType<APIRes<User_LoginRes>>(200)]
        public async Task<IActionResult> Login(User_LoginReq obj)
        {
            var user = await _unitOfWork.Users.Find(item => item.PhoneNumber == obj.PhoneNumber && item.IsDeleted == false && item.Locked == false).FirstOrDefaultAsync();
            if (user == null)
                return NotFound($"Phone {obj.PhoneNumber} doesn't exists");

            var login = await _signInManager.CheckPasswordSignInAsync(user, obj.Password, false);
            if (!login.Succeeded)
                return Unauthorized();

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(new APIRes<User_LoginRes>
            {
                Data = new User_LoginRes
                {
                    User = userDto,
                    AccessToken = await _tokenService.CreateTokenAsync(user, TimeSpan.FromMinutes(10), "access_token"),
                    RefreshToken = await _tokenService.CreateTokenAsync(user, TimeSpan.FromHours(2), "refresh_token")
                }
            });
        }

        [Route("[action]")]
        [HttpPost]
        [Authorize]
        [ProducesResponseType<APIRes<User_RenewTokenRes>>(200)]
        public async Task<IActionResult> RenewToken()
        {
            if (!User.IsRefreshToken())
                return Unauthorized();

            var userId = User.GetUserId();

            var user = await _unitOfWork.Users.Find(item => item.Id == userId && item.IsDeleted == false && item.Locked == false, trackChanges: false).FirstOrDefaultAsync();
            if (user == null)
                return BadRequest("User invalid");

            try
            {
                string? accessToken = await HttpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var renew = _tokenService.RenewRefreshToken(accessToken, TimeSpan.FromMinutes(30));
                    if (renew)
                        return Ok(new APIRes<User_RenewTokenRes>
                        {
                            Data = new User_RenewTokenRes
                            {
                                AccessToken = await _tokenService.CreateTokenAsync(user, TimeSpan.FromMinutes(10), "access_token"),
                                RefreshToken = await _tokenService.CreateTokenAsync(user, TimeSpan.FromHours(2), "refresh_token")
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }

            return Ok(new APIRes<User_RenewTokenRes>
            {
                Data = new User_RenewTokenRes
                {
                    AccessToken = await _tokenService.CreateTokenAsync(user, TimeSpan.FromMinutes(10), "access_token")
                }
            });
        }

        [Route("[action]")]
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            var userId = User.GetUserId();

            var user = await _unitOfWork.Users.Find(item => item.Id == userId && item.IsDeleted == false && item.Locked == false)
                .Include(item => item.ContactsAsParticipantA)
                .Include(item => item.ContactsAsParticipantB)
                .Include(item => item.GroupMembers)
                .FirstOrDefaultAsync();
            if (user == null)
                return BadRequest("User invalid");

            user.IsDeleted = true;
            user.Locked = true;

            foreach (var item in user.ContactsAsParticipantA)
            {
                item.IsDeleted = true;
            }

            foreach (var item in user.ContactsAsParticipantB)
            {
                item.IsDeleted = true;
            }

            foreach (var item in user.GroupMembers)
            {
                item.IsDeleted = true;
            }

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [Route("[action]")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdatePassword(User_UpdatePasswordReq obj)
        {
            var userId = User.GetUserId();

            var user = await _unitOfWork.Users.Find(item => item.Id == userId && item.IsDeleted == false && item.Locked == false, trackChanges: false)
                .FirstOrDefaultAsync();
            if (user == null)
                return BadRequest("User invalid");

            var changePassword = await _userManager.ChangePasswordAsync(user, obj.OldPassword, obj.NewPassword);
            if (!changePassword.Succeeded)
                return BadRequest(changePassword.Errors);

            return Ok();
        }
    }
}
