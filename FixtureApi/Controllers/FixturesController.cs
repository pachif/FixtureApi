using FixtureApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FixtureApi.Controllers
{
    public class FixturesController : ApiController {

        public IHttpActionResult GetAllFixtures() {
            List<Fixture> list = Database.GetFixtures();
            return Ok(list);
        }

        public IHttpActionResult GetFixture(int id) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                return Ok(found);
            }
        }

        public IHttpActionResult PostFixture(Fixture fixture) {
            Database.PersistFixture(fixture);
            return Ok();
        }

        public IHttpActionResult PutFixture(Fixture fixture) {
            Database.PersistFixture(fixture);
            return Ok();
        }
    }
}
