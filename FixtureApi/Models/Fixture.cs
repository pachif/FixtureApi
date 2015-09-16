using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public MatchMode Mode {
            get; set;
        }

        public int GroupSize {
            get; set;
        }

        public List<Team> Teams {
            get {
                if(_teams == null) {
                    _teams = new List<Team>();
                }
                return _teams;
            }
        }

        public List<Group> Groups {
            get; set;
        }
        #endregion

        internal Match FindMatchByOrder(int matchOrder) {
            return Matches.SingleOrDefault(x => x.Order == matchOrder);
        }

        internal Match UpdateMatch(Match match) {
            var currentMatch = this.Matches.SingleOrDefault(x => x.Order == match.Order);
            if(currentMatch != null) {
                currentMatch.Result = match.Result;
                // League processing if necessary update teams scores
                if(currentMatch.Group == null || this.Mode == MatchMode.LeagueOneWay || this.Mode == MatchMode.LeagueTwoWay) {
                    switch(match.Result) {
                        case ResultType.Tie:
                            UpdateTeamScore(currentMatch.AwayTeam, 1);
                            UpdateTeamScore(currentMatch.HomeTeam, 1);
                            break;
                        case ResultType.Win:
                            UpdateTeamScore(currentMatch.HomeTeam, 3);
                            break;
                        case ResultType.Lose:
                            UpdateTeamScore(currentMatch.AwayTeam, 3);
                            break;
                        case ResultType.NotPlayed:
                        case ResultType.Scheduled:
                        default:
                            break;
                    }
                }
                // Tournament Processing
                if (Mode==MatchMode.Tournament)
                {
                    // TODO: Complete the Calculation of next round of matches
                    if (currentMatch.Group != null)
                    {
                        // for Group phase
                        var group = currentMatch.Group;
                        switch (match.Result)
                        {
                            case ResultType.Tie:
                                group.Scores[currentMatch.AwayTeam] += 1;
                                group.Scores[currentMatch.HomeTeam] += 1;
                                break;
                            case ResultType.Win:
                                group.Scores[currentMatch.HomeTeam] += 3;
                                break;
                            case ResultType.Lose:
                                group.Scores[currentMatch.AwayTeam] += 3;
                                break;
                            case ResultType.NotPlayed:
                            case ResultType.Scheduled:
                            default:
                                break;
                        }
                    }
                    else
                    {
                        int playedMatches = 0;
                        string teamName = string.Empty;
                        // we are sitting at Eliminatory matches
                        // check first for qualified games
                        foreach (Group group in Groups)
                        {
                            var matches = Matches.FindAll(m => m.Group.Equals(group));
                            int allMatches = CalculateMaxNumberOfLeagueMatches(group.Teams.Count);
                            playedMatches =
                                matches.Count(m => m.Result != ResultType.NotPlayed || m.Result != ResultType.Scheduled);
                            if (playedMatches.Equals(allMatches) && group.Qualified.Count.Equals(0))
                            {
                                List<KeyValuePair<Team, int>> teams =
                                    group.Scores.OrderByDescending(vk => vk.Value).ToList();
                                for (int i = 1; i < 3; i++)
                                {
                                    var qualified = teams[i].Key;
                                    group.Qualified.Add(i, qualified);
                                    // Replace processing
                                    teamName = string.Format("{0} Group#{1}", i == 1 ? "1st" : "2nd", group.Id);
                                    var matchToReplace = Matches.Find(m => m.HomeTeam.Name.Equals(teamName));
                                    if (i == 1)
                                    {
                                        matchToReplace.HomeTeam = qualified;
                                    }
                                    else
                                    {
                                        matchToReplace.AwayTeam = qualified;
                                    }
                                }
                            }
                        }
                        
                        playedMatches = Matches.Count(m => m.Result != ResultType.Scheduled);
                        int allGroupMatches = CalculateMaxCountGroupMatches();
                        if (playedMatches > allGroupMatches)
                        {
                            // check for eliminatories
                            var matches = Matches.FindAll(m => m.Result == ResultType.Scheduled);
                            foreach (Match elimintatoryGame in matches)
                            {
                                teamName = string.Format("Winner Match #{0}", elimintatoryGame.Order);
                                if (elimintatoryGame.HomeTeam.Name.Equals(teamName) && match.Result == ResultType.Win)
                                {
                                    elimintatoryGame.HomeTeam = match.Result == ResultType.Win ? match.HomeTeam : match.AwayTeam;
                                }
                            }
                            // check for finals
                            string teamNameFormat = "Semifinal Winner Match #{0}";
                            Match foundMatch = matches.Find(m => m.HomeTeam.Name.Equals(string.Format(teamNameFormat, match.Order)));
                            if (foundMatch!=null )
                            {
                                foundMatch.HomeTeam = match.Result == ResultType.Win ? match.HomeTeam : match.AwayTeam;
                            }
                            foundMatch = matches.Find(m => m.AwayTeam.Name.Equals(string.Format(teamNameFormat, match.Order)));
                            if (foundMatch != null) {
                                foundMatch.AwayTeam = match.Result == ResultType.Win ? match.HomeTeam : match.AwayTeam;
                            }
                            //-
                            teamNameFormat = "Semifinal Loser Match #{0}";
                            foundMatch = matches.Find(m => m.HomeTeam.Name.Equals(string.Format(teamNameFormat, match.Order)));
                            if (foundMatch != null) {
                                foundMatch.HomeTeam = match.Result == ResultType.Lose ? match.HomeTeam : match.AwayTeam;
                            }
                            foundMatch = matches.Find(m => m.AwayTeam.Name.Equals(string.Format(teamNameFormat, match.Order)));
                            if (foundMatch != null) {
                                foundMatch.AwayTeam = match.Result == ResultType.Lose ? match.HomeTeam : match.AwayTeam;
                            }
                        }
                        
                    }
                }
            }

            return currentMatch;
        }

        internal int AddTeam(string name) {
            int newid = 0;
            if(!Teams.Any(x => x.Name == name)) {
                newid = Teams.Count + 1;
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
            return Matches.Any(x => (x.HomeTeam == teamA && x.AwayTeam == teamB)
            || (x.HomeTeam == teamB && x.AwayTeam == teamA));
        }

        internal bool AnyHomeMatch(Team teamA, Team teamB) {
            return Matches.Any(x => x.HomeTeam == teamA && x.AwayTeam == teamB);
        }

        internal bool AnyAwayMatch(Team teamA, Team teamB) {
            return Matches.Any(x => x.HomeTeam == teamB && x.AwayTeam == teamA);
        }

        internal void AddMatch(Team home, Team away, Group group) {
            int order = Matches.Count;
            var newMatch = new Match { HomeTeam = home, AwayTeam = away, Group = group, Order = order + 1 };
            _matches.Add(newMatch);
        }

        internal void AddMatch(Match match) {
            if(Matches.Any(x => x.Order == match.Order)) {
                throw new Exception("Cannot add the match, there is one with the same order #" + match.Order);
            }
            _matches.Add(match);
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

        private void CheckValidations(Fixture fixture)
        {
            if (fixture.Mode == MatchMode.Tournament)
            {
                if (fixture.Teams.Count < 4)
                {
                    throw new Exception("Insuficient Numbers of Teams");
                } else if (fixture.Teams.Count < fixture.GroupSize)
                {
                    throw new Exception("Insuficient Numbers of Teams according GroupSize");
                }
            }

        }

        private static void BuildTournamentMatches(ref Fixture fixture) {
            var groupQuantity = fixture.Teams.Count / fixture.GroupSize;

            #region Teams Draw
            // draw phase
            List<Group> groups = new List<Group>();
            List<Team> notDrawTeams = new List<Team>(fixture.Teams);
            Random ra = new Random();
            for (int g = 0; g < groupQuantity; g++) {
                string groupName = string.Format("Group #{0}", g + 1);
                Group gr = new Group { Id = g + 1, Name = groupName };
                for (int i = 0; i < fixture.GroupSize; i++) {
                    // loop until reach a team that is not drawn yet
                    Team team = fixture.Teams[ra.Next(fixture.Teams.Count)];
                    while (!notDrawTeams.Contains(team)) {
                        team = fixture.Teams[ra.Next(fixture.Teams.Count)];
                    }
                    // Now configure the group
                    gr.Teams.Add(team);
                    gr.Scores.Add(team, 0);
                    notDrawTeams.Remove(team);
                }
                GenerateLeagueMatches(ref fixture, gr.Teams, false, gr);
                groups.Add(gr);
            }
            fixture.Groups = groups; 
            #endregion

            #region 1st Round Cup Fixture
            int order = fixture.Matches.Count;
            int ficticiousId = int.MaxValue - 1;
            foreach (Group group in fixture.Groups) { 
                // Skip even
                if (group.Id % 2 == 0) continue;
                string teamNameA = string.Format("1st Group #{0}", group.Id);
                string teamNameB = string.Format("2nd Group #{0}", group.Id + 1);
                var teamA = new Team(ficticiousId, teamNameA);
                ficticiousId--;
                var teamB = new Team(ficticiousId, teamNameB);
                ficticiousId--;
                var newMatch = new Match { HomeTeam = teamA, AwayTeam = teamB, Order = order + 1, Result = ResultType.Scheduled };
                fixture.AddMatch(newMatch);
                order++;
            }

            foreach (Group group in fixture.Groups) { 
                // Skip uneven
                if (group.Id % 2 == 1) continue;
                string teamNameA = string.Format("1st Group #{0}", group.Id);
                string teamNameB = string.Format("2nd Group #{0}", group.Id - 1);
                var teamA = new Team(ficticiousId, teamNameA);
                ficticiousId--;
                var teamB = new Team(ficticiousId, teamNameB);
                ficticiousId--;
                var newMatch = new Match { HomeTeam = teamA, AwayTeam = teamB, Order = order + 1, Result = ResultType.Scheduled };
                fixture.AddMatch(newMatch);
                order++;
            }
            #endregion

            #region Eliminatories
            // Final eliminatories matches
            var maxGroupNum = fixture.Groups.Count * 2;
            while (maxGroupNum > 4) {
                for (int i = 0; i < maxGroupNum; i = i + 2) {
                    int roundMatch1 = order - maxGroupNum - i;
                    int roundMatch2 = order - maxGroupNum - i + 1;
                    var teamA = new Team(ficticiousId, string.Format("Winner Match #{0}", roundMatch1));
                    ficticiousId--;
                    var teamB = new Team(ficticiousId, string.Format("Winner Match #{0}", roundMatch2));
                    ficticiousId--;

                    var newMatch = new Match {
                        HomeTeam = teamA,
                        AwayTeam = teamB,
                        Order = order,
                        Result = ResultType.Scheduled
                    };
                    fixture.AddMatch(newMatch);
                    order++;
                }

                maxGroupNum = maxGroupNum / 2;
            }
            #endregion

            #region Final Matches
            // Firts 4 Teams position matches
            int finalsMatch1 = order - 1;
            int finalsMatch2 = order;
            for (int l = 0; l < 4; l = l + 2) {
                var teamA = new Team(ficticiousId, string.Format("Semifinal {1} Match #{0}", finalsMatch1, l < 2 ? "Winner" : "Loser"));
                ficticiousId--;
                var teamB = new Team(ficticiousId, string.Format("Semifinal {1} Match #{0}", finalsMatch2, l < 2 ? "Winner" : "Loser"));
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
            #endregion
        }

        private static void GenerateLeagueMatches(ref Fixture fixture, List<Team> teams, bool isTwoWays, Group group = null) {
            //int maxMatchOrder = fixture.Matches.Count;
            int maxNumber = CalculateMaxNumberOfLeagueMatches(teams.Count);
            int bucket = isTwoWays ? maxNumber : maxNumber / 2;
            Random r = new Random();
            while(bucket!=0) {
                int i = r.Next(teams.Count);
                int j = r.Next(teams.Count);
                if(i!=j) {
                    Team teamA = teams[i];
                    Team teamB = teams[j];
                    if(isTwoWays) {
                        if(!fixture.AnyHomeMatch(teamA,teamB)) {
                            fixture.AddMatch(teamA, teamB, group);
                            bucket--;
                        }
                    } else {
                        if(!fixture.AnyMatch(teamA, teamB)) {
                            fixture.AddMatch(teamA, teamB, group);
                            bucket--;
                        }
                    }
                }
            }
        }

        private static int CalculateMaxNumberOfLeagueMatches(int numberOfTeams) {
            int result = 0;
            for(int i = 0; i < numberOfTeams; i++) {
                for(int j = 0; j < numberOfTeams; j++) {
                    if(i == j) {
                        continue;
                    }
                    result++;
                }
            }
            return result;
        }

        private int CalculateMaxCountGroupMatches()
        {
            return CalculateMaxNumberOfLeagueMatches(GroupSize) * Groups.Count;
        }
    }
}