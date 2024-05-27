using API.Core.Contracts;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Core.Implements
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private UserManager<AppUser> _userManager;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public async Task<string> CreateTokenAsync(AppUser appUser, TimeSpan? offsetTime = null, string type = "access_token")
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, appUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, appUser.UserName),
                new Claim(JwtRegisteredClaimNames.Name, appUser.DisplayName),
                new Claim(nameof(type), type)
            };

            var roles = await _userManager.GetRolesAsync(appUser);

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = creds
            };

            if (offsetTime != null)
                tokenDescriptor.Expires = DateTime.Now.Add(offsetTime.Value);
            else
                tokenDescriptor.Expires = DateTime.Now.AddDays(1);

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public bool RenewRefreshToken(string token, TimeSpan offsetRenewRefreshToken)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = false,
                ValidateAudience = false
            };

            SecurityToken validatedToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, validationParameters, out validatedToken);

            if (validatedToken.ValidTo > DateTime.Now.Add(offsetRenewRefreshToken))
                return true;
            else
                return false;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
