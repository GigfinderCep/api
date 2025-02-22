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
using Microsoft.Extensions.Logging;


namespace GigFinder.Controllers
{
    public static class AplicationTypes {
        public const string ACCEPTED = "accepted";
        public const string PENDENT = "pendent";
        public const string REJECTED = "rejected";
    }

    [RoutePrefix("api/events")]
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

        [HttpGet]
        [Route("all/opened")]
        public async Task<IHttpActionResult> GetAll()
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var events = await db.Events
                 .Include(e => e.Local) // Include Local
                 .Include(e => e.Genre) // Include Genre
                 .Where(e => e.opened_offer == true)
                 .Select(e => new
                 {
                     e.id,
                     e.description,
                     e.date_start,
                     e.date_end,
                     e.price,
                     e.opened_offer,
                     e.canceled,
                     e.cancel_msg,
                     Genre = new
                     {
                         e.Genre.id,
                         e.Genre.name // Include only required fields, avoiding `Genre.Events`
                     },
                     Local = new
                     {
                         e.Local.id,
                         e.Local.capacity,
                         e.Local.x_coordination,
                         e.Local.y_coordination
                     }
                 })
                 .ToListAsync();



                return Ok(events);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("all-my")]
        [ProtectedUser]
        public async Task<IHttpActionResult> GetAllMyEvents()
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                User user = UserUtils.GetCurrentUser();

                var events = await db.Events
                 .Include(e => e.Local) // Include Local
                 .Include(e => e.Genre) // Include Genre
                 .Where(e => (e.local_id == user.id || e.musician_id == user.id))
                 .Select(e => new
                 {
                     e.id,
                     e.description,
                     e.date_start,
                     e.date_end,
                     e.price,
                     e.opened_offer,
                     e.canceled,
                     e.cancel_msg,
                     Genre = new
                     {
                         e.Genre.id,
                         e.Genre.name // Include only required fields, avoiding `Genre.Events`
                     },
                     e.local_id,
                     e.musician_id
                 })
                 .ToListAsync();



                return Ok(events);
            }
            catch (Exception e)
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
                    return BadRequest("event not found");
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

        [HttpPost]
        [Route("{eventId}/cancel")]
        [ProtectedUser]
        public async Task<IHttpActionResult> CancelEvent(int eventId)
        {
            try
            {
                if (eventId < 1)
                {
                    return BadRequest("Invalid event ID. It must be a numeric value greater than or equal to 1.");
                }

                User user = UserUtils.GetCurrentUser();

                var appEvent = await db.Events.FindAsync(eventId);
                if (appEvent == null)
                {
                    return BadRequest("event not found");
                }
                if(appEvent.canceled == true)
                {
                    return BadRequest("application already canceled");
                }
               if(appEvent.musician_id != user.id && appEvent.local_id != user.id)
                {
                    return BadRequest("you are not authorized to perform that action");
                }
                appEvent.canceled = true;

                await db.SaveChangesAsync();
                return Ok(ResponseMessages.SUCCESS);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("{eventId}/aplication/{userId}/accept")]
        [ProtectedUser(UserTypes.LOCAL)]
        public async Task<IHttpActionResult> AcceptAplication(int eventId, int userId)
        {
            try
            {

                if(eventId < 1)
                {
                    return BadRequest("Invalid event ID. It must be a numeric value greater than or equal to 1.");
                }

                if (userId < 1)
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

                var aplication = await db.Aplications
                                      .FirstOrDefaultAsync(a => a.user_id == userId && a.event_id == eventId);

                if (aplication == null)
                {
                    return BadRequest("aplication not found");
                }
                var appEvent = await db.Events.FindAsync(aplication.event_id);
       
                if (!appEvent.local_id.Equals(user.id))
                {
                    return BadRequest("user is not authorized to manage event requests");
                }

                if(appEvent.musician_id != null)
                {
                    return BadRequest("event already accepted an aplication");
                }

                appEvent.musician_id = aplication.user_id;
                appEvent.opened_offer = false;
                aplication.status = AplicationTypes.ACCEPTED;

                 var otherApplications = db.Aplications
                    .Where(a => a.event_id == appEvent.id && a.user_id != userId)
                    .ToList();

                foreach (var app in otherApplications)
                {
                    app.status = AplicationTypes.REJECTED;
                }
                
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