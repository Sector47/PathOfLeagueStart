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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace PathOfLeagueStart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string logFilePath = File.ReadLines(@"Data/config.txt").Take(1).First()
            .Substring(13, File.ReadLines(@"Data/config.txt").Take(1).First().Length - 14);
        public MainWindow()
        {
            InitializeComponent();
            checkValidLogPath();
            UpdateGUI();
            downloadSkillGemData();
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
            //Grab the skill gem data from the poe wiki using cargoquery. Save it as xml/ Create skill gem items using this data.
            XmlDocument skillGemXml = new XmlDocument();
            skillGemXml.Load("https://pathofexile.gamepedia.com/api.php?action=cargoquery&tables=items,skill_gems,skill_levels,skill&fields=items.name,skill_levels.level_requirement,items.tags,skill.item_class_id_restriction%20&where=items.frame_type=%22gem%22%20AND%20skill_levels.level=%221%22&join_on=items.name=skill_gems._pageName,skill_gems._pageName=skill_levels._pageName,skill_gems._pageName=skill._pageName&limit=500&format=xml");
            skillGemXml.Save(@"Data/itemData.xml");
            /*using (WebClient client = new WebClient())
            {
                client.DownloadFile("https://pathofexile.gamepedia.com/api.php?action=cargoquery&tables=items,skill_gems,skill_levels,skill&fields=items.name,skill_levels.level_requirement,items.tags,skill.item_class_id_restriction%20&where=items.frame_type=%22gem%22%20AND%20skill_levels.level=%221%22&join_on=items.name=skill_gems._pageName,skill_gems._pageName=skill_levels._pageName,skill_gems._pageName=skill._pageName&limit=500&format=xml",
                    @"Data/itemData.xml");
            }*/
        }

        private void enterLogPath()
        {
            // prompt user for valid log path. Change that 
            logFilePath = Microsoft.VisualBasic.Interaction.InputBox("Please enter a valid location for the Path of exile log file. This is required for the program to work. It should be located in the game directory Grinding Gear Games\\Path of Exile\\logs ", "Title", "C:\\Program Files (x86)\\Grinding Gear Games\\Path of Exile\\logs");
            checkValidLogPath();
        }

        private void populateListBox(string selectedItem)
        {
            // will populate skill gem or support gem list box depending on name of item selected
            // ie: if you pass through magic it will ignore weapon choice and fill in all spells in skill list box
            // ie: if you pass through 2h axe and have selected melee it will fill the skill gems with all gems able to be used by 2h Axes in the skill gem list box
            // ie: if you pass through Sunder it will fill in all supports able to support Sunder in the support list box using the selected weapon from weapon list box
        }

        private void ListBoxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateGUI();
            this.UpdateConfig();
        }

        private void UpdateGUI()
        {
            // Update GUI
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
            // populate the list box 3 using the itemData.xml based on the item restrictions vs the selected weapon.
            ListBoxType.Items.Clear();
            //foreach()
            // Populate the support gems based on the selected skill gems.

        }

        private void UpdateConfig()
        {

        }
    }
}
