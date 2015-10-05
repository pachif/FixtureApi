using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using FixtureApi.Serialization;
using Newtonsoft.Json;

namespace FixtureApi.Models {
    public class Group : BaseObject {
        private List<Match> _matches;
        private Dictionary<Team, int> _scores;
        private Dictionary<int, Team> _qualified;
        private List<Team> _teams;

        [JsonIgnore]
        public List<Team> Teams
        {
            get { return _teams ?? (_teams = new List<Team>()); }
        }

        public List<ScoreItem> TeamScores {
            get
            {
                List<ScoreItem> scores = new List<ScoreItem>();
                foreach (var keyValue in Scores) {
                    var score = new ScoreItem() { TeamName = keyValue.Key.Name, Score = keyValue.Value };
                    scores.Add(score);
                }
                scores.Sort(new ScoreComparer());
                return scores;
            }
        }

        [JsonIgnore]
        public Dictionary<Team, int> Scores
        {
            get { return _scores ?? (_scores = new Dictionary<Team, int>()); }
        }

        [JsonIgnore]
        public Dictionary<int, Team> Qualified
        {
            get { return _qualified ?? (_qualified = new Dictionary<int, Team>()); }
        }
    }
}