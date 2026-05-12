using System.Windows;
using System.Windows.Controls;
namespace VNSpace
{
    public partial class MainWindow : Window
    {
        private VNLibrary library;
        private UserEntry? currentEntry; // nullable fixes the non-nullable warning

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
            UserEntry? selected = vnListBox.SelectedItem as UserEntry;
            if (selected == null) return;

            currentEntry = selected;
            titleLabel.Text = selected.GetVN().GetName();
            companyWriterLabel.Text = $"{selected.GetVN().GetCompany()} · {selected.GetVN().GetWriter()}";
            statusText.Text = selected.GetStatusLabel();
            ratingLabel.Text = selected.GetRating().HasValue ? $"{selected.GetRating()}/10" : "Unrated";
            playtimeLabel.Text = selected.GetPlayTime().HasValue ? $"{selected.GetPlayTime()} hrs" : "—";
            startDateLabel.Text = selected.GetStartDate().HasValue ? selected.GetStartDate().ToString() : "—";
            finishDateLabel.Text = selected.GetFinishDate().HasValue ? selected.GetFinishDate().ToString() : "—";
            categoryLabel.Text = selected.GetCategory() ?? "—";
            vndbRatingLabel.Text = selected.GetVN().GetAverageRating().HasValue ? $"{selected.GetVN().GetAverageRating()}/100" : "—";
            notesBox.Text = selected.GetNotes() ?? "";
            vndbBadge.Visibility = selected.GetVN().IsLinkedToVNDB() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveNotesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            currentEntry.SetNotes(notesBox.Text);
        }

        private void AddVNBtn_Click(object sender, RoutedEventArgs e)
        {
            library.AddVN("New VN", "Unknown", "Unknown");
            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntries();
        }

        private void LaunchBtn_Click(object sender, RoutedEventArgs e)
        {
            // placeholder for now
        }
    }
}