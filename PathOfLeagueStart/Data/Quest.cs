﻿using Newtonsoft.Json;
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
        public string reward { get; set; }

        [JsonProperty("classes")]
        public string classes { get; set; }

        public string initialZone { get; set; }

        public string finishZone { get; set; }

        public string prerequisiteQuest { get; set; }

        // constructor for a quest object without rewards
        public Quest(string questName, string intInitialZone, string finishZone,
            string prerequisiteQuest)
        {
            this.questName = questName;
            this.initialZone = intInitialZone;
            this.finishZone = finishZone;
            this.prerequisiteQuest = prerequisiteQuest;
        }
    }

}
