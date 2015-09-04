using FixtureApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixtureApi {
    public static class Database {
        private static Dictionary<int, Fixture> _fixtures = new Dictionary<int, Fixture>();

        public static void Initialize() {
            if(_fixtures.Count > 0) return;

            var fixture = new Fixture(1, "Demo Fixture");
            fixture.Mode = MatchMode.LeagueOneWay;
            fixture.AddTeam("Team A");
            fixture.AddTeam("Team C");
            fixture.AddTeam("Team D");
            fixture.AddTeam("Team B");

            fixture.BuildMatches();

            _fixtures.Add(1, fixture);
        }

        internal static List<Fixture> GetFixtures() {
            return _fixtures.Values.ToList();
        }

        internal static Fixture GetFixture(int fixtureId) {
            return !_fixtures.ContainsKey(fixtureId) ? null : _fixtures[fixtureId];
        }

        internal static void PersistFixture(Fixture fixture) {
            if(fixture.Id == 0) {
                fixture.Id = _fixtures.Count + 1;
                _fixtures.Add(fixture.Id, fixture);
            } else {
                _fixtures[fixture.Id] = fixture;
            }
        }
    }
}
