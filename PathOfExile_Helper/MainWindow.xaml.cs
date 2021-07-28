using System;
using System.Collections.Generic;
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
using PathOfExile_Helper.Data;
using PathOfExile_Helper.Classes;

namespace PathOfExile_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isSocketsLocked;
        public MainWindow()
        {
            InitializeComponent();
            CheckConfigFile();
            AddSocketListeners();
        }

        private void CheckConfigFile()
        {
            // Create a settingsData class for holding our setting information.
            // This will allow us to fill in the relevant fields based on the settings chosen.
            // It will also allow us to update those settings.
            SettingsDisplayData settings = new SettingsDisplayData();
            Logger.Log("The settings were successfully loaded");
        }

        private void AddSocketListeners()
        {
            foreach(object o in gridEquipmentIcons.Children)
            {
                Image socketToBeClicked = new Image();
                if(o is Image)
                {
                    socketToBeClicked = o as Image; 
                }
                if (socketToBeClicked.Source != null && socketToBeClicked.Source.ToString().Contains("Socket"))
                {
                    socketToBeClicked.MouseDown += new MouseButtonEventHandler(SocketIconClicked);
                }                
            }
        }
            
        private void SocketIconClicked(object sender, RoutedEventArgs e)
        {
            if (!isSocketsLocked)
            {
                // On socket click if not locked, prompt with a custom view with all skill gems. Four columns str, int, dex, null(detonate mines, portal)
                Image socketClicked = sender as Image;

                Random r = new Random();
                int i = r.Next(0, 3);
                switch (i)
                {
                    case 1:
                        {
                            socketClicked.Source = new BitmapImage(new Uri(@"/Assets/redSocket.png", UriKind.Relative));
                            break;
                        }
                    case 2:
                        {
                            socketClicked.Source = new BitmapImage(new Uri(@"/Assets/greenSocket.png", UriKind.Relative));
                            break;
                        }
                    default:
                        {
                            socketClicked.Source = new BitmapImage(new Uri(@"/Assets/blueSocket.png", UriKind.Relative));
                            break;
                        }
                }

                
                
            }            
        }

        private void lockGemsButton_Click(object sender, RoutedEventArgs e)
        {
            // Flip our bool for if the sockets are locked.
            isSocketsLocked = !isSocketsLocked;

            // And change the text of our button.
            Button b = (Button)sender;
            if (isSocketsLocked)
            {
                b.Content = "Unlock Gems";
            }
            else
            {
                b.Content = "Lock Gems";
            }
            
        }
    }
}
