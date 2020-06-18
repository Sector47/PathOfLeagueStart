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
using System.Xml;
using Newtonsoft.Json.Linq;

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

        public MainWindow()
        {
            InitializeComponent();
            this.checkValidLogPath();
            this.populateListBox();
            this.downloadSkillGemData();
        }

        private void checkValidLogPath()
        {
            // TODO fix check for valid file path. Maybe check to make the sure the file is the log file?
            if (logFilePath == null || logFilePath == "ENTERLOGPATHHERE")
            {
                this.enterLogPath();
            }
        }

        private void downloadSkillGemData()
        {
            // download json file.
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
        }

        private void enterLogPath()
        {
            // prompt user for valid log path. Change that 
            logFilePath = Microsoft.VisualBasic.Interaction.InputBox("Please enter a valid location for the Path of exile log file. This is required for the program to work. It should be located in the game directory Grinding Gear Games\\Path of Exile\\logs ", "Title", "C:\\Program Files (x86)\\Grinding Gear Games\\Path of Exile\\logs");
            this.checkValidLogPath();
        }

        private void populateListBox()
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
            this.savePreviousSelection();
            // Clear the skills and supports
            ListBoxSkills.Items.Clear();
            ListBoxSupports.Items.Clear();
            // Update the list boxes with current valid options
            this.updateGUI();
            // restore previous selections if possible
            this.restorePreviousSelection();
            // Update the known info
            this.updateKnownInfo();
            // Save new current selection to config.
            this.UpdateConfig();
        }

        private void ListBoxSkills_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear the supports
            ListBoxSupports.Items.Clear();
            // Store current selected skills for updating known info
            this.savePreviousSelection();
            // Update the list boxes with current valid options
            // overload update gui to only update the supports listbox.
            this.updateGUI("skills");
            // Update the known info
            this.updateKnownInfo();
            // Save new current selection to config.
            this.UpdateConfig();
        }

        private void ListBoxWeapon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // save previous selections
            this.savePreviousSelection();
            // Clear the skills and supports
            ListBoxSkills.Items.Clear();
            ListBoxSupports.Items.Clear();
            // Update the list boxes with current valid options
            this.updateGUI();
            // restore previous selections if possible
            this.restorePreviousSelection();
            // Update the known info
            this.updateKnownInfo();
            // Save new current selection to config.
            this.UpdateConfig();
        }

        private void savePreviousSelection()
        {
            // Clear temp selected skill gems
            selectedSkillGems.Clear();
            selectedSupportGems.Clear();
            // First store the previous selected active skills to select them if they are still available and support gems.
            foreach (var selectedItem in ListBoxSkills.SelectedItems)
            {
                selectedSkillGems.Add(getGem(selectedItem.ToString()));
            }
            foreach (var selectedItem in ListBoxSupports.SelectedItems)
            {
                selectedSupportGems.Add(getGem(selectedItem.ToString()));
            }
        }

        private void restorePreviousSelection()
        {
            // List to store which items to select after looping through listbox because you can't continue searching through a listbox after modifying it.
            List<string> foundMatches = new List<string>();
            // Reselect items that were previously selected if they are still valid
            foreach (Gem g in selectedSkillGems)
            {
                foreach (var i in ListBoxSkills.Items)
                {
                    Console.Write("asdlf");
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
            // Clear saved selected items and refresh them.
            this.savePreviousSelection();
        }
        private void updateGUI()
        {
            // Update GUI
            // populate the list box 3 using the downloaded json
            // Insert any gems that match the selected weapon tag. If magic is selected list all spells instead.
            foreach (Gem g in allSkillGems)
            {
                if (g.gem_tags.Contains("Support"))
                {
                    ListBoxSupports.Items.Add(g.name);
                }
                else if(ListBoxWeapon.SelectedItem != null)
                {
                    string test = ListBoxWeapon.SelectedItem.ToString();
                    if(g.item_class_id_restriction.Contains(ListBoxWeapon.SelectedItem.ToString()))
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
        private void updateGUI(string listBoxName)
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
        private void updateKnownInfo()
        {
            // Fill known information text block with known information.
            TextBlockKnownInformation.Text =
                "Character Level: " + /*insertcharacterlevel*/"\n\n";
            // Show current zone
            TextBlockKnownInformation.Text += "Current Zone: " + "\n\n";
            // show next area

            // show skill gems available in town this will go in seperate text block
            TextBlockAvailableGems.Text = "These gems are available in town from vendors:\n" + "\n" +
                                          "\nThese gems are available as a quest reward\n";
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
        private Gem getGem(string gemName)
        {
            foreach (Gem g in allSkillGems)
            {
                if (g.name == gemName)
                    return g;
            }

            return null;
        }

        private void UpdateConfig()
        {

        }

        private void ListBoxSupports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Store data of current selected items
            this.savePreviousSelection();
            // Update the known info
            this.updateKnownInfo();
            // Save new current selection to config.
            this.UpdateConfig();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // toggle visibility of listboxes to freeze skill gem selection
            if (ListBoxType.Visibility == Visibility.Visible)
            {
                ListBoxType.Visibility = Visibility.Collapsed;
                ListBoxSkills.Visibility = Visibility.Collapsed;
                ListBoxWeapon.Visibility = Visibility.Collapsed;
                ListBoxSupports.Visibility = Visibility.Collapsed;
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
                ListBoxType.Visibility = Visibility.Visible;
                ListBoxSkills.Visibility = Visibility.Visible;
                ListBoxWeapon.Visibility = Visibility.Visible;
                ListBoxSupports.Visibility = Visibility.Visible;
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
