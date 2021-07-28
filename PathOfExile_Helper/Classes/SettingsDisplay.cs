using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Microsoft.Win32;
using System.Windows;
using PathOfExile_Helper.Classes;

namespace PathOfExile_Helper.Data
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
        public string getClientTxtFilePath 
        { 
            get { return clientTxtFilePath; }
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
    }
}
