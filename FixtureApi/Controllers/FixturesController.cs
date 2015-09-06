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

        [Route("Results")]
        public IHttpActionResult GetResults(int id) {
            Fixture found = Database.GetFixture(id);
            Dictionary<Team, int> result = new Dictionary<Team, int>();
            if(found == null) {
                return NotFound();
            } else {
                switch(found.Mode) {
                    case MatchMode.Tournament:
                        //TODO:Retrive teams according positions, once it is ended
                        break;
                    case MatchMode.LeagueOneWay:
                    case MatchMode.LeagueTwoWay:
                        result = found.TeamScores;
                        break;
                }
                return Ok(result);
            }
        }

        [Route("Groups")]
        public IHttpActionResult GetAllGroups(int id) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                if(found.Mode!=MatchMode.Tournament) {
                    return NotFound(); 
                }
                if(found.Groups.Count == 0) {
                    found.BuildMatches();
                }
                return Ok(found.Matches);
            }
        }
    }
}
