using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;

namespace FixtureApi.Models {
    public class Group : BaseObject {
        private List<Match> _matches;
        private Dictionary<Team, int> _scores;
        private Dictionary<int, Team> _qualified;
        private List<Team> _teams;

        public List<Team> Teams
        {
            get { return _teams ?? (_teams = new List<Team>()); }
        }

        public Dictionary<Team, int> Scores
        {
            get { return _scores ?? (_scores = new Dictionary<Team, int>()); }
        }

        public Dictionary<int, Team> Qualified
        {
            get { return _qualified ?? (_qualified = new Dictionary<int, Team>()); }
        }
    }
}