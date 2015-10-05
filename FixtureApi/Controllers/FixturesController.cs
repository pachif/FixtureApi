using FixtureApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FixtureApi.Serialization;

namespace FixtureApi.Controllers
{
    public class FixturesController : ApiController {

        /// <summary>
        /// Retrieves current list of fixtures
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetAllFixtures() {
            List<Fixture> list = Database.GetFixtures();
            return Ok(list);
        }

        /// <summary>
        /// Returns a single fixture
        /// </summary>
        /// <param name="id">the fixture id</param>
        /// <returns></returns>
        public HttpResponseMessage GetFixture(int id) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            } else {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, found);
                return response;
            }
        }

        /// <summary>
        /// Create or Updates the fixture passed in
        /// </summary>
        /// <param name="fixture">the affected fixture</param>
        /// <returns></returns>
        public IHttpActionResult PostFixture(Fixture fixture) {
            Database.PersistFixture(fixture);
            return Ok();
        }

        /// <summary>
        /// Get the Team results depending on the fixture mode
        /// </summary>
        /// <param name="id">the fixture id</param>
        /// <param name="groupId">Optional. the group we want results</param>
        /// <returns></returns>
        [Route("api/Fixtures/{id}/Results/{groupId:int?}")]
        public IHttpActionResult GetResults(int id, int? groupId = null)
        {
            Fixture found = Database.GetFixture(id);
            Dictionary<Team, int> result = new Dictionary<Team, int>();
            if (found == null)
            {
                return NotFound();
            }
            else
            {
                switch (found.Mode)
                {
                    case MatchMode.Tournament:
                        //TODO:Retrive teams according positions, once it is ended
                        if (groupId.HasValue)
                        {
                            var group = found.Groups.SingleOrDefault(g => g.Id == groupId.Value);
                            result = group == null ? null : group.Scores;
                        }
                        break;
                    case MatchMode.LeagueOneWay:
                    case MatchMode.LeagueTwoWay:
                        result = found.TeamScores;
                        break;
                }
                List<ScoreItem> scores =new List<ScoreItem>();
                foreach (var keyValue in result)
                {
                    var score = new ScoreItem() {TeamName = keyValue.Key.Name, Score = keyValue.Value};
                    scores.Add(score);
                }
                scores.Sort(new ScoreComparer());
                return Ok(scores);
            }
        }

        /// <summary>
        /// Returns all the groups with the teams embeded
        /// </summary>
        /// <param name="id">the fixture id</param>
        /// <returns></returns>
        [Route("api/Fixtures/{id}/Groups")]
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
                return Ok(found.Groups);
            }
        }
    }
}
