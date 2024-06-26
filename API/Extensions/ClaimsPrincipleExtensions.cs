﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            return Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(JwtRegisteredClaimNames.Name);
        }

        public static bool IsRefreshToken(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("type") == "refresh_token";
        }
    }
}
