using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;

namespace FixtureApi.Models {
    public class Group : BaseObject {
        public List<Team> Teams {
            get; set;
        }

        public Dictionary<int, Team> Qualified {
            get; set;
        }
    }
}