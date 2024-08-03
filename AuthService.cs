using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Models;
using Microsoft.IdentityModel.Tokens;

namespace api
{
    public class AuthService()
    {
        private readonly JwtSecurityTokenHandler tokenHandler = new();

        /// <summary>
        /// Generate a valid JWT token that expires in 2 hours
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_config"></param>
        /// <returns>Return a string of JWT token</returns>
        public string GenerateJwtToken(User _user, IConfiguration _config)
        {
            SecurityTokenDescriptor tokenDescriptor =
                new()
                {
                    Subject = new ClaimsIdentity(
                        [
                            new Claim(ClaimTypes.Name, _user.Username),
                            new Claim(ClaimTypes.Email, _user.Email)
                        ]
                    ),
                    Expires = DateTime.UtcNow.AddYears(100),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_config["JWT:SECRET_KEY"]!)
                        ),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Get the bearer token from header.authorization
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Return payload of JWT token</returns>
        public JwtPayload GetAndDecodeJwtToken(HttpRequest request)
        {
            string authHeader = request.Headers.Authorization!;
            string token = authHeader.Split(' ')[1];
            JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);

            return jwtToken.Payload;
        }
    }
}
