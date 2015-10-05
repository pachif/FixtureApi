using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FixtureApi.Serialization {
    public class ScoreItem {
        public int Score { get; set; }
        public string TeamName { get; set; }
    }

    public class ScoreComparer : IComparer<ScoreItem> {
        public int Compare(ScoreItem x, ScoreItem y)
        {
            return y.Score.CompareTo(x.Score);
        }
    }
}
