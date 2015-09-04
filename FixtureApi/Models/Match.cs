using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;

namespace FixtureApi.Models {
    public class Match {
        public Team HomeTeam {
            get; set;
        }

        public Team AwayTeam {
            get; set;
        }

        public ResultType Result {
            get; set;
        }

        public int Order {
            get; set;
        }

        public Group Group {
            get; set;
        }
    }
}