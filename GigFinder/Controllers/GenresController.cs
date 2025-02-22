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
    [RoutePrefix("api/genres")]

    public class GenresController: ApiController
    {
        private gigfinderEntities1 db = new gigfinderEntities1();

        [HttpGet]
        [Route("all")]
        public async Task<IHttpActionResult> GetAll()
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var genres = await db.Genres
                 .ToListAsync();

                return Ok(genres);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

    }
}