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
        public string AreaLevel { get; set; }

        [JsonProperty("has waypoint")]
        public string HasWaypoint { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("main page")]
        public string MainPage { get; set; }
    }
} 
