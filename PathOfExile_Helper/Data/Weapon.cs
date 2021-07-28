using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PathOfLeagueStart
{
    public class Weapon
    {
        [JsonProperty("required level")]
        public int requiredLevel { get; set; }

        [JsonProperty("drop level")]
        public int dropLevel { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}
