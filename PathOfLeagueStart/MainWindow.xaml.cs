using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            populateListBox();
            checkValidLogPath();
        }

        private void checkValidLogPath()
        {
            // TODO fix check for valid file path. Maybe check to make the sure the file is the log file?
            if (logFilePath == null || logFilePath == "ENTERLOGPATHHERE")
            {
                this.enterLogPath();
            }
        }

        private void enterLogPath()
        {
            // prompt user for valid log path. Change that 
            logFilePath = Microsoft.VisualBasic.Interaction.InputBox("Please enter a valid location for the Path of exile log file. This is required for the program to work. It should be located in the game directory Grinding Gear Games\\Path of Exile\\logs ", "Title", "C:\\Program Files (x86)\\Grinding Gear Games\\Path of Exile\\logs");
            checkValidLogPath();
        }

        private void populateListBox()
        {
            string line;
            int columnToWriteTo = 0;
            System.IO.StreamReader file = new StreamReader(@"Data/listBoxData.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("COL"))
                {
                    columnToWriteTo = Convert.ToInt32(Regex.Replace(line, "[^0-9]", string.Empty));
                }
                else
                {
                    switch (columnToWriteTo)
                    {
                        case 1:
                            ListBoxType.Items.Add(line);
                            break;
                        case 2:
                            ListBoxWeapon.Items.Add(line);
                            break;
                        case 3:
                            ListBoxSkills.Items.Add(line);
                            break;
                        default:
                            break;
                    }
                }
            }
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
        }

        private void UpdateConfig()
        {

        }
    }
}
