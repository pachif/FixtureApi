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
    public class TeamsController : ApiController
    {
        [Route("Teams")]
        public IHttpActionResult GetAllTeams(int id) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                return Ok(found.Teams);
            }
        }

        [Route("Teams/{teamId}")]
        public IHttpActionResult GetAllTeams(int id, int teamId) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                Team team = found.FindTeamById(teamId);
                if(team==null) {
                    return NotFound();
                }
                return Ok(team);
            }
        }

        public IHttpActionResult Post(int id, [FromBody] Team team) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                int newid = found.AddTeam(team.Name);
                if(newid==0) {
                    throw new Exception(string.Format("A Team with the name: {0} is already stored", team.Name));
                }
                Database.PersistFixture(found);
                Team created = found.FindTeamById(newid); 
                return Ok(created);
            }
        }
    }
}
