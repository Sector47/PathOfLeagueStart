using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using Newtonsoft.Json.Linq;
using PathOfLeagueStart.Data;

namespace PathOfLeagueStart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string logFilePath = File.ReadLines(@"Data/config.txt").Take(1).First()
            .Substring(13, File.ReadLines(@"Data/config.txt").Take(1).First().Length - 14);
        List<Gem> allSkillGems = new List<Gem>();
        private List<Gem> selectedSkillGems = new List<Gem>();
        private List<Gem> selectedSupportGems = new List<Gem>();
        private List<Quest> questRewardsList = new List<Quest>();
        private List<Vendor> vendorRewardsList = new List<Vendor>();
        private List<Quest> questList = new List<Quest>();
        private List<Area> arealList = new List<Area>();
        private List<Weapon> weaponList = new List<Weapon>();
        private StreamReader logReader;

        // Variables used for currentKnownInformation
        private int characterLevel = 0;
        private string currentArea;
        private string characterClass ="unknown";
        private List<Quest> startedQuests = new List<Quest>();
        private List<Quest> completedQuests = new List<Quest>();
        private List<string> acquiredGems = new List<string>();

        // areasEntered is used to auto complete some quests and for tracking progress
        private List<string> areasEntered = new List<string>();



        public MainWindow()
        {
            InitializeComponent();
            this.CheckValidLogPath();
            this.PopulateListBox();
            this.DownloadJSONData();
            this.CreateFileWatcher(logFilePath);
            this.FillQuestList();

            // create a dispatchertimer to read the byte on a 1 second timer.
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick +=  new EventHandler(DispatcherTimer_Tick);
            // set timer interval to 1 seconds and start timer
            dispatcherTimer.Interval = new TimeSpan(0,0,0,0,100);
            dispatcherTimer.Start();
        }

        private void CheckValidLogPath()
        {
            // TODO fix check for valid file path. Maybe check to make the sure the file is the log file?
            if (logFilePath == null || logFilePath == "ENTERLOGPATHHERE")
            {
                this.EnterLogPath();
            }
        }

        private void DownloadJSONData()
        {
            // download skill gem data
            using (WebClient wc = new WebClient())
            {
                string jsonFile = wc.DownloadString(
                    "https://pathofexile.gamepedia.com/api.php?action=cargoquery&tables=items,skill_gems,skill_levels,skill&fields=items.name,skill_levels.level_requirement,items.tags,skill.item_class_id_restriction,skill_gems.gem_tags,items.stat_text&where=items.frame_type=%22gem%22%20AND%20skill_levels.level=%221%22&join_on=items.name=skill_gems._pageName,skill_gems._pageName=skill_levels._pageName,skill_gems._pageName=skill._pageName&limit=500&format=json");

                // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                JObject gemJObject = JObject.Parse(jsonFile);
                List<JToken> results = gemJObject["cargoquery"].Children().Children().Children().ToList();

                foreach (JToken result in results)
                {
                    Gem gem = result.ToObject<Gem>();
                    allSkillGems.Add(gem);
                }
            }
            // download quest reward data
            using (WebClient wc = new WebClient())
            {
                string jsonFile = wc.DownloadString(
                    "https://pathofexile.gamepedia.com/api.php?action=cargoquery&tables=quest_rewards&fields=quest,reward,classes&limit=500&format=json");

                // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                JObject questJObject = JObject.Parse(jsonFile);
                List<JToken> results = questJObject["cargoquery"].Children().Children().Children().ToList();

                foreach (JToken result in results)
                {
                    Quest quest = result.ToObject<Quest>();
                    questRewardsList.Add(quest);
                }
            }

            // download vendor reward data
            using (WebClient wc = new WebClient())
            {

                string jsonFile = wc.DownloadString(
                    "https://pathofexile.gamepedia.com/api.php?action=cargoquery&tables=vendor_rewards&fields=quest,reward,classes,npc&limit=500&format=json");

                // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                JObject vendorJObject = JObject.Parse(jsonFile);
                List<JToken> results = vendorJObject["cargoquery"].Children().Children().Children().ToList();

                foreach (JToken result in results)
                {
                    Vendor vendor = result.ToObject<Vendor>();
                    vendorRewardsList.Add(vendor);
                }

                jsonFile = wc.DownloadString(
                    "https://pathofexile.gamepedia.com/api.php?action=cargoquery&tables=vendor_rewards&fields=quest,reward,classes,npc&limit=500&offset=500&format=json");

                // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                vendorJObject = JObject.Parse(jsonFile);
                results = vendorJObject["cargoquery"].Children().Children().Children().ToList();

                foreach (JToken result in results)
                {
                    Vendor vendor = result.ToObject<Vendor>();
                    vendorRewardsList.Add(vendor);
                }
                jsonFile = wc.DownloadString(
                    "https://pathofexile.gamepedia.com/api.php?action=cargoquery&tables=vendor_rewards&fields=quest,reward,classes,npc&limit=500&offset=1000&format=json");

                // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                vendorJObject = JObject.Parse(jsonFile);
                results = vendorJObject["cargoquery"].Children().Children().Children().ToList();

                foreach (JToken result in results)
                {
                    Vendor vendor = result.ToObject<Vendor>();
                    vendorRewardsList.Add(vendor);
                }
            }
            // download area data
            using (WebClient wc = new WebClient())
            {

                string jsonFile = wc.DownloadString(
                    "https://pathofexile.gamepedia.com/api.php?action=cargoquery&tables=areas&fields=area_level,has_waypoint,name,main_page&where=is_labyrinth_area=%22no%22%20AND%20main_page!=%22%22%20AND%20area_level%20%3C%2068&group_by=main_page&limit=500&format=json");

                // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                JObject areaJObject = JObject.Parse(jsonFile);
                List<JToken> results = areaJObject["cargoquery"].Children().Children().Children().ToList();

                foreach (JToken result in results)
                {
                    Area area = result.ToObject<Area>();
                    arealList.Add(area);
                }
            }

            // download weapon data
            using (WebClient wc = new WebClient())
            {

                string jsonFile = wc.DownloadString(
                    "https://pathofexile.gamepedia.com/api.php?action=cargoquery&tables=items&fields=items.required_level,items.name&where=items.class_id=%22Two%20Hand%20Axe%22%20AND%20items.frame_type=%22normal%22&limit=500&format=json");

                // Make a JObject from parsing the json file, then make a list of json Tokens from that jobject skipping through the blank parent, cargoquery parent, and title parent. Use this list of tokens to create skill gems
                JObject weaponJObject = JObject.Parse(jsonFile);
                List<JToken> results = weaponJObject["cargoquery"].Children().Children().Children().ToList();

                foreach (JToken result in results)
                {
                    Weapon weapon = result.ToObject<Weapon>();
                    weaponList.Add(weapon);
                }
            }


        }

        private void FillQuestList()
        {
            string line;
            string[] lineStrings = new string[4];
            int count = 0;

            System.IO.StreamReader file = new StreamReader(@"Data/questProgression.txt");
            while ((line = file.ReadLine()) != null)
            {
                lineStrings = line.Split('.');
                questList.Add(new Quest(lineStrings[0], lineStrings[1], lineStrings[2], lineStrings[3]));
                count++;
            }

            foreach (Quest q in questList)
            {
                Console.WriteLine(q.questName + " " + q.initialZone + " " + q.finishZone + " " + q.prerequisiteQuest);
            }
        }

        private void EnterLogPath()
        {
            // prompt user for valid log path. Change that 
            logFilePath = Microsoft.VisualBasic.Interaction.InputBox("Please enter a valid location for the Path of exile log file. This is required for the program to work. It should be located in the game directory Grinding Gear Games\\Path of Exile\\logs ", "Title", "C:\\Program Files (x86)\\Grinding Gear Games\\Path of Exile\\logs");
            this.CheckValidLogPath();
        }

        private int GetNumbersFromString(string text)
        {
            int foundNumber = Convert.ToInt32(Regex.Replace(text, "[^0-9]", string.Empty));
            return foundNumber;
        }

        private void PopulateListBox()
        {
            // Fills First 2 list boxes using the listBoxData.txt
            string line;
            int columnToWriteTo = 0;
            System.IO.StreamReader file = new StreamReader(@"Data/listBoxData.txt");
            while ((line = file.ReadLine()) != null)
            {
                columnToWriteTo = Convert.ToInt32(Char.GetNumericValue(line, 3));

                switch (columnToWriteTo)
                {
                    case 1:
                        ListBoxType.Items.Add(line.Substring(5, line.Length - 5));
                        break;
                    case 2:
                        ListBoxWeapon.Items.Add(line.Substring(5, line.Length - 5));
                        break;
                    default:
                        break;
                }
            }
        }

        private void ListBoxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(sender.ToString());
            // save previous selections
            this.SavePreviousSelection();
            // Clear the skills and supports
            ListBoxSkills.Items.Clear();
            ListBoxSupports.Items.Clear();
            // Update the list boxes with current valid options
            this.UpdateGUI();
            // restore previous selections if possible
            this.RestorePreviousSelection();
            // Update the known info
            this.UpdateKnownInfo();
            // Save new current selection to config.
            this.UpdateConfig();
        }

        private void ListBoxSkills_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear the supports
            ListBoxSupports.Items.Clear();
            // Store current selected skills for updating known info
            this.SavePreviousSelection();
            // Update the list boxes with current valid options
            // overload update gui to only update the supports listbox.
            this.UpdateGUI("skills");
            // Update the known info
            this.UpdateKnownInfo();
            // Save new current selection to config.
            this.UpdateConfig();
        }

        private void ListBoxWeapon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // save previous selections
            this.SavePreviousSelection();
            // Clear the skills and supports
            ListBoxSkills.Items.Clear();
            ListBoxSupports.Items.Clear();
            // Update the list boxes with current valid options
            this.UpdateGUI();
            // restore previous selections if possible
            this.RestorePreviousSelection();
            // Update the known info
            this.UpdateKnownInfo();
            // Save new current selection to config.
            this.UpdateConfig();
        }

        private void SavePreviousSelection()
        {
            if (ListBoxSkills.Items.Count != 0)
            {
                selectedSkillGems.Clear();
            }

            if (ListBoxSupports.Items.Count != 0)
            {
                selectedSupportGems.Clear();
            }

            // First store the previous selected active skills to select them if they are still available and support gems.
            foreach (var selectedItem in ListBoxSkills.SelectedItems)
            {
                selectedSkillGems.Add(GetGem(selectedItem.ToString()));
            }
            foreach (var selectedItem in ListBoxSupports.SelectedItems)
            {
                selectedSupportGems.Add(GetGem(selectedItem.ToString()));
            }
        }

        private void RestorePreviousSelection()
        {
            // List to store which items to select after looping through listbox because you can't continue searching through a listbox after modifying it.
            List<string> foundMatches = new List<string>();
            // Reselect items that were previously selected if they are still valid
            foreach (Gem g in selectedSkillGems)
            {
                foreach (var i in ListBoxSkills.Items)
                {
                    if (i.ToString() == g.name)
                    {
                        foundMatches.Add(i.ToString());
                    }
                }
            }

            foreach (string s in foundMatches)
            {
                ListBoxSkills.SelectedItems.Add(s);
            }

            foundMatches.Clear();

            foreach (Gem g in selectedSupportGems)
            {
                foreach (var i in ListBoxSupports.Items)
                {
                    if (i.ToString() == g.name)
                    {
                        foundMatches.Add(i.ToString());
                    }
                }
            }

            foreach (string s in foundMatches)
            {
                ListBoxSupports.SelectedItems.Add(s);
            }
            // Clear temp selected skill gems
            selectedSkillGems.Clear();
            selectedSupportGems.Clear();
        }
        private void UpdateGUI()
        {
            // Update GUI
            // populate the list box 3 using the downloaded json
            // Insert any gems that match the selected weapon tag. If magic is selected list all spells as well.
            foreach (Gem g in allSkillGems)
            {
                if (g.gem_tags.Contains("Support"))
                {
                    ListBoxSupports.Items.Add(g.name);
                }
                else if(ListBoxWeapon.SelectedItem != null)
                {
                    string test = ListBoxWeapon.SelectedItem.ToString();
                    if(g.item_class_id_restriction.Contains(ListBoxWeapon.SelectedItem.ToString()) || g.item_class_id_restriction == string.Empty)
                        ListBoxSkills.Items.Add(g.name);
                }
            }

            // ie: if you pass through magic it will ignore weapon choice and fill in all spells in skill list box
            // ie: if you pass through 2h axe and have selected melee it will fill the skill gems with all gems able to be used by 2h Axes in the skill gem list box
            // ie: if you pass through Sunder it will fill in all supports able to support Sunder in the support list box using the selected weapon from weapon list box
            // If a skill gem is selected we will search stat_text(stat.20.text) for weapon names ie: axes to see if the support and support the selected skill by verifying it matches the current weapon and the skills tags?
            // For now it will just populate all support gems because of no easy query for this. Possibly grab poedb.tw data? Otherwise parse the stat_text for can be used by or cannot be used with
        }

        // overloaded updategui to handle only skill or support listbox change selection so as not to duplicate items in listbox
        private void UpdateGUI(string listBoxName)
        {
            foreach (Gem g in allSkillGems)
            {
                if (g.gem_tags.Contains("Support") && listBoxName == "skills")
                {
                    // TODO add checks to correct supports being generated
                    ListBoxSupports.Items.Add(g.name);
                     
                }
            }
        }

        // update the display of known info with all known info.
        private void UpdateKnownInfo()
        {
            int currentZoneLevel = Convert.ToInt32(GetArea(currentArea).AreaLevel);
            UpdateQuestData();
            // TODO Fill known information text block with known information.
            TextBlockKnownInformation.Text =
                characterClass +" Level: " + characterLevel + "\n\n";
            // Show current zone
            TextBlockKnownInformation.Text += "Current Zone: " + currentArea + "\n\n";
            if (GetArea(currentArea) != null)
            {
                TextBlockKnownInformation.Text += "Zone Level: " + currentZoneLevel + "\n\n";
                // Check if character level is too far below zone;
                int safeDistance = 3 + ((this.characterLevel) / 16);
                if (Math.Abs(this.characterLevel - currentZoneLevel) > safeDistance)
                {
                    double xpPenalty =
                        Math.Pow(
                            ((characterLevel + 5) /
                             (characterLevel + 5 + Math.Pow((currentZoneLevel - characterLevel), 2.5))), 1.5);
                    if (characterLevel > currentZoneLevel)
                    {
                        TextBlockKnownInformation.Text += "WARNING you are too high a level for the zone and will have an xp penalty multiplier of: " +
                                                          xpPenalty.ToString("p") + "\n\n";
                    }

                    if (characterLevel < currentZoneLevel)
                    {
                        TextBlockKnownInformation.Text += "WARNING you are too low a level for the zone and will have an xp penalty multiplier of: " + 
                                                         xpPenalty.ToString("p") + "\n\n";
                    }
                    TextBlockKnownInformation.Background = new SolidColorBrush(Colors.Orange);
                }
                else if (Math.Abs(characterLevel - currentZoneLevel) == safeDistance)
                {
                    TextBlockKnownInformation.Text += "You are close to receiving an xp penalty";
                    TextBlockKnownInformation.Background = new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    TextBlockKnownInformation.Background = new SolidColorBrush(Colors.Gray);
                }
            }


            // show skill gems available in town this will go in separate text block

            TextBlockAvailableGems.Text = "These gems are available as a quest reward:\n";
            foreach (string s in GetAvailableGems())
            {
                TextBlockAvailableGems.Text += GetAvailableGems().IndexOf(s) + " " + s + "\n";
            }
            TextBlockAvailableGems.Text += "\nThese gems are available in town from vendors: \n";
            foreach (string s in GetAvailableVendorGems())
            {
                if (!GetAvailableGems().Contains(s))
                {
                    TextBlockAvailableGems.Text += (GetAvailableVendorGems().IndexOf(s)+GetAvailableGems().Count)+ " " + s + " from " + GetNpc(s) + "\n";
                }
            }
            
            // show highest required level item that is available to drop in this zone.
            TextBlockWeapons.Text = "Highest required level axe available to drop: ";
            Weapon displayAxe = new Weapon();
            displayAxe.requiredLevel = 0;
            foreach (Weapon w in weaponList)
            {
                if (w.requiredLevel < currentZoneLevel + 2 && displayAxe.requiredLevel < w.requiredLevel)
                {
                    displayAxe.name = w.name;
                }
            }
            TextBlockWeapons.Text += displayAxe.name + "\n";

    



            // show any current quest commands
            TextBlockCommands.Text = "To send a command whisper \"a\" in game.\n\nCommands:\n\nCompleted current quest = @a:y\n\nCurrent Character level = x @a:level x\nx must be an integer between 1-100 This will update on it's own upon level up.\n\nClear available gem x @a:x\n x is the number next to the gem";

            if (selectedSkillGems != null)
            {
                TextBlockKnownInformation.Text += "Selected Active Skill Gems: ";
                foreach (Gem g in selectedSkillGems)
                {
                    if (g != null)
                    {
                        TextBlockKnownInformation.Text += g.name + ", ";
                    }
                }
                // Cull the extra comma and space
                TextBlockKnownInformation.Text = TextBlockKnownInformation.Text.Substring(0, TextBlockKnownInformation.Text.Length - 2);

                TextBlockKnownInformation.Text += "\n\nSelected Support Gems: ";
                foreach (Gem g in selectedSupportGems)
                {
                    if (g != null)
                    {
                        TextBlockKnownInformation.Text += g.name + ", ";
                    }
                }
                // Cull the extra comma and space
                TextBlockKnownInformation.Text = TextBlockKnownInformation.Text.Substring(0, TextBlockKnownInformation.Text.Length - 2);

                
            }
        }

        // Gets the gem with the given name
        private Gem GetGem(string gemName)
        {
            foreach (Gem g in allSkillGems)
            {
                if (g.name == gemName)
                    return g;
            }

            return null;
        }

        private string GetNpc(string gemName)
        {
            foreach (Vendor v in vendorRewardsList)
            {
                if (v.reward == gemName && v.isCompleted)
                {
                    return v.npc;
                }
            }

            return null;
        }

        // Gets the area with the given name
        private Area GetArea(string areaName)
        {
            Area foundArea = new Area();
            List<Area> possibleAreas = new List<Area>();

            foreach (Area a in arealList)
            {
                if (a.Name == areaName)
                {
                    possibleAreas.Add(a);
                }
            }

            int levelDifference = 100;
            foreach (Area a in possibleAreas)
            {
                if (a.AreaLevel != string.Empty && levelDifference > Math.Abs(characterLevel - Convert.ToInt32(a.AreaLevel)))
                {
                    levelDifference = Math.Abs(characterLevel - Convert.ToInt32(a.AreaLevel));
                    foundArea = a;
                }
            }

            return foundArea;
        }

        private List<string> GetAvailableGems()
        {
            List<string> availableGemsList = new List<string>();
            foreach (var g in selectedSkillGems.Zip(selectedSupportGems, Tuple.Create))
            {
                Gem skillGem = g.Item1;
                Gem supportGem = g.Item2;

                // get a list of gems available
                

                for (int i = 0; i < questRewardsList.Count; i++)
                {
                    bool alreadyAvailable = false;
                    foreach (string s in availableGemsList)
                    {
                        if (s == skillGem.name || s == supportGem.name)
                        {
                            alreadyAvailable = true;
                        }
                    }


                    if (questRewardsList[i].reward == skillGem.name && questRewardsList[i].isCompleted && (questRewardsList[i].classes.Contains(characterClass) || questRewardsList[i].classes == String.Empty) && !alreadyAvailable)
                    {
                        availableGemsList.Add(skillGem.name);
                    }
                    if (questRewardsList[i].reward == supportGem.name && questRewardsList[i].isCompleted && (questRewardsList[i].classes.Contains(characterClass) || questRewardsList[i].classes == String.Empty) && !alreadyAvailable)
                    {
                        availableGemsList.Add(supportGem.name);
                    }
                }
            }

            // remove any acquired gems from list
            List<string> removeList = new List<string>();
            foreach (string s in availableGemsList)
            {
                if (acquiredGems.Contains(s))
                {
                    if (acquiredGems[acquiredGems.IndexOf(s)] == s)
                    {
                        removeList.Add(s);
                    }
                }
            }

            foreach (string s in removeList)
            {
                availableGemsList.Remove(s);
            }
            
            return availableGemsList;
        }

        private List<string> GetAvailableVendorGems()
        {
            List<string> availableGemsList = new List<string>();
            foreach (var g in selectedSkillGems.Zip(selectedSupportGems, Tuple.Create))
            {
                Gem skillGem = g.Item1;
                Gem supportGem = g.Item2;

                

                for (int i = 0; i < vendorRewardsList.Count; i++)
                {
                    // get a list of gems available
                    bool alreadyAvailable = false;
                    foreach (string s in availableGemsList)
                    {
                        if (s == skillGem.name || s == supportGem.name)
                        {
                            alreadyAvailable = true;
                        }
                    }

                    if (vendorRewardsList[i].reward == skillGem.name && vendorRewardsList[i].isCompleted && (vendorRewardsList[i].classes.Contains(characterClass) || vendorRewardsList[i].classes == String.Empty) && !alreadyAvailable)
                    {
                        availableGemsList.Add(skillGem.name);
                    }
                    if (vendorRewardsList[i].reward == supportGem.name && vendorRewardsList[i].isCompleted && (vendorRewardsList[i].classes.Contains(characterClass) || vendorRewardsList[i].classes == String.Empty) && !alreadyAvailable)
                    {
                        availableGemsList.Add(supportGem.name);
                    }
                }
            }

            // remove any acquired gems from list
            List<string> removeList = new List<string>();
            foreach (string s in availableGemsList)
            {
                if (acquiredGems.Contains(s))
                {
                    if (acquiredGems[acquiredGems.IndexOf(s)] == s)
                    {
                        removeList.Add(s);
                    }
                }
            }

            foreach (string s in removeList)
            {
                availableGemsList.Remove(s);
            }

            return availableGemsList;
        }

        private void UpdateQuestData()
        {
            // Grab data from entered areas to see if you've received any new quests
            // Add them to list of current quests, ignoring quests that were already added

            // TODO complete quests if done
            foreach (Quest q in questList)
            {
                if (areasEntered.Contains(q.finishZone))
                {
                    q.isCompleted = true;
                }

                if (areasEntered.Contains(q.initialZone))
                {
                    q.isStarted = true;
                }
            }

            foreach (Quest qr in questRewardsList)
            {
                foreach (Quest q in questList)
                {
                    if (qr.questName == q.questName && q.isCompleted)
                    {
                        qr.isCompleted = true;
                    }
                }
            }

            foreach (Vendor v in vendorRewardsList)
            {
                foreach (Quest q in questList)
                {
                    if (v.questName == q.questName && q.isCompleted)
                    {
                        v.isCompleted = true;
                    }
                }
            }
        }

        // This will update the config with client.txt file location as well as any other option that is saved
        private void UpdateConfig()
        {
        }

        private void CreateFileWatcher(string path)
        {
            // instead of FileSystemWatcher which can't update the file itself to view changes
            // make a streamreader with filesharereadwrite.
            // move streamreader to the end of file this allows us to know the lines that have changed since the streamreader was created
            this.logReader = new StreamReader(new FileStream(this.logFilePath + "\\Client.txt",
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite)); // , Encoding.GetEncoding("windows-1252"));
            this.logReader.ReadToEnd();
        }

        private void ReadLine(string line)
        {
            Console.WriteLine(line);
            if (line.Contains(") is now level "))
            {
                this.LevelChange(line);
            }
            else if (line.Contains("You have entered "))
            {
                this.AreaChange(line, line.IndexOf("You have entered ") + 17);
            }
            else if (line.Contains("@To a: "))
            {
                this.ExecuteCommand(line, line.IndexOf("@To a: ") + 7);
            }

            this.UpdateKnownInfo();
        }

        private void ExecuteCommand(string line, int index)
        {
            string command = line.Substring(index);
            int gemCount = this.GetAvailableGems().Count;
            int numbersFromString = this.GetNumbersFromString(command);
            if (command.Contains("g "))
            {
                if (this.GetNumbersFromString(command) < this.GetAvailableGems().Count)
                {
                    this.acquiredGems.Add(this.GetAvailableGems()[this.GetNumbersFromString(command)]);
                }
                else if (numbersFromString > gemCount && numbersFromString < GetAvailableVendorGems().Count)
                {
                    this.acquiredGems.Add(this.GetAvailableVendorGems()[this.GetNumbersFromString(command) - this.GetAvailableGems().Count]);
                }
                else
                {
                    Console.WriteLine("Number was not found");
                }
            }
        }

        private void LevelChange(string line)
        {
            try
            {
                characterLevel = Convert.ToInt32(line.Substring(line.Length - 2, 2));
            }
            catch (FormatException)
            {
                Console.WriteLine("Probably a whisper\nCharacter level was not valid");
            }
            // Also grab character class because level up says CHARACTERNAME (CLASSNAME) is now level x
            int indexStart = line.IndexOf("(");
            int indexEnd = line.IndexOf(")");

            this.characterClass = line.Substring(indexStart + 1, indexEnd - indexStart - 1);
        }

        private void AreaChange(string line, int indexOfArea)
        {
            this.currentArea = line.Substring(indexOfArea, line.Substring(indexOfArea).Length - 1);
            // Add the area to list of areas entered.
            this.areasEntered.Add(this.currentArea);
            // Update Quest data
            this.UpdateQuestData();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Whenever the timer ticks we read through the lines until the end of the file using the streamreader we created.
            // this allows us to read lines that have been added since the previous tick/creation
            if (this.logReader != null)
            {
                string line;

                while ((line = this.logReader.ReadLine()) != null)
                {
                    this.ReadLine(line);
                }
            }
        }

        private void ListBoxSupports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Store data of current selected items
            this.SavePreviousSelection();
            // Update the known info
            this.UpdateKnownInfo();
            // Save new current selection to config.
            this.UpdateConfig();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // toggle visibility of listboxes to freeze skill gem selection
            if (this.ListBoxType.Visibility == Visibility.Visible)
            {
                this.ListBoxType.Visibility = Visibility.Collapsed;
                this.ListBoxSkills.Visibility = Visibility.Collapsed;
                this.ListBoxWeapon.Visibility = Visibility.Collapsed;
                this.ListBoxSupports.Visibility = Visibility.Collapsed;
                foreach (var s in GridDisplay.Children)
                {
                    if (s is ScrollViewer)
                    {
                        ScrollViewer sv = s as ScrollViewer;
                        Grid.SetRow(sv, 0);
                    }
                }
            }
            else
            {
                this.ListBoxType.Visibility = Visibility.Visible;
                this.ListBoxSkills.Visibility = Visibility.Visible;
                this.ListBoxWeapon.Visibility = Visibility.Visible;
                this.ListBoxSupports.Visibility = Visibility.Visible;
                foreach (var s in GridDisplay.Children)
                {
                    if (s is ScrollViewer)
                    {
                        ScrollViewer sv = s as ScrollViewer;
                        Grid.SetRow(sv, 1);
                    }
                }
            }
        }
    }
}
/*
This file uses the Newtonsoft.Json library available under the MIT license as follows:

The MIT License (MIT)

Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
