using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfLeagueStart
{
    public class Area
    {

        [JsonProperty("area level")]
        public string areaLevel { get; set; }

        [JsonProperty("has waypoint")]
        public string hasWaypoint { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("main page")]
        public string mainPage { get; set; }
    }

}
