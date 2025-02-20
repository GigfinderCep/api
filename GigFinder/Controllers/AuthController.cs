using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Description;
using GigFinder.Models;
using BCrypt.Net;
using GigFinder.Controllers.Request;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GigFinder.Controllers
{
    public static class AuthMessages
    {
        public const string EXISTS = "exists";

    }

    public static class UserTypes
    {
        public const string MUSIC = "music";
        public const string LOCAL = "local";
    }

    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private gigfinderEntities1 db = new gigfinderEntities1();

        // GET: api/Auth
        public IQueryable<User> GetUsers()
        {
            return db.Users;
        }

        private async Task<User> createUser(string name, string email, string password, string description = "", string type = UserTypes.MUSIC)
        {
            // Hash the password using BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);  // Hash the password

            // Create a new user with the provided details
            var newUser = new User
            {
                name = name,
                description = description,
                email = email,
                password = hashedPassword,  // Save the hashed password
                type = type,
                avg_rating = 0
            };

            // Add the new user to the Users table
            db.Users.Add(newUser);

            // Save the changes to the database
            await db.SaveChangesAsync();

            // Return the newly created user
            return newUser;
        }

        private string generateUserJwt(User user)
        {
            var key = "61e6ac9f0e7ac2c9648805ccb06728486f663180ae0375a633d810421d5adf0e5126cfb4dc78a5997e953df14dd74751cd0bc0111a5a7ba1b8757431fb7780a634243787e5fac11f7c599af56c15080d93e28b76d6a86851a8966e5228ca6115d8b948d796410c6dd7d56fe1ce0dd086fc38a31a953caedfe5b9882e7d9168f488036677cc31d9b7d7fd462726e80ad55238da2223b9c84450733cfa4e4438583f243a0bd954f2ab8b2b166a6e79c620d7e6e61e2b379a6a621cad45cff81af3bb3ad1e668376240a05767a2f23f4d5a0f73aa0d69f511dab3cd0188b3ea1230c713d4e542887a7f34507377f27bc77ddef44188aa0169a1e81a671d86cc5792";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)); // Replace with a secure key
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

        // POSt: api/auth/signup/musician
        [HttpPost]
        [Route("signup/musician")]
        public async Task<IHttpActionResult> SignupMusician([FromBody] RequestSignupMusician request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }
                // validate user does not exists
                IHttpActionResult res;
                User user = await db.Users
                                .Where(u => u.email == request.Email)
                                .FirstOrDefaultAsync();
                if (user != null)
                {
                    return BadRequest(AuthMessages.EXISTS);
                }

                // create new user
                var newUser = await createUser(
                    request.Name,
                    request.Email,
                    request.Password,
                    request.Description,
                    UserTypes.MUSIC
                 );

                // create musican 
                var musican = new Musician
                {
                    id = newUser.id,
                    size = (byte)request.Size,
                    price = request.Price,
                    songs_lang = request.LangId
                };
                db.Musicians.Add(musican);
                await db.SaveChangesAsync();

                // create musican

                return Ok(generateUserJwt(newUser));
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        // POSt: api/auth/signup/local
        [HttpPost]
        [Route("signup/local")]
        public async Task<IHttpActionResult> SignupLocal([FromBody] RequestSignupLocal request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }
                // validate user does not exists
                IHttpActionResult res;
                User user = await db.Users
                                .Where(u => u.email == request.Email)
                                .FirstOrDefaultAsync();
                if (user != null)
                {
                    return BadRequest(AuthMessages.EXISTS);
                }

                // create new user
                var newUser = await createUser(
                    request.Name,
                    request.Email,
                    request.Password,
                    request.Description,
                    UserTypes.LOCAL
                 );

                // create musican 
                var local = new Local
                {
                    id = newUser.id,
                    capacity = request.Capacity,
                    x_coordination = request.X_coordination,
                    y_coordination = request.Y_coordination
                };
                db.Locals.Add(local);
                await db.SaveChangesAsync();

                // create musican

                return Ok(generateUserJwt(newUser));
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        //// GET: api/Auth/5
        //[ResponseType(typeof(User))]
        //public async Task<IHttpActionResult> GetUser(int id)
        //{
        //    User user = await db.Users.FindAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(user);
        //}

        //// PUT: api/Auth/5
        //[ResponseType(typeof(void))]
        //public async Task<IHttpActionResult> PutUser(int id, User user)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != user.id)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(user).State = EntityState.Modified;

        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UserExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //// POST: api/Auth
        //[ResponseType(typeof(User))]
        //public async Task<IHttpActionResult> PostUser(User user)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Users.Add(user);
        //    await db.SaveChangesAsync();

        //    return CreatedAtRoute("DefaultApi", new { id = user.id }, user);
        //}

        //// DELETE: api/Auth/5
        //[ResponseType(typeof(User))]
        //public async Task<IHttpActionResult> DeleteUser(int id)
        //{
        //    User user = await db.Users.FindAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Users.Remove(user);
        //    await db.SaveChangesAsync();

        //    return Ok(user);
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        //private bool UserExists(int id)
        //{
        //    return db.Users.Count(e => e.id == id) > 0;
        //}
    }
}