using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfLeagueStart
{
    // This class is used to store data about skill gems.
    public class Gem
    {
        public string name { get; set; }
        public string level_requirement { get; set; }
        public string tags { get; set; }
        public string item_class_id_restriction { get; set; }
        public string gem_tags { get; set; }
        public string stat_text { get; set; }
        /*
        private int requiredLvl;
        private List<string> compatibleWeapons;
        private List<string> gemTags;




        // constructor for gem. Requires Name, ReqLvl, Compatible weapons, Gem tags.
        public Gem(string name, int requiredLvl, List<string> compatibleWeapons, List<string> gemTags)
        {
            this.name = name;
            this.requiredLvl = requiredLvl;
            this.compatibleWeapons = compatibleWeapons;
            this.gemTags = gemTags;
        }
        */
    }
}
