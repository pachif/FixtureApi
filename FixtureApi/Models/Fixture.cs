using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace FixtureApi.Models {
    public class Fixture : BaseObject {
        private List<Match> _matches;
        private Dictionary<Team, int> _teamScores;
        private List<Team> _teams;

        public Fixture(int v, string name) {
            Id = v;
            Name = name;
        }

        #region Properties
        public List<Match> Matches {
            get {
                if(_matches == null) {
                    _matches = new List<Match>();
                }
                return _matches;
            }
        }

        public Dictionary<Team, int> TeamScores {
            get {
                if(_teamScores == null) {
                    _teamScores = new Dictionary<Team, int>();
                }
                return _teamScores;
            }
        }

        public List<Team> Teams {
            get {
                if(_teams == null) {
                    _teams = new List<Team>();
                }
                return _teams;
            }
        }

        public MatchMode Mode {
            get; set;
        }

        public int GroupSize {
            get; set;
        }

        public List<Group> Groups {
            get; set;
        } 
        #endregion

        internal int AddTeam(string name) {
            int newid = Teams.Count + 1;
            if(!Teams.Any(x => x.Name == name)) {
                _teams.Add(new Team(newid, name));
            }
            return newid;
        }

        internal void RemoveTeam(int teamId) {
            var team = FindTeamById(teamId);
            _teams.Remove(team);
        }

        internal Team FindTeamById(int teamId) {
            return _teams.FirstOrDefault(x => x.Id.Equals(teamId));
        }

        internal void UpdateTeamScore(Team team, int score) {
            if(!TeamScores.ContainsKey(team)) {
                _teamScores.Add(team, 0);
            }
            _teamScores[team] += score;
        }

        internal bool AnyMatch(Team teamA, Team teamB) {
            return _matches.Any(x => (x.HomeTeam == teamA && x.AwayTeam == teamB)
            || (x.HomeTeam == teamB && x.AwayTeam == teamA));
        }

        internal bool AnyHomeMatch(Team teamA, Team teamB) {
            return _matches.Any(x => x.HomeTeam == teamA && x.AwayTeam == teamB);
        }

        internal bool AnyAwayMatch(Team teamA, Team teamB) {
            return _matches.Any(x => x.HomeTeam == teamB && x.AwayTeam == teamA);
        }

        internal void AddMatch(Team home, Team away, Group group) {
            int order = _matches.Count;
            var newMatch = new Match { HomeTeam = home, AwayTeam = away, Group = group, Order = order + 1 };
            _matches.Add(newMatch);
        }

        internal void AddMatch(Match match) {
            if(!Matches.Contains(match)) {
                _matches.Add(match);
            }
        }

        /// <summary>
        /// Method responsible for creating the appropiate start matches according fixture mode
        /// </summary>
        internal void BuildMatches() {

            var fixture = this;
            CheckValidations(fixture);
            switch(fixture.Mode) {
                case MatchMode.Tournament:
                    BuildTournamentMatches(ref fixture);
                    break;
                case MatchMode.LeagueOneWay:
                    GenerateLeagueMatches(ref fixture, fixture.Teams, false);
                    break;
                case MatchMode.LeagueTwoWay:
                    GenerateLeagueMatches(ref fixture, fixture.Teams, true);
                    break;
                default:
                    break;
            }

        }

        private void CheckValidations(Fixture fixture) {
            if(fixture.Mode == MatchMode.Tournament && fixture.Teams.Count < 4) {
                throw new Exception("Insuficient Numbers of Teams");
            }
        }

        private static void BuildTournamentMatches(ref Fixture fixture) {
            int teamCount = 1;
            var groupQuantity = fixture.Teams.Count / fixture.GroupSize;
            //int matchOrder = 1;
            List<Group> groups = new List<Group>();
            for(int g = 0; g < groupQuantity; g++) {
                string groupName = string.Format("Group #{0}", g + 1);
                Group gr = new Group { Id = g + 1, Name = groupName };
                var teams = new List<Team>();
                for(int i = 0; i < fixture.GroupSize; i++) {
                    Team teamA = new Team(teamCount, string.Format("Team #{0}", teamCount));
                    teamCount++;
                    teams.Add(teamA);
                }
                gr.Teams = teams;
                GenerateLeagueMatches(ref fixture, gr.Teams, false, gr);
                groups.Add(gr);
            }
            fixture.Groups = groups;

            // 1st Round Cup Fixture
            int order = fixture.Matches.Count;
            int ficticiousId = int.MaxValue - 1;
            foreach(Group group in fixture.Groups) { // Impares
                if(group.Id % 2 == 1) continue;
                string teamNameA = string.Format("1st Group #{0}", group.Id);
                string teamNameB = string.Format("2st Group #{0}", group.Id + 1);
                var teamA = new Team(ficticiousId, teamNameA);
                ficticiousId--;
                var teamB = new Team(ficticiousId, teamNameB);
                ficticiousId--;
                var newMatch = new Match { HomeTeam = teamA, AwayTeam = teamB, Order = order + 1, Result = ResultType.Scheduled };
                fixture.AddMatch(newMatch);
                order++;
            }

            foreach(Group group in fixture.Groups) { // Pares
                if(group.Id % 2 == 0) continue;
                string teamNameA = string.Format("1st Group #{0}", group.Id);
                string teamNameB = string.Format("2st Group #{0}", group.Id - 1);
                var teamA = new Team(ficticiousId, teamNameA);
                ficticiousId--;
                var teamB = new Team(ficticiousId, teamNameB);
                ficticiousId--;
                var newMatch = new Match { HomeTeam = teamA, AwayTeam = teamB, Order = order + 1, Result = ResultType.Scheduled };
                fixture.AddMatch(newMatch);                
                order++;
            }

            var maxGroupNum = fixture.Groups.Count*2;
            while (maxGroupNum > 4)
            {
                for (int i = 0; i < maxGroupNum; i = i + 2)
                {
                    int roundMatch1 = order - maxGroupNum - i;
                    int roundMatch2 = order - maxGroupNum - i + 1;
                    var teamA = new Team(ficticiousId, string.Format("Winer Match #{0}", roundMatch1));
                    ficticiousId--;
                    var teamB = new Team(ficticiousId, string.Format("Winer Match #{0}", roundMatch2));
                    ficticiousId--;
                    order++;
                    var newMatch = new Match
                        {
                            HomeTeam = teamA,
                            AwayTeam = teamB,
                            Order = order,
                            Result = ResultType.Scheduled
                        };
                    fixture.AddMatch(newMatch);
                }

                maxGroupNum = maxGroupNum/2;
            }

            // Firts 4 Teams position matches
            for (int l = 0; l < 4; l=l+2)
            {
                int finalsMatch1 = order - maxGroupNum - l;
                int finalsMatch2 = order - maxGroupNum - l + 1;
                var teamA = new Team(ficticiousId, string.Format("{1} Match #{0}", finalsMatch1, l<2?"Winer":"Looser"));
                ficticiousId--;
                var teamB = new Team(ficticiousId, string.Format("{1} Match #{0}", finalsMatch2, l < 2 ? "Winer" : "Looser"));
                ficticiousId--;
                order++;
                var newMatch = new Match {
                    HomeTeam = teamA,
                    AwayTeam = teamB,
                    Order = order,
                    Result = ResultType.Scheduled
                };
                fixture.AddMatch(newMatch);
            }
        }

        private static void GenerateLeagueMatches(ref Fixture fixture, List<Team> teams, bool isTwoWays, Group group = null) {
            int maxMatchOrder = fixture.Matches.Count;
            int teamNumber = teams.Count;
            for(int i = 0; i < teamNumber; i++) {
                Team teamA = teams.ElementAt(i);
                for(int j = 0; j < teamNumber; j++) {
                    if(i == j) {
                        continue;
                    }
                    Team teamB = teams.ElementAt(j);
                    if(isTwoWays) {
                        fixture.AddMatch(teamA, teamB, group);
                    } else {
                        if(!fixture.AnyMatch(teamA, teamB)) {
                            fixture.AddMatch(teamA, teamB, group);
                        }
                    }
                }
            }
        }

    }
}