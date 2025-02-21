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
    }
}