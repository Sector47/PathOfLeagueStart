﻿using System;
using System.ComponentModel;
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
using System.Windows.Media.Animation;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using FMUtils.KeyboardHook;
using System.Configuration;
using System.Threading;

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
        private List<Area> areasEntered = new List<Area>();
        private int currentLevel = 1;
        private string characterName;
        private string characterClass = "Scion";
        private string lastWhisperName;
        private Quest currentQuest;
        private List<string> GemsAvailable = new List<string>();
        private List<Quest> allQuests = new List<Quest>();
        private List<string[]> recentWhispers = new List<string[]>();
        // Binding for our whispers listview
        private System.ComponentModel.BindingList<string> listItems = new System.ComponentModel.BindingList<string>();
        private StreamReader logReader;
        private SettingsDisplayData settings;
        private DispatcherTimer dispatcherTimer;
        private Hook hotkeyHook = new Hook("Global Action Hook");
        private bool inSettings = false;



        private List<Quest> startedQuests = new List<Quest>();
        private List<Quest> completedQuests = new List<Quest>();



        public MainWindow()
        {
            InitializeComponent();
            CheckConfigFile();
            AddSocketListeners();
            dataFetcher = FetchData();
            CreateFileWatcher(settings.ClientTxtFilePath);
            StartDispatcherTimer();
            FillQuestList();
            SetZIndexDefault();
            UpdateSettingsUI();

            AttachKeyboardInterceptor(hotkeyHook);
            

            

            WhisperLogView.ItemsSource = listItems;
            UpdateDataInUI();
        }


        private void AttachKeyboardInterceptor(Hook hookToAttach)
        {
            hookToAttach.KeyDownEvent += CheckForHotKeyPress;
        }

        private void CheckForHotKeyPress(KeyboardHookEventArgs e)
        {
            if (!inSettings)
            {
                string hotKeyPressed = settings.GetHotKey(e.Key.ToString());
                if (hotKeyPressed != string.Empty)
                {
                    switch (hotKeyPressed)
                    {
                        case "GoToHideoutHotkey":
                            {
                                GoToHideout();
                                break;
                            }
                        case "LogOutHotkey":
                            {
                                LogOut();
                                break;
                            }
                        case "WhisperBackHotkey":
                            {
                                WhisperBack();
                                break;
                            }
                        case "InviteLastPlayerHotkey":
                            {
                                InviteLastPlayer();
                                break;
                            }
                        case "InviteFriend1Hotkey":
                            {
                                InviteFriend(1);
                                break;
                            }
                        case "InviteFriend2Hotkey":
                            {
                                InviteFriend(2);
                                break;
                            }
                        case "InviteFriend3Hotkey":
                            {
                                InviteFriend(3);
                                break;
                            }
                        case "CustomHotkey":
                            {
                                CustomHotKey();
                                break;
                            }

                        default:
                            {
                                Logger.LogDebug("Could not find a method to run for the hotkey: " + hotKeyPressed);
                                break;
                            }
                    }
                }
            }
            
           
            Logger.LogDebug(e.Key.ToString());
        }

        private void WhisperBack()
        {
            
            SendKeys.SendWait("~");
            System.Windows.Clipboard.SetText("@" + lastWhisperName + " ");
            SendKeys.SendWait("^{v}");
            Task.Delay(50).ContinueWith(t => SendBackSpace());
            SendKeys.SendWait(" ");
            SendBackSpace();
            
        }

        private void SendBackSpace()
        {
            SendKeys.SendWait("{BS}");
        }

        private void InviteLastPlayer()
        {
            if(!string.IsNullOrEmpty(lastWhisperName))
            {
                SendKeys.SendWait("~");
                System.Windows.Clipboard.SetText("/invite " + lastWhisperName);
                SendKeys.SendWait("^{v}");
                SendKeys.SendWait("~");
            }            
        }

        private void InviteFriend(int friendToInvite)
        {
            string friend = GetFriend(friendToInvite);            
            if (!string.IsNullOrEmpty(friend))
            {                
                SendKeys.SendWait("~");
                System.Windows.Clipboard.SetText("/invite " + friend);
                SendKeys.SendWait("^{v}");
                SendKeys.SendWait("~");
            }
            
        }

        private void CustomHotKey()
        {
            if (!string.IsNullOrEmpty(settings.CustomFunction))
            {
                SendKeys.SendWait("~");
                System.Windows.Clipboard.SetText(settings.CustomFunction);
                SendKeys.SendWait("^{v}");
                SendKeys.SendWait("~");
            }
        }


        private string GetFriend(int friendToInvite)
        {
            switch (friendToInvite)
            {
                case 1:
                    return settings.Friend1;
                case 2:
                    return settings.Friend2;
                case 3:
                    return settings.Friend3;
                default:
                    break;
            }
            return string.Empty;
        }




        private void LogOut()
        {
            SendKeys.SendWait("~");
            System.Windows.Clipboard.SetText("/exit");
            SendKeys.SendWait("^{v}");
            SendKeys.SendWait("~");
        }



         public void GoToHideout()
         {
             SendKeys.SendWait("~");
             System.Windows.Clipboard.SetText("/Hideout");
             SendKeys.SendWait("^{v}");
             SendKeys.SendWait("~");
         }

         private void InviteLastWhisper()
         {
             SendKeys.SendWait("~");
             System.Windows.Clipboard.SetText("/" + lastWhisperName);
             SendKeys.SendWait("^{v}");
             SendKeys.SendWait("~");
         }


        private void StartDispatcherTimer()
        {
            // set our dispatchertimer to read the client.txt on a 1 second timer.
            dispatcherTimer = new DispatcherTimer();

            // Add the handler for the tick
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);

            // set timer interval to 1 seconds and start timer
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
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

        private void FillQuestList()
        {
            string line;
            string[] lineStrings = new string[4];
            int count = 0;



            // Get our file location to store data
            string questProgressionFileLocation = @"Data\questProgression.txt";

            System.IO.StreamReader streamReader = new StreamReader(questProgressionFileLocation);

            while ((line = streamReader.ReadLine()) != null)
            {
                lineStrings = line.Split('.');
                allQuests.Add(new Quest(lineStrings[0], lineStrings[1], lineStrings[2], lineStrings[3]));
                count++;
            }

            foreach (Quest q in allQuests)
            {
                Logger.LogDebug(q.questName + " " + q.initialZone + " " + q.finishZone + " " + q.prerequisiteQuest);
            }
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
                    UpdateDataInUI();
                }
            }
        }

        private void ReadLine(string line)
        {
            Logger.LogDebug("Reading Line: " + line);
            //if (line.Contains("@To "))
            {
                // whisper message sent ignore this for now, later we can add custom commands through here maybe?
                // Probably just do commands through hotkeys or through local messages instead.
            }
             if (line.Contains("@From "))
            {
                UpdateWhispers(line);
            }
            else if (line.Contains(") is now level "))
            {
                UpdateLevel(line);
            }
            else if (line.Contains("Generating level "))
            {
                UpdateArea(line);
            }
            else if (line.Contains("@To a: "))
            {
                //ExecuteCommand(line, line.IndexOf("@To a: ") + 7);
            }
        }

        private void UpdateArea(string line)
        {
            /*
             * Note currently using the client.txt debug line of generating level ## area "" with seed if this changes area loading will be broken for a bit. Commenting out the old way which is unable to determine area level. IF I have to revert to this I can fix this by assuming you aren't in an area over 10 levels above you when i setcurrentarea()
             * 
            // To avoid magic numbers we will count the length of our string.
            String areaEnter = "You have entered ";
            int indexOfArea = line.IndexOf(areaEnter) + areaEnter.Length;
            // to get areaName, we go to index of You have entered in the string, and take the remaining string in the line by going to end of line with a substring.
            // Line - index location - length of "You have entered " and the line ends with a period we don't want, so -1;
            string currentAreaName = line.Substring(indexOfArea, (line.Length - 1 - areaEnter.Length - line.IndexOf(areaEnter)));
            SetCurrentArea(currentAreaName);
            Logger.LogDebug("Entering Area: " + currentArea);
            */
            // To avoid magic numbers we will count the length of our string.
            String areaEnter = "Generating level ";
            int indexOfAreaLevel = line.LastIndexOf(areaEnter) + "Generating level ".Length;
            // Magic number here of 2 is from the fact that area levels are at most 2 characters long. We are parsing to int so we will either get "##" or "# " which will parse just fine.
            int maxLengthOfAreaLevel = 2;
            int currentAreaLevel = GetNumbersFromString(line.Substring(indexOfAreaLevel, maxLengthOfAreaLevel));
            // to get areaName, we go to index of You have entered in the string, and take the remaining string in the line by going to end of line with a substring.
            // Line - index location - length of "You have entered " and the line ends with a period we don't want, so -1;
            // 
            // Example area generation line. So we can get first index of " and last index of" for our substring perfectly.
            // Generating level 48 area "The Western Forest" with 
            // Substring will count the escape character so we have to add 1 and subtract 1 for that, or we can replace them with a non escaped character
            string currentAreaName = line.Substring(line.IndexOf("\"") + 1, line.LastIndexOf("\"") - line.IndexOf("\"") - 1);
            SetCurrentArea(currentAreaName, currentAreaLevel);
            Logger.LogDebug("Entering Area: " + currentArea);

        }

        private int GetNumbersFromString(string text)
        {
            int foundNumber = Convert.ToInt32(Regex.Replace(text, "[^0-9]", string.Empty));
            return foundNumber;
        }

        private void UpdateLevel(string line)
        {
            // Check if character name has not been set yet, if so set the character name. This can also be done from the settings.
            if (string.IsNullOrEmpty(characterName))
            {
                int start = line.LastIndexOf(": ") + ": ".Length;
                int end = line.LastIndexOf(" (");
                characterName = line.Substring(start, end - start);
            }
            // once we have character name we can see if the line is for them or a party member
            if (!string.IsNullOrEmpty(characterName) && line.Contains(characterName) && line.Contains(") is now level "))
            {
                // Set our character Class
                int start = line.IndexOf("(") + "(".Length;
                int end = line.LastIndexOf(")");
                characterClass = line.Substring(start, end - start);


                // To avoid magic numbers we will count the length of our string.
                string nowLevel = "now level ";
                int indexOfLevel = line.IndexOf(nowLevel) + nowLevel.Length;

                // to get current level we just read the remaining end of the line.                
                currentLevel = int.Parse(line.Substring(indexOfLevel));
                Logger.LogDebug(CharacterNameTextBlock + " is now level: " + currentLevel.ToString());
            }
        }

        private void UpdateWhispers(string line)
        {
            // whisper message received
            // Add our whisper to recent whispers for displaying in the scrollviewer.
            // record the name of who sent the whisper for replying / quick invite
            int start = line.IndexOf("@From ") + "@From ".Length;
            // For getting the end of their name we need to look for the first : after the @from message
            int end = line.Substring(start).IndexOf(":");
            lastWhisperName = line.Substring(start, end);
            string lastMessage = line.Substring(end + start + ":".Length);
            string[] compiledMessage = new string[2] { lastWhisperName, lastMessage };

            recentWhispers.Add(compiledMessage);
        }

        private void UpdateQuestData()
        {
            // Grab data from entered areas to see if you've received any new quests
            // Add them to list of current quests, ignoring quests that were already added.

            // Complete quests when they are done, then while we are looping through we mark them down once when they are completed
            List<Vendor> listVr = new List<Vendor>();
            List<Quest> listQr = new List<Quest>();


            foreach (Quest q in allQuests)
            {
                if (areasEntered.Find(a => a.name == q.finishZone) != null && !q.isCompleted)
                {
                    q.isCompleted = true;
                    completedQuests.Add(q);
                    // All vendor rewards that match the quest that was completed
                    listVr = dataFetcher.VendorRewardsList.Where(vr => vr.questName == q.questName && vr.classes.Contains(characterClass)).ToList();
                    listQr = dataFetcher.QuestRewardsList.Where(qr => qr.questName == q.questName && qr.classes.Contains(characterClass)).ToList();
                }

                if (areasEntered.Find(a => a.name == q.initialZone && completedQuests.Exists(cq => q.prerequisiteQuest == cq.questName || q.prerequisiteQuest == string.Empty)) != null && !q.isStarted && !q.isCompleted)
                {
                    q.isStarted = true;
                    currentQuest = q;
                }
            }

            List<string> compiledGemNames = new List<string>();
            foreach(Quest q in listQr)
            {
                if(compiledGemNames.Find(g => g == q.reward) == null)
                {
                    compiledGemNames.Add(q.reward);
                }
            }
            foreach (Vendor v in listVr)
            {
                if (compiledGemNames.Find(g => g == v.reward) == null)
                {
                    compiledGemNames.Add(v.reward);
                }
            }
            HighLightGems(compiledGemNames);
            
        }

        private void HighLightGems(List<string> gemsToHighlight)
        {
            foreach(string gem in gemsToHighlight)
            {
                CreateHighlight(gem);
            }
        }

        private void CreateHighlight(string gemName)
        {

            List<Image> imagesToAddHighlightTo = new List<Image>();
            foreach(Object obj in gridEquipmentIcons.Children)
            {
                if(obj is Image)
                {
                    Image img = obj as Image;
                    if(img.Source.ToString().Contains("Socket") && img.Tag != null && img.Tag.ToString() == gemName)
                    {
                        imagesToAddHighlightTo.Add(img);
                        if (!GemsAvailable.Contains(gemName))
                        {
                            GemsAvailable.Add(gemName);
                        }
                    }
                }
            }
            foreach(Image img in imagesToAddHighlightTo)
            {
                Image glowImage = new Image();
                glowImage.Source = new BitmapImage(new Uri(@"/Assets/glowHighlight.png", UriKind.Relative));



                // set location in grid to same as the img we are looking at.
                Grid.SetColumn(glowImage, GetGridLocation(img).ElementAt(0));
                Grid.SetRow(glowImage, GetGridLocation(img).ElementAt(1));
                // Create Highlight on socket at same grid location. Delete it on click :)img.Source = new BitmapImage(new Uri(@"/Assets/weaponIcon.png", UriKind.Relative));
                Grid.SetZIndex(glowImage, Grid.GetZIndex(img));
                glowImage.HorizontalAlignment = img.HorizontalAlignment;
                glowImage.Stretch = Stretch.Uniform;
                glowImage.Margin = new Thickness(-img.RenderSize.Width/4);



                gridEquipmentIcons.Children.Add(glowImage);


                // Set an animation for the image to decrease opacity, reverse and repeat forever
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.To = 0;
                doubleAnimation.Duration = TimeSpan.FromSeconds(2);
                doubleAnimation.FillBehavior = FillBehavior.Stop;
                doubleAnimation.AutoReverse = true;
                doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

                doubleAnimation.Completed += (s, a) => glowImage.Opacity = .25;
                glowImage.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);





                // Add event handler to the glow
                glowImage.MouseDown += new System.Windows.Input.MouseButtonEventHandler(HighlightGem_Clicked);
                glowImage.Tag = img.Tag;
                ToolTipService.SetToolTip(glowImage, glowImage.Tag);
            }

            
        }

        private void SetXpPenalty()
        {
            // Check if character level is too far below zone;
            // Path of exile xp penalty has a safe distance before it starts being calculated.
            int safeDistance = (3 + (currentLevel / 16));
            int currentAreaLevel = 0;
            if(currentArea != null && currentArea.areaLevel != null)
            {
                currentAreaLevel = int.Parse(currentArea.areaLevel);
            }

            int effectiveDifference = Math.Max(Math.Abs(currentLevel - currentAreaLevel) - safeDistance, 0);

            if (Math.Abs(currentLevel - currentAreaLevel) > safeDistance)
            {
                double xpPenalty =
                    Math.Max(Math.Pow(
                        ((currentLevel + 5) /
                         (currentLevel + 5 + Math.Pow(effectiveDifference, 2.5))), 1.5), 0.01);

                string stringFormattedXpPenaltyValue = String.Format("Xp Penalty: {0:P2}", xpPenalty);

                if (currentLevel > currentAreaLevel)
                {
                    TextBlock formattedXpPenalty = new TextBlock();
                    formattedXpPenalty.Inlines.Add("Too high level, ");
                    formattedXpPenalty.Inlines.Add(new Bold(new Run(stringFormattedXpPenaltyValue)));                    
                    XpPenaltyTextBlock.Text = string.Empty;
                    XpPenaltyTextBlock.Inlines.Add(formattedXpPenalty);
                }

                if (currentLevel < currentAreaLevel)
                {
                    TextBlock formattedXpPenalty = new TextBlock();
                    formattedXpPenalty.Inlines.Add("Too low level, ");
                    formattedXpPenalty.Inlines.Add(new Bold(new Run(stringFormattedXpPenaltyValue)));
                    XpPenaltyTextBlock.Text = string.Empty;
                    XpPenaltyTextBlock.Inlines.Add(formattedXpPenalty);
                }
                XpPenaltyDockPanel.Background = new SolidColorBrush(Color.FromRgb(253, 173, 92));
            }
            else if (Math.Abs(currentLevel - currentAreaLevel) == safeDistance)
            {
                if((currentLevel - currentAreaLevel) < 0)
                {
                    TextBlock formattedXpPenalty = new TextBlock();
                    formattedXpPenalty.Inlines.Add("You are at the safe distance low end");
                    XpPenaltyTextBlock.Text = string.Empty;
                    XpPenaltyTextBlock.Inlines.Add(formattedXpPenalty);
                }
                else
                {
                    TextBlock formattedXpPenalty = new TextBlock();
                    formattedXpPenalty.Inlines.Add("You are at the safe distance high end");
                    XpPenaltyTextBlock.Text = string.Empty;
                    XpPenaltyTextBlock.Inlines.Add(formattedXpPenalty);
                }
                
                XpPenaltyDockPanel.Background = new SolidColorBrush(Colors.LightYellow);
            }
            else
            {
                int distance = Math.Abs(Math.Abs(currentLevel - currentAreaLevel) - safeDistance);
                TextBlock formattedXpPenalty = new TextBlock();
                formattedXpPenalty.Inlines.Add("You are within the safe distance by " + distance);
                XpPenaltyTextBlock.Text = string.Empty;
                XpPenaltyTextBlock.Inlines.Add(formattedXpPenalty);
                XpPenaltyDockPanel.Background = new SolidColorBrush(Colors.White);
            }
        }

        /// <summary>
        /// gets the Area data for a given area's name and sets our current area to it.
        /// </summary>
        /// <param name="currentAreaName"> The area string we are in. </param>
        private void SetCurrentArea(string currentAreaName)
        {
            // the -current level < 10 means that the area level can't be greater than 10 levels above the character. this hopefully fixes area level
            currentArea = dataFetcher.AreaList.FirstOrDefault((a => a.name == currentAreaName && int.Parse(a.areaLevel) - currentLevel < 10));
            // If we found the area set it, if not set to twilight strand to avoid issues.
            if(currentArea != null)
            {
                areasEntered.Add(currentArea);
                UpdateQuestData();
            }
        }

        /// <summary>
        /// Overloaded version of SetCurrentArea that is used when we know the arealevel, which for now is always but if te debug changes we will go back to the nonoverloaded method.
        /// </summary>
        /// <param name="currentAreaName"> The area string we are in. </param>
        /// <param name="areaLevel"> The level of the area we are in. </param>
        private void SetCurrentArea(string currentAreaName, int areaLevel)
        {
            currentArea = dataFetcher.AreaList.FirstOrDefault((a => a.name == currentAreaName && int.Parse(a.areaLevel) == areaLevel));
            // If we found the area set it, if not set to twilight strand to avoid issues.
            if (currentArea != null)
            {
                areasEntered.Add(currentArea);
                UpdateQuestData();
            }
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

                                // Remove previous gem icons
                                RemovePreviousGemImage(GetGridLocation(socketClicked).ElementAt(0), GetGridLocation(socketClicked).ElementAt(1));

                                gridEquipmentIcons.Children.Add(image);
                                Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));
                                Grid.SetZIndex(image, Grid.GetZIndex(socketClicked) + 1);

                                // Make the images horizontal alignment match the alignment of the socket clicked.
                                image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                // Make it so you can still click the socket behind it.
                                image.IsHitTestVisible = false;

                                // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                socketClicked.Tag = selectedGem.name;
                                ToolTipService.SetToolTip(socketClicked, socketClicked.Tag);
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

                                // Remove previous gem icons
                                RemovePreviousGemImage(GetGridLocation(socketClicked).ElementAt(0), GetGridLocation(socketClicked).ElementAt(1));

                                gridEquipmentIcons.Children.Add(image);
                                Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));
                                Grid.SetZIndex(image, Grid.GetZIndex(socketClicked) + 1);

                                // Make the images horizontal alignment match the alignment of the socket clicked.
                                image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                // Make it so you can still click the socket behind it.
                                image.IsHitTestVisible = false;

                                // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                socketClicked.Tag = selectedGem.name;
                                ToolTipService.SetToolTip(socketClicked, socketClicked.Tag);
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

                                // Remove previous gem icons
                                RemovePreviousGemImage(GetGridLocation(socketClicked).ElementAt(0), GetGridLocation(socketClicked).ElementAt(1));

                                gridEquipmentIcons.Children.Add(image);
                                Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));
                                Grid.SetZIndex(image, Grid.GetZIndex(socketClicked) + 1);

                                // Make the images horizontal alignment match the alignment of the socket clicked.
                                image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                // Make it so you can still click the socket behind it.
                                image.IsHitTestVisible = false;

                                // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                socketClicked.Tag = selectedGem.name;
                                ToolTipService.SetToolTip(socketClicked, socketClicked.Tag);
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

                                    // Remove previous gem icons
                                    RemovePreviousGemImage(GetGridLocation(socketClicked).ElementAt(0), GetGridLocation(socketClicked).ElementAt(1));

                                    gridEquipmentIcons.Children.Add(image);
                                    Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                    Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));
                                    Grid.SetZIndex(image, Grid.GetZIndex(socketClicked) + 1);



                                    // Make the images horizontal alignment match the alignment of the socket clicked.
                                    image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                    // Make it so you can still click the socket behind it.
                                    image.IsHitTestVisible = false;

                                    // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                    socketClicked.Tag = selectedGem.name;
                                    ToolTipService.SetToolTip(socketClicked, socketClicked.Tag);
                                }
                                else
                                {
                                    socketClicked.Source = new BitmapImage(new Uri(@"/Assets/whiteSocket.png", UriKind.Relative));
                                    GetGridLocation(socketClicked).ElementAt(0);

                                    // Add another image to show the socket is filled.
                                    // TODO change this based on skill vs support gem
                                    Image image = new Image();
                                    image.Source = new BitmapImage(new Uri(@"/Assets/noneGem.png", UriKind.Relative));

                                    // Remove previous gem icons
                                    RemovePreviousGemImage(GetGridLocation(socketClicked).ElementAt(0), GetGridLocation(socketClicked).ElementAt(1));

                                    gridEquipmentIcons.Children.Add(image);
                                    Grid.SetColumn(image, GetGridLocation(socketClicked).ElementAt(0));
                                    Grid.SetRow(image, GetGridLocation(socketClicked).ElementAt(1));

                                    // Make the images horizontal alignment match the alignment of the socket clicked.
                                    image.HorizontalAlignment = socketClicked.HorizontalAlignment;

                                    // Make it so you can still click the socket behind it.
                                    image.IsHitTestVisible = false;

                                    // Set the tag for the gem to the selected gem's name so that we can highlight it later.
                                    socketClicked.Tag = selectedGem.name;
                                    ToolTipService.SetToolTip(socketClicked, socketClicked.Tag);
                                }

                                
                                break;
                            }
                    }

                    Logger.LogDebug("Selected Gem: " + selectedGem);

                }
            }
        }

        /// <summary>
        /// Gets an array of the column and row for a given object
        /// </summary>
        /// <param name="objectToFind"> The object you are trying to find in the grid </param>
        /// <returns> A 2 value array [x, y]</returns>
        private int[] GetGridLocation(object objectToFind)
        {
            UIElement uiElement = (UIElement)objectToFind;
            int[] result = new int[2];
            result[0] = Grid.GetColumn(uiElement);
            result[1] = Grid.GetRow(uiElement);

            return result;
        }

        /// <summary>
        /// Remove gem icon at given grid coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void RemovePreviousGemImage(int x, int y)
        {
            // loop through the grid of images and find the image at that location.
            foreach(object obj in gridEquipmentIcons.Children)
            {
                if(obj is Image)
                {
                    Image img = obj as Image;
                   
                    int[] imgLocation = GetGridLocation(img);
                    if (imgLocation[0] == x && imgLocation[1] == y && img.Source.ToString().Contains("Gem"))
                    {
                        // Remove the img
                        gridEquipmentIcons.Children.Remove(img);
                        // break so that we do not throw an exception for changing the collection.
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
            System.Windows.Controls.Button b = (System.Windows.Controls.Button)sender;
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

        private void HighlightGem_Clicked(Object sender, System.Windows.Input.MouseEventArgs e)
        {
            gridEquipmentIcons.Children.Remove((UIElement)sender);
            Image img = sender as Image;
            GemsAvailable.Remove(img.Tag.ToString());
            UpdateDataInUI();
        }

        private void UpdateDataInUI()
        {
            if(currentArea != null)
            {
                AreaNameTextBlock.Text = currentArea.name;
                AreaLevelTextBlock.Text = currentArea.areaLevel;
            }            
            CharacterLevelTextBlock.Text = currentLevel.ToString();
            CharacterNameTextBlock.Text = characterName + " (" + characterClass + ")";
            if(currentQuest != null)
            {
                CurrentQuestTextBlock.Text = currentQuest.questName;
                int indexOfCurrentQuest = allQuests.FindIndex(q => q.questName == currentQuest.questName);
                try
                {
                    NextQuestTextBlock.Text = allQuests.ElementAt(indexOfCurrentQuest + 1).questName;
                    NextAreaTextBlock.Text = allQuests.ElementAt(indexOfCurrentQuest + 1).finishZone;
                }
                catch
                {
                    NextQuestTextBlock.Text = "All quests done.";
                }
                
            }
            

            AddWhispersToListView();

            WhisperNameTextBlock.Text = lastWhisperName;
            bool didFindAGem = false;
            AvailableGemsTextBlock.Text = string.Empty;
            foreach (string gem in GemsAvailable)
            {
                AvailableGemsTextBlock.Text += gem + ", ";
                didFindAGem = true;
            }
            if (didFindAGem)
            {
                AvailableGemsTextBlock.Text = AvailableGemsTextBlock.Text.Substring(0, AvailableGemsTextBlock.Text.Length - 2);
            }


            try
            {
                HighestBaseTextBlock.Text = GetHighestWeapon().name;
            }
            catch (Exception e)
            {
                Logger.LogDebug(e.ToString());
            }
                

            
            
            SetXpPenalty();
        }

        private Weapon GetHighestWeapon()
        {
            Weapon nearest = new Weapon();
            if(selectedWeapon != null)
            {
                nearest = dataFetcher.WeaponList.Where(w => w.classId == selectedWeapon.classId).OrderBy(w => Math.Abs(w.dropLevel - currentLevel)).First();
            }

            
            return nearest;
        }

        private void UpdateSettingsUI()
        {
            ClientTxtFilePathTextBox.Text = settings.ClientTxtFilePath;
            HideoutHotkeyTextBox.Text = settings.HideoutHotkey;
            LogOutHotkeyTextBox.Text = settings.LogOutHotkey;
            WhisperBackHotkeyTextBox.Text = settings.LogOutHotkey;
            InviteLastPlayerHotkeyTextBox.Text = settings.InviteLastPlayerHotkey;
            InviteFriend1HotkeyTextBox.Text = settings.InviteFriend1Hotkey;
            InviteFriend2HotkeyTextBox.Text = settings.InviteFriend2Hotkey;
            InviteFriend3HotkeyTextBox.Text = settings.InviteFriend3Hotkey;
            Friend1NameTextBox.Text = settings.Friend1;
            Friend2NameTextBox.Text = settings.Friend2;
            Friend3NameTextBox.Text = settings.Friend3;


            CustomHotkeyTextBox.Text = settings.CustomHotkey;
            CustomFunctionTextBox.Text = settings.CustomFunction;
        }

        private void AddWhispersToListView()
        {
            foreach(string[] stringArray in recentWhispers)
            {
                //ListViewItem listViewItem = new ListViewItem();
                //listViewItem.Content = stringArray[0] + ": " + stringArray[1];                
                //WhisperLogView.Items.Add(listViewItem);
                listItems.Add(stringArray[0] + ": " + stringArray[1]);
            }
            recentWhispers.Clear();

        }

        private void SetZIndexDefault()
        {
            
            foreach(Object o in gridEquipmentIcons.Children)
            {
                if(o is Image)
                {
                    Image i = o as Image;
                    Grid.SetZIndex(i, 1);
                }
                
            }
        }

        private void saveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            settings.HideoutHotkey = HideoutHotkeyTextBox.Text;
            settings.LogOutHotkey = LogOutHotkeyTextBox.Text;
            settings.LogOutHotkey = WhisperBackHotkeyTextBox.Text;
            settings.InviteLastPlayerHotkey = InviteLastPlayerHotkeyTextBox.Text;
            
            settings.InviteFriend1Hotkey = InviteFriend1HotkeyTextBox.Text;
            settings.InviteFriend2Hotkey = InviteFriend2HotkeyTextBox.Text;
            settings.InviteFriend3Hotkey = InviteFriend3HotkeyTextBox.Text;

            settings.Friend1 = Friend1NameTextBox.Text;
            settings.Friend2 = Friend2NameTextBox.Text;
            settings.Friend3 = Friend3NameTextBox.Text;
            settings.CustomHotkey = CustomHotkeyTextBox.Text;
            settings.CustomFunction = CustomFunctionTextBox.Text;
            settings.Save();
            UpdateSettingsUI();
        }

        private string PromptKeyPress()
        {
            string keyPressed = string.Empty;


            return keyPressed;
        }

        private void ClientTxtFilePathTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            settings.setClientTxtFilePath();
            UpdateSettingsUI();
        }

        

        private void HotKeyTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            textBox.Text = "Enter a Key";
            inSettings = true;

            WaitForKeyPress waitForKeyPress = new WaitForKeyPress();
            if ((bool)waitForKeyPress.ShowDialog())
            {
                inSettings = false;
            }

            textBox.Text = waitForKeyPress.GetKeyPressed();
            inSettings = false;
        }

        private void manualUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            dataFetcher.ManualUpdateData();
        }
    }
}
