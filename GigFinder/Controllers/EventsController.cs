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
using System.Web.Http.Description;


namespace GigFinder.Controllers
{

    [RoutePrefix("api/events")]
    [ProtectedUser(UserTypes.LOCAL)]
    public class EventsController : ApiController
    {
        private gigfinderEntities1 db = new gigfinderEntities1();

        [HttpPost]
        [Route("create")]
        [ProtectedUser(UserTypes.LOCAL)]
        [ResponseType(typeof(Event))]
        public async Task<IHttpActionResult> Create([FromBody] RequestCreateEvents request)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }
                User user = UserUtils.GetCurrentUser();
     
                var genre = await db.Genres.FindAsync(request.GenreId);
                if(genre == null)
                {
                    return BadRequest("genre not found");
                }
                var newEvent = new Event
                {
                    local_id = user.id,
                    price = request.Price,
                    description = request.Description,
                    genre_id = request.GenreId,
                    date_start = request.DateStart,
                    date_end = request.DateEnd,
                    opened_offer = true,
                    canceled = false
                };

                db.Events.Add(newEvent);
                await db.SaveChangesAsync();
                return Ok(newEvent);
            }
            catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("{eventId}/aplicate")]
        [ProtectedUser(UserTypes.MUSIC)]
        public async Task<IHttpActionResult> CreateAplication(int eventId,[FromBody] RequestCreateAplication request)
        {
            try
            {
                if (eventId < 1)
                {
                    return BadRequest("Invalid event ID. It must be a numeric value greater than or equal to 1.");
                }

                db.Configuration.LazyLoadingEnabled = false;

                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }
                User user = UserUtils.GetCurrentUser();

                var appEvent = await db.Events.FindAsync(eventId);
                if (appEvent == null)
                {
                    return BadRequest("genre not found");
                }

                bool applicationExists = await db.Aplications
            .       AnyAsync(a => a.user_id == user.id && a.event_id == eventId);
                if(applicationExists)
                {
                    return BadRequest("aplication exists");
                }
                var newApplication = new Aplication
                {
                    user_id = user.id,
                    description = request.Description,
                    event_id = eventId,
                    status = "pendent"
                };

                db.Aplications.Add(newApplication);
                await db.SaveChangesAsync();
                return Ok(ResponseMessages.SUCCESS);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}