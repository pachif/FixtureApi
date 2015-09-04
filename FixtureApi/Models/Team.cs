using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;

namespace FixtureApi.Models {
    public class Team : BaseObject {
        private List<Player> _players;

        public Team(int newid, string name) {
            Id = newid;
            Name = name;
        }

        public List<Player> Players {
            get {
                if(_players == null) {
                    _players = new List<Player>();
                }
                return _players;
            }
        }
    }
}