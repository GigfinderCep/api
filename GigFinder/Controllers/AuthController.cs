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

        // POSt: api/auth/signup
        [HttpPost]
        [Route("api/auth/signup")]
        public async Task<IHttpActionResult> Signup([FromBody] RequestSignupMusician request)
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

                return Ok("token");
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