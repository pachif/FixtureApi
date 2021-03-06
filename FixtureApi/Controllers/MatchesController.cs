﻿using FixtureApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FixtureApi.Controllers {
    [RoutePrefix("api/Fixtures/{id}")]
    public class MatchesController : ApiController {
        /// <summary>
        /// Retrieves all Fixture Matches which are planned
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Matches")]
        public IHttpActionResult GetAllMatches(int id) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                if(found.Matches.Count == 0) {
                    found.BuildMatches();
                }
                return Ok(found.Matches);
            }
        }

        /// <summary>
        /// Retrieves a single Match object for the given parameters
        /// </summary>
        /// <param name="id">the fixture id</param>
        /// <param name="matchOrder">the match order</param>
        /// <returns></returns>
        [Route("Matches/{matchOrder}")]
        public IHttpActionResult GetMatchByOrder(int id, int matchOrder) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                if(found.Matches.Count == 0) {
                    found.BuildMatches();
                }
                Match match = found.FindMatchByOrder(matchOrder);
                if(match == null) {
                    return NotFound();
                }
                return Ok(match);
            }
        }

        /// <summary>
        /// Updates the result existent match
        /// </summary>
        /// <param name="id">the fixture id</param>
        /// <param name="match">the edited match</param>
        /// <returns></returns>
        [Route("Matches")]
        public IHttpActionResult Post(int id, [FromBody] Match match) {
            Fixture found = Database.GetFixture(id);
            if(found == null) {
                return NotFound();
            } else {
                if(found.Matches.Count == 0) {
                    found.BuildMatches();
                }
                var foundMatch = found.UpdateMatch(match);
                if(foundMatch == null) {
                    return NotFound();
                }
                // Save modified match
                Database.PersistFixture(found);
                return Ok(foundMatch);
            }
        }
    }
}
