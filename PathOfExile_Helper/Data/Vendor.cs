namespace PathOfLeagueStart
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Vendor
    {

        [JsonProperty("quest")]
        public string questName { get; set; }

        [JsonProperty("reward")]
        public string reward { get; set; }

        [JsonProperty("classes")]
        public string classes { get; set; }

        [JsonProperty("npc")]
        public string npc { get; set; }

        public string initialZone { get; set; }

        public string finishZone { get; set; }

        public string prerequisiteQuest { get; set; }

        public bool isCompleted { get; set; }

        public bool isStarted { get; set; }
    }
}
