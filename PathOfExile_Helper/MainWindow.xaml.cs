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
using System.Windows.Threading;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PathOfLeagueStart.Data;
using PathOfLeagueStart.Classes;
using PathOfLeagueStart.Views;

namespace PathOfLeagueStart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isSocketsLocked;
        private APIDataFetcher dataFetcher;
        private Weapon selectedWeapon;
        private Area currentArea = new Area();
        private int currentLevel;
        private Quest currentQuest;
        private List<String[]> recentWhispers;
        private StreamReader logReader;
        private SettingsDisplayData settings;
        private DispatcherTimer dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();
            CheckConfigFile();
            AddSocketListeners();
            dataFetcher = FetchData();
            CreateFileWatcher(settings.getClientTxtFilePath);
            StartDispatcherTimer();
        }

        

        private void StartDispatcherTimer()
        {
            // set our dispatchertimer to read the client.txt on a 1 second timer.
            dispatcherTimer = new DispatcherTimer();

            // Add the handler for the tick
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);

            // set timer interval to 1 seconds and start timer
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }

        private void CheckConfigFile()
        {
            // Create a settingsData class for holding our setting information.
            // This will allow us to fill in the relevant fields based on the settings chosen.
            // It will also allow us to update those settings.
            settings = new SettingsDisplayData();
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
                    socketToBeClicked.MouseDown += new MouseButtonEventHandler(SocketIcon_Clicked);
                }                
            }
        }

        private APIDataFetcher FetchData()
        {
            return new APIDataFetcher();

        }

        private void CreateFileWatcher(string path)
        {
            // instead of FileSystemWatcher which can't update the file itself to view changes
            // make a streamreader with filesharereadwrite.
            // move streamreader to the end of file this allows us to know the lines that have changed since the streamreader was created
            logReader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite)); // , Encoding.GetEncoding("windows-1252"));
            logReader.ReadToEnd();

        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Whenever the timer ticks we read through the lines until the end of the file using the streamreader we created.
            // this allows us to read lines that have been added since the previous tick/creation
            if (logReader != null)
            {
                string line;

                while ((line = logReader.ReadLine()) != null)
                {
                    ReadLine(line);
                }
            }
        }

        private void ReadLine(string line)
        {
            Logger.LogDebug("Reading Line: " + line);
            if (line.Contains(") is now level "))
            {
                //LevelChange(line);
            }
            else if (line.Contains("You have entered "))
            {
                String areaEnter = "You have entered ";
                int indexOfArea = line.IndexOf(areaEnter) + areaEnter.Length;
                // to get areaName, we go to index of You have entered in the string, and take the remaining string in the line by going to end of line with a substring.
                // Line - index location - length of "You have entered " and the line ends with a period we don't want, so -1;
                string currentAreaName = line.Substring(indexOfArea, (line.Length - 1 - areaEnter.Length - line.IndexOf(areaEnter)));
                SetCurrentArea(currentAreaName);
                Logger.LogDebug("Entering Area: " + currentArea);
                UpdateDataInUI();
            }
            else if (line.Contains("@To a: "))
            {
                //ExecuteCommand(line, line.IndexOf("@To a: ") + 7);
            }

            //UpdateKnownInfo();
        }

        private void SetCurrentArea(string currentAreaName)
        {
            currentArea = dataFetcher.AreaList.FirstOrDefault((a => a.name == currentAreaName));
        }

        private void SocketIcon_Clicked(object sender, RoutedEventArgs e)
        {
            if (!isSocketsLocked)
            {
                // On socket click if not locked, prompt with a custom view with all skill gems. Four columns str, int, dex, null(detonate mines, portal)
                Image socketClicked = sender as Image;

                GemSelectionWindow gemSelectionWindow = new GemSelectionWindow(dataFetcher.AllSkillGems);
                if ((bool)gemSelectionWindow.ShowDialog())
                {
                    Gem selectedGem = gemSelectionWindow.GetSelectedGem();
                    switch (selectedGem.primary_attribute)
                    {
                        case "strength":
                            {
                                // Change the color of the socket to match the gem.
                                socketClicked.Source = new BitmapImage(new Uri(@"/Assets/redSocket.png", UriKind.Relative));
                                GetGridLocation(socketClicked).ElementAt(0);

                                // Add another image to show the socket is filled.
                                // TODO change this based on skill vs support gem
                                Image image = new Image();
                                image.Source = new BitmapImage(new Uri(@"/Assets/redGem.png", UriKind.Relative));
                                gridEquipmentIcons.Children.Add(image);
                                Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));

                                // Make the images horizontal alignment match the alignment of the socket clicked.
                                image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                // Make it so you can still click the socket behind it.
                                image.IsHitTestVisible = false;

                                // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                image.Tag = selectedGem.name;
                                break;
                            }
                        case "dexterity":
                            {
                                socketClicked.Source = new BitmapImage(new Uri(@"/Assets/greenSocket.png", UriKind.Relative));
                                GetGridLocation(socketClicked).ElementAt(0);

                                // Add another image to show the socket is filled.
                                // TODO change this based on skill vs support gem
                                Image image = new Image();
                                image.Source = new BitmapImage(new Uri(@"/Assets/greenGem.png", UriKind.Relative));
                                gridEquipmentIcons.Children.Add(image);
                                Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));

                                // Make the images horizontal alignment match the alignment of the socket clicked.
                                image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                // Make it so you can still click the socket behind it.
                                image.IsHitTestVisible = false;

                                // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                image.Tag = selectedGem.name;
                                break;
                            }
                        case "intelligence":
                            {
                                socketClicked.Source = new BitmapImage(new Uri(@"/Assets/blueSocket.png", UriKind.Relative));
                                GetGridLocation(socketClicked).ElementAt(0);

                                // Add another image to show the socket is filled.
                                // TODO change this based on skill vs support gem
                                Image image = new Image();
                                image.Source = new BitmapImage(new Uri(@"/Assets/blueGem.png", UriKind.Relative));
                                gridEquipmentIcons.Children.Add(image);
                                Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));

                                // Make the images horizontal alignment match the alignment of the socket clicked.
                                image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                // Make it so you can still click the socket behind it.
                                image.IsHitTestVisible = false;

                                // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                image.Tag = selectedGem.name;
                                break;
                            }
                        default:
                            {
                                // Special Case: If selected gem is corrupting fever, set to red.
                                if (selectedGem.name == "Corrupting Fever")
                                {
                                    socketClicked.Source = new BitmapImage(new Uri(@"/Assets/redSocket.png", UriKind.Relative));
                                    GetGridLocation(socketClicked).ElementAt(0);

                                    // Add another image to show the socket is filled.
                                    // TODO change this based on skill vs support gem
                                    Image image = new Image();
                                    image.Source = new BitmapImage(new Uri(@"/Assets/redGem.png", UriKind.Relative));
                                    gridEquipmentIcons.Children.Add(image);
                                    Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                    Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));

                                    // Make the images horizontal alignment match the alignment of the socket clicked.
                                    image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                    // Make it so you can still click the socket behind it.
                                    image.IsHitTestVisible = false;

                                    // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                    image.Tag = selectedGem.name;
                                }
                                else
                                {
                                    socketClicked.Source = new BitmapImage(new Uri(@"/Assets/whiteSocket.png", UriKind.Relative));
                                    GetGridLocation(socketClicked).ElementAt(0);

                                    // Add another image to show the socket is filled.
                                    // TODO change this based on skill vs support gem
                                    Image image = new Image();
                                    image.Source = new BitmapImage(new Uri(@"/Assets/noneGem.png", UriKind.Relative));
                                    gridEquipmentIcons.Children.Add(image);
                                    Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                    Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));

                                    // Make the images horizontal alignment match the alignment of the socket clicked.
                                    image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                    // Make it so you can still click the socket behind it.
                                    image.IsHitTestVisible = false;

                                    // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                    image.Tag = selectedGem.name;
                                }

                                
                                break;
                            }
                    }

                    Logger.LogDebug("Selected Gem: " + selectedGem);

                }
            }
        }

        private int[] GetGridLocation(object objectToFind)
        {
            UIElement uiElement = (UIElement)objectToFind;
            int[] result = new int[2];
            result[0] = Grid.GetColumn(uiElement);
            result[1] = Grid.GetRow(uiElement);

            return result;
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

        private void changeWeaponButton_Click(object sender, RoutedEventArgs e)
        {
            WeaponSelectionWindow weaponSelectionWindow = new WeaponSelectionWindow(dataFetcher.WeaponList);
            if ((bool)weaponSelectionWindow.ShowDialog())
            {
                selectedWeapon = weaponSelectionWindow.GetSelectedWeapon();
                changeWeaponButton.Content = selectedWeapon;
                Logger.LogDebug("Selected weapon: " + selectedWeapon);
            }

        }

        private void UpdateDataInUI()
        {
            AreaNameTextBlock.Text = currentArea.name;
            AreaLevelTextBlock.Text = currentArea.areaLevel;
            
        }
    }
}
