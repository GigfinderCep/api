using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using GigFinder.Models;
using GigFinder.Controllers.Request;
using GigFinder.Utils;
using GigFinder.Attributes;

namespace GigFinder.Controllers
{
    public static class ResponseMessages
    {
        public const string EXISTS = "exists";
        public const string INVALID_EMAIL_OR_PASSWORD = "invalid_email_or_password";
        public const string USER_NOT_FOUND = "user_not_found";
        public const string SUCCESS = "success";
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
                User user = await db.Users
                                .Where(u => u.email == request.Email)
                                .FirstOrDefaultAsync();
                if (user != null)
                {
                    return BadRequest(ResponseMessages.EXISTS);
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

                foreach (var item in request.Genres)
                {
                    Genre genre = await db.Genres.FindAsync(item);
                    if(genre == null)
                    {
                        return BadRequest("genre does not exists");
                    }
                    
                    newUser.Genres.Add(genre);
                }
                await db.SaveChangesAsync();

                // create musican

                return Ok(Jwt.GenerateUserJwt(newUser));
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
                User user = await db.Users
                                .Where(u => u.email == request.Email)
                                .FirstOrDefaultAsync();
                if (user != null)
                {
                    return BadRequest(ResponseMessages.EXISTS);
                }

                // create new user
                var newUser = await createUser(
                    request.Name,
                    request.Email,
                    request.Password,
                    request.Description,
                    UserTypes.LOCAL
                 );

                // create local 
                var local = new Local
                {
                    id = newUser.id,
                    capacity = request.Capacity,
                    x_coordination = request.X_coordination,
                    y_coordination = request.Y_coordination
                };
                db.Locals.Add(local);

                foreach (var item in request.Genres)
                {
                    Genre genre = await db.Genres.FindAsync(item);
                    if (genre == null)
                    {
                        return BadRequest("genre does not exists");
                    }

                    newUser.Genres.Add(genre);
                }
                await db.SaveChangesAsync();

                // create musican

                return Ok(Jwt.GenerateUserJwt(newUser));
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        // POSt: api/auth/signup/musician
        [HttpPut]
        [Route("update/musician")]
        [ProtectedUser(UserTypes.MUSIC)]
        public async Task<IHttpActionResult> UpdateMusician([FromBody] RequestUpdateMusician request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }

                User user = UserUtils.GetCurrentUser();

                user.name = request.Name;
                user.description = request.Description;

                Musician musician = user.Musician;
                musician.size = (byte)request.Size;
                musician.price = request.Price;
                musician.songs_lang = request.LangId;

              
                user.Genres.Clear();
                foreach (var item in request.Genres)
                {
                    Genre genre = await db.Genres.FindAsync(item);
                    if (genre == null)
                    {
                        return BadRequest("genre does not exists");
                    }

                    user.Genres.Add(genre);
                }
                await db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        // POSt: api/auth/signup/local
        [HttpPut]
        [Route("update/local")]
        [ProtectedUser(UserTypes.LOCAL)]
        public async Task<IHttpActionResult> UpdatesLocal([FromBody] RequestUpdateLocal request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }

                User user = UserUtils.GetCurrentUser();

                user.name = request.Name;
                user.description = request.Description;

                Local local = user.Local;
                local.capacity = request.Capacity;
                local.x_coordination = request.X_coordination;
                local.y_coordination = request.Y_coordination;
                
                user.Genres.Clear();
                foreach (var item in request.Genres)
                {
                    Genre genre = await db.Genres.FindAsync(item);
                    if (genre == null)
                    {
                        return BadRequest("genre does not exists");
                    }

                    user.Genres.Add(genre);
                }
                await db.SaveChangesAsync();

                // create musican

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login([FromBody] RequestLogin request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }
                // validate user does  exists
                User user = await db.Users
                                .Where(u => u.email == request.Email)
                                .FirstOrDefaultAsync();
                if (user == null)
                {
                    return BadRequest(ResponseMessages.INVALID_EMAIL_OR_PASSWORD);
                }

                // validate user password
                var correctPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.password);
                if (!correctPassword)
                {
                    return BadRequest(ResponseMessages.INVALID_EMAIL_OR_PASSWORD);
                }

                return Ok(Jwt.GenerateUserJwt(user));
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("forgotpass")]
        public async Task<IHttpActionResult> ForgotPassword([FromBody] RequestForgotPassword request)
        {
            // create admin incidence
            try
            {
                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }

                User user = await db.Users
                               .Where(u => u.email == request.Email)
                               .FirstOrDefaultAsync();
                if (user == null)
                {
                    return BadRequest(ResponseMessages.USER_NOT_FOUND);
                }

                var newIncidence = new Incidence
                {
                    user_id = user.id,
                    description = "forgotpass",
                    status = "pendent"
                };

                db.Incidences.Add(newIncidence);
                await db.SaveChangesAsync();

                return Ok(ResponseMessages.SUCCESS);
            }
            catch (Exception e){
                return BadRequest(e.ToString());
            }
        }


        [HttpGet]
        [Route("whoami")]
        [ProtectedUser]
        public async Task<IHttpActionResult> WhoAmI()
        {
            User user = UserUtils.GetCurrentUser();

            return Ok(new
            {
                id = user.id,
                name = user.name,
                description = user.description,
                type = user.type,
                avg_rating = user.avg_rating,
                profile_image = user.profile_image_identifier
            });
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