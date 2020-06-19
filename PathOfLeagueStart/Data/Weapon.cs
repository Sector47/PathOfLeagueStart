using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PathOfLeagueStart
{
    class Weapon
    {
        [JsonProperty("required level")]
        public int requiredLevel { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }
    }
}
