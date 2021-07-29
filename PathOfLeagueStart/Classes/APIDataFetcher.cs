using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace PathOfLeagueStart.Classes
{
    /// <summary>
    /// This class will request data from the pathofexile gamepedia api once per league or when manually initiated.
    /// The data will be converted to an sqlite database locally.
    /// An api call will confirm that the current league matches the league stored locally, if not then it will refetch the data.
    /// </summary>
    class APIDataFetcher
    {
        private List<Gem> allSkillGems = new List<Gem>();
        private List<Quest> questRewardsList = new List<Quest>();
        private List<Vendor> vendorRewardsList = new List<Vendor>();
        private List<Quest> questList = new List<Quest>();
        private List<Area> areaList = new List<Area>();
        private List<Weapon> weaponList = new List<Weapon>();

        public APIDataFetcher()
        {
            // Check if current data is out of data or missing
            //if (localLeague == currentLeague) 
            // Then don't downloadjsondata or adddatatodb
            bool isLeagueUpToDate = false;
            try
            {
                // using a webclient we will get our json data from the api call and store the current event league
                using (WebClient wc = new WebClient())
                {
                    // create our api call string
                    string jsonFile = wc.DownloadString(
                        "https://www.pathofexile.com/api/leagues?type=main&" +
                        "realm=pc&" +
                        "compat=1&" +
                        // We only want one result
                        "limit=1&" +
                        // We don't need the permanent leagues, standard, ssf,  hardcore, ssfhc
                        "offset=4&" +
                        "format=json");


                    // Make a JArray from parsing the json file.
                    JArray arrayOfLeague = JArray.Parse(jsonFile);

                    // Check that something was downloaded, if not throw an exception
                    if (arrayOfLeague.Count == 0)
                    {
                        throw new Exception("League array was empty");
                    }

                    string currentLeague = arrayOfLeague[0]["id"].ToString();

                    // Check against our last league launch
                    if (Properties.Settings.Default.LastLeagueLaunched == currentLeague)
                    {
                        // if we are up to date, then set our bool to true, and don't redownload data
                        isLeagueUpToDate = true;
                    }
                    else
                    {
                        // Set our setting for last league launched to this for future checking
                        Properties.Settings.Default.LastLeagueLaunched = currentLeague;
                        Properties.Settings.Default.Save();
                        Logger.LogDebug("Last League Launched is now: " + currentLeague);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while attempting to download skill gem data", e);
            }

            // if we are out of date, download json data otherwise pull from db
            // TODO invert this once db is setup
            if (!isLeagueUpToDate)
            {
                DownloadJSONData();
                SaveData();

                
            }
            else
            {
                PullDataFromFile();
            }

            
        }

        public List<Weapon> WeaponList { 
            get { return weaponList;  }
            set { weaponList = value; }
        }
        public List<Gem> AllSkillGems { 
            get { return allSkillGems; }
            set { allSkillGems = value; }
        }
        public List<Quest> QuestRewardsList {
            get { return questRewardsList; }
            set { questRewardsList = value; }
        }
        public List<Vendor> VendorRewardsList { 
            get { return vendorRewardsList; }
            set { vendorRewardsList = value; }
        }
        public List<Quest> QuestList { 
            get { return questList; }
            set { questList = value; }
        }
        public List<Area> AreaList { 
            get { return areaList; }
            set { areaList = value; }
        }

        /// <summary>
        /// Used to manually update the data, rather than waiting for league to change.
        /// </summary>
        public void ManualUpdateData()
        {
            DownloadJSONData();
            SaveData();
        }

        /// <summary>
        /// This downloads all the relevant data from the pathofexile.fandom.com wiki using their cargoquery api
        /// </summary>
        private void DownloadJSONData()
        {
            // For debug purposes we count all failed error calls.
            // Additionally if an error occurs, we will not update the database with the faulty data.
            int errorCount = 0;
            try
            {
                // using a webclient we will get our json data from the api calls, and store them in the lists.
                using (WebClient wc = new WebClient())
                {
                    // create our api call string
                    string jsonFile = wc.DownloadString(
                        "https://pathofexile.gamepedia.com/api.php?action=cargoquery&" +
                        // Our API string is made up of the tables we are asking for, in this case we need the items table, skill_gems, skill_levels, and skill tables because each table only holds some of the information we need.
                        "tables=items,skill_gems,skill_levels,skill&" +
                        // We need the name of the skill gem, the level requirements, the tags, the class id restriction which specifies what weapons it requires, the gem tags which we might use to categorize them with, and we grab the stat text in case we want to display the skill gems in the future
                        "fields=items.name,skill_levels.level_requirement,items.tags,skill.item_class_id_restriction,skill_gems.gem_tags,items.stat_text,skill_gems.primary_attribute&" +
                        // Then some where conditions to get only the relevant items from the items table. Frame gem means it is the gem item type, the skil_levels means we only grab the base levels for the gem, otherwise the call would be 40x as big.
                        "where=items.frame_type=%22gem%22%20AND%20skill_levels.level=%221%22&" +
                        // Join our tables we requested on the name of the skill gem.
                        "join_on=items.name=skill_gems._pageName,skill_gems._pageName=skill_levels._pageName,skill_gems._pageName=skill._pageName&" +
                        // Default limit is too small, this limit will work for a bit longer, and can be expanded by making a second call starting after this one ends, or will last longer if removing awakened gems from results.
                        "limit=500&" +
                        // The format we want is json.
                        "format=json");

                    // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                    JObject gemJObject = JObject.Parse(jsonFile);
                    List<JToken> results = gemJObject["cargoquery"].Children().Children().Children().ToList();

                    foreach (JToken result in results)
                    {
                        Gem gem = result.ToObject<Gem>();
                        AllSkillGems.Add(gem);
                    }

                    // Check that something was downloaded, if not throw an exception
                    if (AllSkillGems.Count == 0)
                    {
                        throw new Exception("Gem count is 0");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while attempting to download skill gem data", e);
                errorCount++;                
            }

            try
            {
                // download quest reward data
                using (WebClient wc = new WebClient())
                {

                    string jsonFile = wc.DownloadString(
                        "https://pathofexile.gamepedia.com/api.php?action=cargoquery&" +
                        // The quest_rewards table holds all of the individual rewards available for each quest, and specifies what classes receive those awards.
                        "tables=quest_rewards&" +
                        // We need  the quest, the _pageName=Page which is the reward name, and the classes that are rewarded that gem
                        "fields=quest,_pageName=Page,classes&" +
                        // Default limit too small, we don't really need this big of a limit, but it doesn't hurt in case more quest rewards are added.
                        "limit=500&" +
                        // The format we want is json.
                        "format=json");

                    // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                    JObject questJObject = JObject.Parse(jsonFile);
                    List<JToken> results = questJObject["cargoquery"].Children().Children().Children().ToList();

                    foreach (JToken result in results)
                    {
                        Quest quest = result.ToObject<Quest>();
                        QuestRewardsList.Add(quest);
                    }

                    // Check that something was downloaded, if not throw an exception
                    if (QuestRewardsList.Count == 0)
                    {
                        throw new Exception("Quest Reward List count is 0");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while attempting to download quest reward data", e);
                errorCount++;
            }

            try
            {
                // download vendor reward data
                using (WebClient wc = new WebClient())
                {                   
                    string jsonFile = wc.DownloadString(
                        "https://pathofexile.gamepedia.com/api.php?action=cargoquery&" +
                        // Vendor rewards tables holds the same data as quest rewards, except it is referring to rewards that are able to be purchased after quest completion and which npc sells it
                        "tables=vendor_rewards&" +
                        // The quest that rewards it, the name of the gem, the classes able to purchase it, and the npc that sells it
                        "fields=quest,_pageName=Page,classes,npc&" +
                        // Because the vendor reward list is too long and goes past the 500 limit we will have to split it up, rather than do an offset we can remove a few npc rewards
                        // Lilly Roth sells all rewards to all classes after her quest is finished, we have no reason to ask what she will sell
                        // Siosa sells most rewards, we will need to know what he sells in case it is the only way for a class to receive that rewards but this can be its own api call
                        "where=npc!=%22Lilly%20Roth%22%20AND%20npc!=%22Siosa%22&" +
                        // Default limit too small, with removing the two npcs we are 100 under the limit.
                        "limit=500&" +
                        // The format we want is json.
                        "format=json");

                    // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                    JObject vendorJObject = JObject.Parse(jsonFile);
                    List<JToken> results = vendorJObject["cargoquery"].Children().Children().Children().ToList();

                    foreach (JToken result in results)
                    {
                        Vendor vendor = result.ToObject<Vendor>();
                        VendorRewardsList.Add(vendor);
                    }

                    // Our second call with just siosa

                    jsonFile = wc.DownloadString(
                        "https://pathofexile.fandom.com/api.php?action=cargoquery&" +
                        "tables=vendor_rewards&" +
                        "fields=quest,_pageName=Page,classes,npc&" +
                        // Same as our previous call except we specify we want the npc to be siosa
                        "where=npc=%22Siosa%22&" +
                        // Default limit too small, siosa has currently 328 results himself
                        "limit=500&" +
                        "format=json");

                    // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                    vendorJObject = JObject.Parse(jsonFile);
                    results = vendorJObject["cargoquery"].Children().Children().Children().ToList();

                    foreach (JToken result in results)
                    {
                        Vendor vendor = result.ToObject<Vendor>();
                        VendorRewardsList.Add(vendor);
                    }

                    // Just because this is easier we also grab lilly roth
                    jsonFile = wc.DownloadString(
                        "https://pathofexile.fandom.com/api.php?action=cargoquery&" +
                        "tables=vendor_rewards&" +
                        "fields=quest,_pageName=Page,classes,npc&" +
                        // Same as our previous call except we specify we want the npc to be lilly roth
                        "where=npc=%22Lilly%20Roth%22&" +
                        // Default limit too small, siosa has currently 365 results herself
                        "limit=500&" +
                        "format=json");

                    // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                    vendorJObject = JObject.Parse(jsonFile);
                    results = vendorJObject["cargoquery"].Children().Children().Children().ToList();

                    foreach (JToken result in results)
                    {
                        Vendor vendor = result.ToObject<Vendor>();
                        VendorRewardsList.Add(vendor);
                    }
                    // Check that something was downloaded, if not throw an exception
                    if (VendorRewardsList.Count == 0)
                    {
                        throw new Exception("Vendor reward count is 0");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while attempting to download vendor reward data", e);
                errorCount++;
            }

            try
            {
                // download area data
                using (WebClient wc = new WebClient())
                {

                    string jsonFile = wc.DownloadString(
                        "https://pathofexile.gamepedia.com/api.php?action=cargoquery&" +
                        // All the data we need is in the area table
                        "tables=areas&" +
                        // We need area level, whether it has a waypoint, the name, and the main page because that specifies which act it is from
                        "fields=area_level,has_waypoint,name,main_page&" +
                        // We need to not get results for labyrinth areas, empty name areas, and map areas which are above area lvl 68. This allows us to fit inside the limit
                        "where=is_labyrinth_area=%22no%22%20AND%20main_page!=%22%22%20AND%20area_level%20%3C%2068&" +
                        // We group it by the main page so we get seperate areas for each version (act version)
                        "group_by=main_page&" +
                        // Default limit too small
                        "limit=500&" +
                        // The format we want is json
                        "format=json");

                    // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                    JObject areaJObject = JObject.Parse(jsonFile);
                    List<JToken> results = areaJObject["cargoquery"].Children().Children().Children().ToList();

                    foreach (JToken result in results)
                    {
                        Area area = result.ToObject<Area>();
                        AreaList.Add(area);
                    }

                    // Check that something was downloaded, if not throw an exception
                    if (AreaList.Count == 0)
                    {
                        throw new Exception("Area count is 0");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while attempting to download area reward data", e);
                errorCount++;
            }

            try
            {
                // download weapon data
                using (WebClient wc = new WebClient())
                {

                    string jsonFile = wc.DownloadString(
                        "https://pathofexile.fandom.com/api.php?action=cargoquery&" +
                        // We grab from the items table so we don't have to do a join for the drop level, required level, or class id. 
                        // This could be changed so that it also has the dps from the weapons table in the future.
                        // For now we will assume that as required lvl goes up, that is the best base available.
                        "tables=items&" +
                        // Fields we want are required level, name, class id, and drop level
                        "fields=items.required_level,items.name,items.class_id&" +
                        // We want items that match the class ids that weapons have
                        "where=" +
                            "(" +
                                "items.class_id=%22Claws%22%20OR%20" +
                                "items.class_id=%22Dagger%22%20OR%20" +
                                "items.class_id=%22Wand%22%20OR%20" +
                                "items.class_id=%22One%20Hand%20Sword%22%20OR%20" +
                                "items.class_id=%22Thrusting%20One%20Hand%20Sword%22%20OR%20" +
                                "items.class_id=%22One%20Hand%20Axe%22%20OR%20" +
                                "items.class_id=%22One%20Hand%20Mace%22%20OR%20" +
                                "items.class_id=%22Bow%22%20OR%20" +
                                "items.class_id=%22Staff%22%20OR%20" +
                                "items.class_id=%22Two%20Hand%20Sword%22%20OR%20" +
                                "items.class_id=%22Two%20Hand%20Axe%22%20OR%20" +
                                "items.class_id=%22Two%20Hand%20Mace%22%20OR%20" +
                                "items.class_id=%22Sceptre%22" +
                            ")" +
                            // and only if they are normal items, otherwise we would get every unique item as well.
                            "AND%20" +
                                "items.frame_type=%22normal%22&" +
                        // Default limit too small
                        "limit=500&" +
                        // The format we want is json
                        "format=json");

                    // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                    JObject weaponJObject = JObject.Parse(jsonFile);
                    List<JToken> results = weaponJObject["cargoquery"].Children().Children().Children().ToList();

                    foreach (JToken result in results)
                    {
                        Weapon weapon = result.ToObject<Weapon>();
                        WeaponList.Add(weapon);
                    }

                    // Check that something was downloaded, if not throw an exception
                    if (WeaponList.Count == 0)
                    {
                        throw new Exception("Weapon count is 0");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while attempting to download area reward data", e);
                errorCount++;
            }
        }

        /// <summary>
        /// This will add our lists of data to the database
        /// </summary>
        private void SaveData()
        {
            // Get our file location to store data
            string localAppDataFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // For each of our lists we will serialize it and store in a file.
            string filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "gemData.json");
            FileStream fs = File.Open(filePath, FileMode.OpenOrCreate);

            // Serialize the object
            string gemString = JsonConvert.SerializeObject(allSkillGems, Formatting.Indented);

            // Make our streamwrite for the given file
            StreamWriter sw = new StreamWriter(fs);
            // write to the file
            sw.WriteLine(gemString);

            // flush the streamwriter.
            sw.Flush();

            // Repeat below changing names

            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "weaponData.json");
            fs = File.Open(filePath, FileMode.OpenOrCreate);
            string weaponString = JsonConvert.SerializeObject(weaponList, Formatting.Indented);
            sw = new StreamWriter(fs);
            sw.WriteLine(weaponString);
            sw.Flush();

            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "areaData.json");
            fs = File.Open(filePath, FileMode.OpenOrCreate);
            string areaString = JsonConvert.SerializeObject(areaList, Formatting.Indented);
            sw = new StreamWriter(fs);
            sw.WriteLine(areaString);
            sw.Flush();

            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "questRewardsData.json");
            fs = File.Open(filePath, FileMode.OpenOrCreate);
            string questRewardsString = JsonConvert.SerializeObject(questRewardsList, Formatting.Indented);
            sw = new StreamWriter(fs);
            sw.WriteLine(questRewardsString);
            sw.Flush();

            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "vendorRewardsData.json");
            fs = File.Open(filePath, FileMode.OpenOrCreate);
            string vendorRewardsString = JsonConvert.SerializeObject(vendorRewardsList, Formatting.Indented);
            sw = new StreamWriter(fs);
            sw.WriteLine(vendorRewardsString);
            sw.Flush();

            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "questData.json");
            fs = File.Open(filePath, FileMode.OpenOrCreate);
            string questString = JsonConvert.SerializeObject(questList, Formatting.Indented);
            sw = new StreamWriter(fs);
            sw.WriteLine(questString);
            sw.Flush();

        }

        /// <summary>
        /// This will fill our lists using our local sqlite db
        /// </summary>
        private void PullDataFromFile()
        {
            // Get our file location to store data
            string localAppDataFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Get our file location for the json data.
            string filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "gemData.json");
            allSkillGems = JsonConvert.DeserializeObject<List<Gem>>(File.ReadAllText(filePath));


            // repeat for other data
            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "weaponData.json");
            weaponList = JsonConvert.DeserializeObject<List<Weapon>>(File.ReadAllText(filePath));

            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "areaData.json");
            areaList = JsonConvert.DeserializeObject<List<Area>>(File.ReadAllText(filePath));

            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "questRewardsData.json");
            questRewardsList = JsonConvert.DeserializeObject<List<Quest>>(File.ReadAllText(filePath));

            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "vendorRewardsData.json");
            vendorRewardsList = JsonConvert.DeserializeObject<List<Vendor>>(File.ReadAllText(filePath));

            filePath = Path.Combine(localAppDataFilePath, "PathOfLeagueStart", "questData.json");
            questList = JsonConvert.DeserializeObject<List<Quest>>(File.ReadAllText(filePath));




        }
    }
}
