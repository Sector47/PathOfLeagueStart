namespace PathOfLeagueStart
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Class to hold gem data
    /// </summary>
    public class Gem
    {
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("level requirement")]
        public string level_requirement { get; set; }

        [JsonProperty("tags")]
        public string tags { get; set; }

        [JsonProperty("item class id restriction")]
        public string item_class_id_restriction { get; set; }

        [JsonProperty("gem tags")]
        public string gem_tags { get; set; }

        [JsonProperty("stat text")]
        public string stat_text { get; set; }
    }
}
