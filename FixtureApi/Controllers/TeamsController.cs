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
        /// <summary>
        /// Get the list of all the teams
        /// </summary>
        /// <param name="id">the fixture id</param>
        /// <returns>list of teams</returns>
        [Route("Teams")]
        public IHttpActionResult GetAllTeams(int id) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                return Ok(found.Teams);
            }
        }

        /// <summary>
        /// Retrieves a single Team
        /// </summary>
        /// <param name="id"></param>
        /// <param name="teamId"></param>
        /// <returns>a single team</returns>
        [Route("Teams/{teamId}")]
        public IHttpActionResult GetTeamById(int id, int teamId) {
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

        /// <summary>
        /// Adds a new Team to the fixture
        /// </summary>
        /// <param name="id">the fixture id</param>
        /// <param name="team">the new team</param>
        /// <returns>a single team</returns>
        [Route("Teams")]
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
