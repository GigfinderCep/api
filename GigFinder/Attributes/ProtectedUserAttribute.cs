using GigFinder.Utils;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.IdentityModel.Tokens;
using GigFinder.Models;

namespace GigFinder.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ProtectedUserAttribute : AuthorizationFilterAttribute
    {
        private readonly string requiredRole = null;
        private gigfinderEntities1 db = new gigfinderEntities1();
        public ProtectedUserAttribute(string role = null)
        {
            requiredRole = role;
        }
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            var authHeader = request.Headers.Authorization;

            if (authHeader == null || authHeader.Scheme != "Bearer")
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    ReasonPhrase = "Unauthorized: Invalid or missing token",
                    Content = new StringContent("Unauthorized: Invalid or missing token")
                };
                return;
            }

            var token = authHeader.Parameter;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(Jwt.PRIVATE_KEY); // Use your static JWT secret key

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "gigfinder",
                    ValidAudience = "gigfinder",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "user_id");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        ReasonPhrase = "Unauthorized: Invalid or missing token",
                        Content = new StringContent("Unauthorized: Invalid or missing token")
                    };
                    return;
                }

                // Retrieve user from database
                var user = db.Users.Find(userId);
                if (user == null)
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        ReasonPhrase = "Unauthorized: Invalid or missing token",
                        Content = new StringContent("Unauthorized: Invalid or missing token")
                    };
                    return;
                }

                if(requiredRole != null)
                {
                    if(user.type != requiredRole)
                    {
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                        {
                            ReasonPhrase = "Unauthorized role",
                            Content = new StringContent("Unauthorized role")
                        };
                        return;
                    }
                }

                // Store the user in HttpContext for controller access
                HttpContext.Current.Items["User"] = user;
            }
            catch (Exception)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }
    }
}
