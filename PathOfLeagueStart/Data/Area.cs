namespace PathOfLeagueStart
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization.Formatters;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Class to hold Area data
    /// </summary>
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

        public override string ToString()
        {
            return name;
        }

    }
}
