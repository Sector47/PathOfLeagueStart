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
    /// Interaction logic for GemSelectionWindow.xaml
    /// </summary>
    public partial class GemSelectionWindow : Window
    {
        private List<Gem> allGemsList;
        private List<Gem> filteredGemsList;
        private Gem selectedGem;
        public GemSelectionWindow(List<Gem> allGems)
        {
            InitializeComponent();
            allGemsList = allGems;
            BindGemData();
            AddEventListeners();
        }

        public Gem GetSelectedGem()
        {
            return selectedGem;
        }

        private void AddEventListeners()
        {
            FilterGemTextBox.TextChanged += FilterGemTextBox_TextChanged;
            StrGemListBox.SelectionChanged += ListBox_SelectionChanged;
            DexGemListBox.SelectionChanged += ListBox_SelectionChanged;
            IntGemListBox.SelectionChanged += ListBox_SelectionChanged;
            NoneGemListBox.SelectionChanged += ListBox_SelectionChanged;
        }

        private void BindGemData()
        {
            StrGemListBox.ItemsSource = allGemsList.Where(g => g.primary_attribute == "strength");
            DexGemListBox.ItemsSource = allGemsList.Where(g => g.primary_attribute == "dexterity");
            IntGemListBox.ItemsSource = allGemsList.Where(g => g.primary_attribute == "intelligence");
            NoneGemListBox.ItemsSource = allGemsList.Where(g => g.primary_attribute == "none");
        }

        private void BindGemData(string filter)
        {
            // Filter our gems using indexof because this version of .Net doesn't have .Contains with the overload for stringcomparison on a string.
            // We could also toUpper or toLower each string we are comparing, but that is silly.
            filteredGemsList = allGemsList.Where(g => g.name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >=0).ToList();

            StrGemListBox.ItemsSource = filteredGemsList.Where(g => g.primary_attribute == "strength");
            DexGemListBox.ItemsSource = filteredGemsList.Where(g => g.primary_attribute == "dexterity");
            IntGemListBox.ItemsSource = filteredGemsList.Where(g => g.primary_attribute == "intelligence");
            NoneGemListBox.ItemsSource = filteredGemsList.Where(g => g.primary_attribute == "none");
        }

        private void FilterGemTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            BindGemData(textBox.Text);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedGem = (sender as ListBox).SelectedItem as Gem;
            DialogResult = true;
            
        }
    }
}
