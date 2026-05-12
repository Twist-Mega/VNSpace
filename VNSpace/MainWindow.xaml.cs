/*Michael Randazzo*/
/*Interaction logic for MainWindow.xaml*/

using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace VNSpace
{
    public partial class MainWindow : Window
    {
        /*library holds all VN entries in memory*/
        private VNLibrary library;

        /*database manager handles all read/write operations*/
        private SQLManager db;

        /*tracks the currently selected entry in the list*/
        private UserEntry? currentEntry;

        public MainWindow()
        {
            InitializeComponent();

            db = new SQLManager();
            library = new VNLibrary();

            /*load all saved entries from database into library on startup*/
            foreach (UserEntry entry in db.LoadAllEntries())
            {
                library.GetAllEntries().Add(entry);
            }

            vnListBox.ItemsSource = library.GetAllEntries();
        }

        /*title view button -- shows all VN entries sorted by title*/
        private void TitleViewBtn_Click(object sender, RoutedEventArgs e)
        {
            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntries();
        }

        /*status view button -- shows entries filtered by Planning status*/
        private void StatusViewBtn_Click(object sender, RoutedEventArgs e)
        {
            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntriesbyStatus(statusLabel.Planning);
        }

        /*category view button -- shows entries filtered by category*/
        private void CategoryViewBtn_Click(object sender, RoutedEventArgs e)
        {
            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntriesbyCategory("Mystery");
        }

        /*selection changed -- populates right panel with selected entry's data*/
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

        /*save notes button -- persists notes to database*/
        private void SaveNotesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;

            /*update notes in memory*/
            currentEntry.SetNotes(notesBox.Text);

            /*persist updated entry to database*/
            db.UpdateEntry(currentEntry, currentEntry.GetEntryId());
        }

        /*add VN button -- opens dialog and saves new entry if confirmed*/
        private void AddVNBtn_Click(object sender, RoutedEventArgs e)
        {
            /*open add VN dialog as modal window*/
            AddVNWindow dialog = new AddVNWindow();
            dialog.Owner = this;
            bool? result = dialog.ShowDialog();

            /*only add entry if user confirmed*/
            if (result == true && dialog.Result != null)
            {
                /*add to library in memory*/
                library.GetAllEntries().Add(dialog.Result);

                /*save to database and store returned Entry_Id*/
                long newId = db.SaveEntry(dialog.Result);
                dialog.Result.SetEntryId(newId);

                /*refresh listbox*/
                vnListBox.ItemsSource = null;
                vnListBox.ItemsSource = library.GetAllEntries();
            }
        }

        /*launch button -- placeholder until executable path feature is implemented*/
        private void LaunchBtn_Click(object sender, RoutedEventArgs e)
        {
            /*placeholder for now*/
        }
    }
}