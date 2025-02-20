using GigFinder.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace GigFinder.Utils
{
    public static class Jwt
    {
        public const string PRIVATE_KEY = "61e6ac9f0e7ac2c9648805ccb06728486f663180ae0375a633d810421d5adf0e5126cfb4dc78a5997e953df14dd74751cd0bc0111a5a7ba1b8757431fb7780a634243787e5fac11f7c599af56c15080d93e28b76d6a86851a8966e5228ca6115d8b948d796410c6dd7d56fe1ce0dd086fc38a31a953caedfe5b9882e7d9168f488036677cc31d9b7d7fd462726e80ad55238da2223b9c84450733cfa4e4438583f243a0bd954f2ab8b2b166a6e79c620d7e6e61e2b379a6a621cad45cff81af3bb3ad1e668376240a05767a2f23f4d5a0f73aa0d69f511dab3cd0188b3ea1230c713d4e542887a7f34507377f27bc77ddef44188aa0169a1e81a671d86cc5792";

        public static string GenerateUserJwt(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(PRIVATE_KEY)); // Replace with a secure key
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                 new Claim("user_id", user.id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "gigfinder",
                audience: "gigfinder",
                claims: claims,
                expires: DateTime.UtcNow.AddYears(1),  // Token expiry time set to 1 years
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}