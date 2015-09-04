using FixtureApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FixtureApi.Controllers
{
    [RoutePrefix("api/Fixtures/{id}")]
    public class FixtureController : ApiController
    {
        [Route("Matches")]
        public IHttpActionResult GetAllMatches(int id) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                if(found.Matches.Count==0) {
                    found.BuildMatches();
                }
                return Ok(found.Matches);
            }
        }

        
    }
}
