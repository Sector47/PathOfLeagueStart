using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void populateListBox()
        {
            string line;
            int columnToWriteTo;
            System.IO.StreamReader file = new StreamReader("listBoxData.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("COL"))
                {
                    //columnToWriteTo = 
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
