using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfLeagueStart
{
    public class Quest
    {

        [JsonProperty("quest")]
        public string questName { get; set; }

        [JsonProperty("reward")]
        public string teward { get; set; }

        [JsonProperty("classes")]
        public string classes { get; set; }
    }

}
