using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using PathOfLeagueStart.Classes;
using System.Configuration;

namespace PathOfLeagueStart.Data
{
    class SettingsDisplayData
    {
        public SettingsDisplayData()
        {
            // Check for a file location that is valid for the path of exile log file.
            clientTxtFilePath = Properties.Settings.Default.GGGClientFilePath;

            // if the file path is empty, doesn't exist at the location, or does not look for the correct file then prompt for input of locaiton
            if (string.IsNullOrEmpty(clientTxtFilePath) || !File.Exists(clientTxtFilePath) || !clientTxtFilePath.Contains("Client.txt"))
            {
                setClientTxtFilePath();
            }                      
        }

        private string clientTxtFilePath;
        public string ClientTxtFilePath 
        { 
            get { return clientTxtFilePath; }
        }

        public string HideoutHotkey 
        { 
            get { return Properties.Settings.Default.GoToHideoutHotkey;  }
            set { Properties.Settings.Default.GoToHideoutHotkey = value; }
        }

        public string LogOutHotkey
        {
            get { return Properties.Settings.Default.LogOutHotkey; }
            set { Properties.Settings.Default.LogOutHotkey = value; }
        }

        public string WhisperBackHotkey
        {
            get { return Properties.Settings.Default.WhisperBackHotkey; }
            set { Properties.Settings.Default.WhisperBackHotkey = value; }
        }

        public string InviteLastPlayerHotkey
        {
            get { return Properties.Settings.Default.InviteLastPlayerHotkey; }
            set { Properties.Settings.Default.InviteLastPlayerHotkey = value; }
        }

        public string InviteFriend1Hotkey
        {
            get { return Properties.Settings.Default.InviteFriend1Hotkey; }
            set { Properties.Settings.Default.InviteFriend1Hotkey = value; }
        }

        public string InviteFriend2Hotkey
        {
            get { return Properties.Settings.Default.InviteFriend2Hotkey; }
            set { Properties.Settings.Default.InviteFriend2Hotkey = value; }
        }

        public string InviteFriend3Hotkey
        {
            get { return Properties.Settings.Default.InviteFriend3Hotkey; }
            set { Properties.Settings.Default.InviteFriend3Hotkey = value; }
        }

        public string CustomHotkey
        {
            get { return Properties.Settings.Default.CustomHotkey; }
            set { Properties.Settings.Default.CustomHotkey = value; }
        }

        public string CustomFunction
        {
            get { return Properties.Settings.Default.CustomFunction; }
            set { Properties.Settings.Default.CustomFunction = value; }
        }


        public string Friend1
        {
            get { return Properties.Settings.Default.Friend1; }
            set { Properties.Settings.Default.Friend1 = value; }
        }
        public string Friend2
        {
            get { return Properties.Settings.Default.Friend2; }
            set { Properties.Settings.Default.Friend2 = value; }
        }
        public string Friend3
        {
            get { return Properties.Settings.Default.Friend3; }
            set { Properties.Settings.Default.Friend3 = value; }
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }


        /// <summary>
        /// Prompts the user to input a file location for the Client.Txt.
        /// </summary>
        public void setClientTxtFilePath()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                // Check that they entered the correct file name
                if ((bool)openFileDialog.ShowDialog() && openFileDialog.FileName.Contains("Client.txt"))
                {
                    clientTxtFilePath = openFileDialog.FileName;
                    Properties.Settings.Default.GGGClientFilePath = clientTxtFilePath;
                    Properties.Settings.Default.Save();
                    Logger.Log("Client.txt location set to " + clientTxtFilePath);
                }
                else
                {
                    throw new Exception("Invalid Log File Path: " + clientTxtFilePath);
                }
            }            
            catch (Exception e)
            {
                Logger.LogError("An error occured while attempting to choose a file location for the Client.txt file.", e);
                setClientTxtFilePath();
            }
        }

        public string GetHotKey(string keyAsString)
        {

            foreach(SettingsProperty settingsProperty in Properties.Settings.Default.Properties)
            {
                if(Properties.Settings.Default[settingsProperty.Name].ToString() == keyAsString)
                {
                    return settingsProperty.Name;
                }
                    
            }
            return string.Empty;
        }
    }
}
