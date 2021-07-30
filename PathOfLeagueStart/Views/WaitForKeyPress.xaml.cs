using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PathOfLeagueStart.Views
{
    /// <summary>
    /// Interaction logic for WaitForKeyPress.xaml
    /// </summary>
    public partial class WaitForKeyPress : Window
    {

        private string pressedKey;
        public WaitForKeyPress()
        {
            InitializeComponent();
            Window window = Window.GetWindow(this);
            window.KeyDown += settingsKeyEntry_KeyDown;
        }

        public string GetKeyPressed()
        {
            return pressedKey;
        }

        private void settingsKeyEntry_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            KeysConverter keysConverter = new KeysConverter();
            pressedKey = keysConverter.ConvertToString(e.Key);
            DialogResult = true;
            
        }
    }
}
