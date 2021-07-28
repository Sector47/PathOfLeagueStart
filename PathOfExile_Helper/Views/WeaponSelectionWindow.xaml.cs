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
using System.Windows.Shapes;

namespace PathOfLeagueStart.Views
{
    /// <summary>
    /// Interaction logic for WeaponSelectionWindow.xaml
    /// </summary>
    public partial class WeaponSelectionWindow : Window
    {
        private List<Weapon> allWeapons;
        private Weapon selectedWeapon;
        public WeaponSelectionWindow(List<Weapon> allWeapons)
        {
            InitializeComponent();
            this.allWeapons = allWeapons;
            BindWeaponData();
            AddEventListeners();
        }

        public Weapon GetSelectedWeapon()
        {
            return selectedWeapon;
        }

        private void AddEventListeners()
        {
            FilterWeaponTextBox.TextChanged += FilterWeaponTextBox_TextChanged;
            WeaponListBox.SelectionChanged += ListBox_SelectionChanged;
        }

        private void BindWeaponData()
        {
            WeaponListBox.ItemsSource = allWeapons;
        }

        private void BindWeaponData(string filter)
        {
            WeaponListBox.ItemsSource = allWeapons.Where(w => w.name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        private void FilterWeaponTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            BindWeaponData(textBox.Text);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedWeapon = (sender as ListBox).SelectedItem as Weapon;
            DialogResult = true;

        }
    }
}
