using System.Windows;
using System.Windows.Controls;

namespace VNSpace
{
    public partial class MainWindow : Window
    {
        private VNLibrary library;

        public MainWindow()
        {
            InitializeComponent();
            library = new VNLibrary();
            library.AddVN("Higurashi no Naku Koro ni", "07th Expansion", "Ryukishi07");
            library.AddVN("Steins;Gate", "5pb.", "Hayashi Naotaka");
            library.AddVN("Clannad", "Key", "Maeda Jun");
            library.AddVN("Muv-Luv", "âge", "Yoshimune Kouki");
            vnListBox.ItemsSource = library.GetAllEntries();
        }

        private void TitleViewBtn_Click(object sender, RoutedEventArgs e)
        {
            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntries();
        }

        private void StatusViewBtn_Click(object sender, RoutedEventArgs e)
        {
            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntriesbyStatus(statusLabel.Planning);
        }

        private void CategoryViewBtn_Click(object sender, RoutedEventArgs e)
        {
            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntriesbyCategory("Mystery");
        }

        private void VnListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserEntry selected = vnListBox.SelectedItem as UserEntry;
            if (selected == null) return;

            titleLabel.Text = selected.GetVN().GetName();
            companyLabel.Text = selected.GetVN().GetCompany();
            statusText.Text = selected.GetStatusLabel();
            ratingLabel.Text = selected.GetRating().HasValue ? $"{selected.GetRating()}/10" : "Unrated";
        }

        private void AddVNBtn_Click(object sender, RoutedEventArgs e)
        {
            library.AddVN("New VN", "Unknown", "Unknown");
            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntries();
        }
    }
}