using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;

namespace FixtureApi.Models {
    public enum ResultType {
        NotPlayed,
        Tie,
        Win,
        Lose,
        Scheduled
    }

    public enum MatchMode {
        Tournament,
        LeagueOneWay,
        LeagueTwoWay
    }
}