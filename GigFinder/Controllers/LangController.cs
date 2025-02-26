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
    public class LangController: ApiController
    {
        private gigfinderEntities1 db = new gigfinderEntities1();

        [HttpGet]
        [Route("api/lang")]
        public async Task<IHttpActionResult> GetLangs()
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                return Ok(await db.Languages.ToListAsync());
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}