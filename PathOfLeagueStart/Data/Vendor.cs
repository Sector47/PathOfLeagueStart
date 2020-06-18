using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfLeagueStart
{
    public class Vendor
    {

        [JsonProperty("quest")]
        public string quest { get; set; }

        [JsonProperty("reward")]
        public string reward { get; set; }

        [JsonProperty("classes")]
        public string classes { get; set; }

        [JsonProperty("npc")]
        public string npc { get; set; }
    }
}
