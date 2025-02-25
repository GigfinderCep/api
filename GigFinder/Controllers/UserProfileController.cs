using GigFinder.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace GigFinder.Controllers
{
    [RoutePrefix("api/user")]
    public class UserProfileController : ApiController
    {
        private gigfinderEntities1 db = new gigfinderEntities1();

        [HttpGet]
        [Route("{userId}")]
        public async Task<IHttpActionResult> GetUserProfile(int userId)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var user = await db.Users
                                .Include(u => u.Musician)
                                .Include(u => u.Attachments)
                                .Include(u => u.Musician.Events)
                                .Include(u => u.Local.Events)
                                .Include(u => u.Ratings)
                                .Include(u => u.Local)
                                .Include(u => u.Genres)
                                .Where(u => u.id == userId)
                                .Select(u => new
                                {
                                    u.id,
                                    u.name,
                                    u.description,
                                    u.type,
                                    u.profile_image_identifier,
                                    genres = u.Genres.Select(g => new
                                    {
                                        g.id,
                                        g.name
                                    }),
                                    ratings = u.Ratings.Select(r => new
                                    {
                                        r.User.name,
                                        r.User.profile_image_identifier,
                                        r.content,
                                        r.User.id
                                    }),
                                    attachments = u.Attachments.Select(a => new
                                    {
                                        a.description,
                                        a.id,
                                        a.File.path,
                                        a.File.mimetype
                                    }),
                                    events = u.Local.Events.Concat(u.Musician.Events)
                                                .Select(e => new
                                                {
                                                    e.date_start,
                                                    e.description,
                                                    e.id
                                                })
                                })
                                .FirstOrDefaultAsync();
                if(user == null)
                {
                    return BadRequest("user not found");
                }

                return Ok(user);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}