/*Michael Randazzo*/
/*Interaction logic for MainWindow.xaml*/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        /*tracks which view mode the left panel is in*/
        /*possible values: "title", "status", "category"*/
        private string currentView = "title";

        /*tracks whether the left panel is showing groups or drilled-down entries*/
        private bool isDrilledDown = false;

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
            library.AddVN("Steins;Gate", "5pb.", "Hayashi Naotaka");
            library.GetAllEntries().Last().SetStatus(statusLabel.Playing);
            library.GetAllEntries().Last().SetCategory("Sci-Fi");

            library.AddVN("Clannad", "Key", "Maeda Jun");
            library.GetAllEntries().Last().SetStatus(statusLabel.Finished);
            library.GetAllEntries().Last().SetCategory("Romance");

            library.AddVN("Muv-Luv", "âge", "Yoshimune Kouki");
            library.GetAllEntries().Last().SetStatus(statusLabel.Planning);
            library.GetAllEntries().Last().SetCategory("Sci-Fi");


            ShowTitleView();


        }

        /*shows flat list of all VNs sorted by title*/
        private void ShowTitleView()
        {
            currentView = "title";
            isDrilledDown = false;
            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntries();
        }

        /*shows all status groups in the listbox*/
        private void ShowStatusGroups()
        {
            currentView = "status";
            isDrilledDown = false;

            /*build list of status group labels that have at least one entry*/
            List<string> groups = new List<string>();
            foreach (statusLabel status in System.Enum.GetValues(typeof(statusLabel)))
            {
                List<UserEntry> matching = library.GetAllEntriesbyStatus(status);
                if (matching.Count > 0)
                    groups.Add($"{status} ({matching.Count})");
            }

            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = groups;
        }

        /*shows all unique categories in the listbox*/
        private void ShowCategoryGroups()
        {
            currentView = "category";
            isDrilledDown = false;

            /*get all unique non-empty categories from library*/
            List<string> groups = library.GetAllEntries()
                .Where(e => !string.IsNullOrEmpty(e.GetCategory()))
                .Select(e => e.GetCategory()!)
                .Distinct()
                .ToList();

            /*add uncategorized group if any entries have no category*/
            int uncategorized = library.GetAllEntries()
                .Count(e => string.IsNullOrEmpty(e.GetCategory()));
            if (uncategorized > 0)
                groups.Add($"Uncategorized ({uncategorized})");

            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = groups;
        }

        /*drills down into a status group -- shows all VNs with that status*/
        private void DrillDownStatus(string groupLabel)
        {
            isDrilledDown = true;

            /*extract the status name from the group label e.g. "Playing (3)" -> "Playing"*/
            string statusName = groupLabel.Split(' ')[0];

            if (System.Enum.TryParse(statusName, out statusLabel status))
            {
                vnListBox.ItemsSource = null;
                vnListBox.ItemsSource = library.GetAllEntriesbyStatus(status);
            }
        }

        /*drills down into a category group -- shows all VNs in that category*/
        private void DrillDownCategory(string groupLabel)
        {
            isDrilledDown = true;

            /*handle uncategorized group separately*/
            if (groupLabel.StartsWith("Uncategorized"))
            {
                vnListBox.ItemsSource = null;
                vnListBox.ItemsSource = library.GetAllEntries()
                    .Where(e => string.IsNullOrEmpty(e.GetCategory()))
                    .ToList();
                return;
            }

            vnListBox.ItemsSource = null;
            vnListBox.ItemsSource = library.GetAllEntriesbyCategory(groupLabel);
        }

        /*title view button -- shows flat list of all VNs*/
        private void TitleViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowTitleView();
        }

        /*status view button -- shows status groups or returns to groups if drilled down*/
        private void StatusViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowStatusGroups();
        }

        /*category view button -- shows category groups or returns to groups if drilled down*/
        private void CategoryViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowCategoryGroups();
        }

        /*selection changed -- handles both group selection and VN selection*/
        private void VnListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (vnListBox.SelectedItem == null) return;

            /*check if selected item is a group label string or a UserEntry*/
            if (vnListBox.SelectedItem is string groupLabel)
            {
                /*user clicked a group -- drill down into it*/
                if (currentView == "status")
                    DrillDownStatus(groupLabel);
                else if (currentView == "category")
                    DrillDownCategory(groupLabel);
                return;
            }

            /*user clicked a VN entry -- populate right panel*/
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
            voiceActorsLabel.Text = currentEntry.GetVN().GetVoiceActors() ?? "—";

            /*show launch button only if executable path is set*/
            execPathLabel.Text = currentEntry.GetVN().GetExecutablePath() ?? "—";
            launchBtn.Visibility = string.IsNullOrEmpty(currentEntry.GetVN().GetExecutablePath())
                                   ? Visibility.Collapsed
                                   : Visibility.Visible;
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

                /*refresh current view*/
                if (currentView == "title")
                    ShowTitleView();
                else if (currentView == "status")
                    ShowStatusGroups();
                else if (currentView == "category")
                    ShowCategoryGroups();
            }
        }

        /*helper -- swaps a TextBlock to edit mode and shows confirm button*/
        private void EnterEditMode(TextBlock label, TextBox input, Button editBtn, Button confirmBtn, string currentValue)
        {
            input.Text = currentValue == "—" ? "" : currentValue;
            label.Visibility = Visibility.Collapsed;
            input.Visibility = Visibility.Visible;
            editBtn.Visibility = Visibility.Collapsed;
            confirmBtn.Visibility = Visibility.Visible;
            input.Focus();
        }

        /*helper -- swaps back to display mode*/
        private void ExitEditMode(TextBlock label, TextBox input, Button editBtn, Button confirmBtn, string newValue)
        {
            label.Text = newValue;
            label.Visibility = Visibility.Visible;
            input.Visibility = Visibility.Collapsed;
            editBtn.Visibility = Visibility.Visible;
            confirmBtn.Visibility = Visibility.Collapsed;
        }

        /*playtime edit and confirm*/
        private void PlaytimeEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            string current = currentEntry.GetPlayTime().HasValue ? $"{currentEntry.GetPlayTime()}" : "—";
            EnterEditMode(playtimeLabel, playtimeInput, playtimeEditBtn, playtimeConfirmBtn, current);
        }

        private void PlaytimeConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            if (float.TryParse(playtimeInput.Text, out float playTime))
            {
                currentEntry.SetPlayTime(playTime);
                db.UpdateEntry(currentEntry, currentEntry.GetEntryId());
                ExitEditMode(playtimeLabel, playtimeInput, playtimeEditBtn, playtimeConfirmBtn, $"{playTime} hrs");
            }
            else
            {
                ExitEditMode(playtimeLabel, playtimeInput, playtimeEditBtn, playtimeConfirmBtn, playtimeLabel.Text);
            }
        }

        /*rating edit and confirm*/
        private void RatingEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            string current = currentEntry.GetRating().HasValue ? $"{currentEntry.GetRating()}" : "—";
            EnterEditMode(ratingLabel, ratingInput, ratingEditBtn, ratingConfirmBtn, current);
        }

        private void RatingConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            if (float.TryParse(ratingInput.Text, out float rating) && rating >= 0 && rating <= 10)
            {
                currentEntry.SetRating(rating);
                db.UpdateEntry(currentEntry, currentEntry.GetEntryId());
                ExitEditMode(ratingLabel, ratingInput, ratingEditBtn, ratingConfirmBtn, $"{rating}/10");
            }
            else
            {
                ExitEditMode(ratingLabel, ratingInput, ratingEditBtn, ratingConfirmBtn, ratingLabel.Text);
            }
        }

        /*start date edit and confirm*/
        private void StartDateEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            string current = currentEntry.GetStartDate().HasValue ? currentEntry.GetStartDate()!.Value.ToShortDateString() : "";
            EnterEditMode(startDateLabel, startDateInput, startDateEditBtn, startDateConfirmBtn, current);
        }

        private void StartDateConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            if (DateTime.TryParse(startDateInput.Text, out DateTime date))
            {
                currentEntry.SetStartDate(date);
                db.UpdateEntry(currentEntry, currentEntry.GetEntryId());
                ExitEditMode(startDateLabel, startDateInput, startDateEditBtn, startDateConfirmBtn, date.ToShortDateString());
            }
            else
            {
                ExitEditMode(startDateLabel, startDateInput, startDateEditBtn, startDateConfirmBtn, startDateLabel.Text);
            }
        }

        /*finish date edit and confirm*/
        private void FinishDateEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            string current = currentEntry.GetFinishDate().HasValue ? currentEntry.GetFinishDate()!.Value.ToShortDateString() : "";
            EnterEditMode(finishDateLabel, finishDateInput, finishDateEditBtn, finishDateConfirmBtn, current);
        }

        private void FinishDateConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            if (DateTime.TryParse(finishDateInput.Text, out DateTime date))
            {
                currentEntry.SetFinishDate(date);
                db.UpdateEntry(currentEntry, currentEntry.GetEntryId());
                ExitEditMode(finishDateLabel, finishDateInput, finishDateEditBtn, finishDateConfirmBtn, date.ToShortDateString());
            }
            else
            {
                ExitEditMode(finishDateLabel, finishDateInput, finishDateEditBtn, finishDateConfirmBtn, finishDateLabel.Text);
            }
        }

        /*category edit and confirm*/
        private void CategoryEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            string current = currentEntry.GetCategory() ?? "";
            EnterEditMode(categoryLabel, categoryInput, categoryEditBtn, categoryConfirmBtn, current);
        }

        private void CategoryConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            currentEntry.SetCategory(categoryInput.Text.Trim());
            db.UpdateEntry(currentEntry, currentEntry.GetEntryId());
            ExitEditMode(categoryLabel, categoryInput, categoryEditBtn, categoryConfirmBtn, categoryInput.Text.Trim());
        }

        /*status edit and confirm -- uses ComboBox instead of TextBox*/
        private void StatusEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;

            /*populate ComboBox with all status options*/
            statusInput.ItemsSource = System.Enum.GetValues(typeof(statusLabel));
            statusInput.SelectedItem = currentEntry.GetStatus();

            statusText.Visibility = Visibility.Collapsed;
            statusInput.Visibility = Visibility.Visible;
            statusEditBtn.Visibility = Visibility.Collapsed;
            statusConfirmBtn.Visibility = Visibility.Visible;
        }

        private void StatusConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            if (statusInput.SelectedItem is statusLabel selected)
            {
                currentEntry.SetStatus(selected);
                db.UpdateEntry(currentEntry, currentEntry.GetEntryId());
            }

            statusText.Text = currentEntry.GetStatusLabel();
            statusText.Visibility = Visibility.Visible;
            statusInput.Visibility = Visibility.Collapsed;
            statusEditBtn.Visibility = Visibility.Visible;
            statusConfirmBtn.Visibility = Visibility.Collapsed;
        }

        /*reset button -- clears all data from database and library*/
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            /*confirm before wiping everything*/
            MessageBoxResult confirm = MessageBox.Show(
                "Are you sure you want to delete all entries? This cannot be undone.",
                "Reset Data",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirm == MessageBoxResult.Yes)
            {
                db.ResetAllData();
                library.GetAllEntries().Clear();
                vnListBox.ItemsSource = null;
                currentEntry = null;
            }
        }

        /*executable path edit and confirm*/
        private void ExecPathEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            string current = currentEntry.GetVN().GetExecutablePath() ?? "";
            EnterEditMode(execPathLabel, execPathInput, execPathEditBtn, execPathConfirmBtn, current);
        }

        private void ExecPathConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            currentEntry.GetVN().SetExecutablePath(execPathInput.Text.Trim());
            db.UpdateEntry(currentEntry, currentEntry.GetEntryId());

            /*show launch button if path is set, hide if cleared*/
            if (!string.IsNullOrEmpty(execPathInput.Text.Trim()))
            {
                launchBtn.Visibility = Visibility.Visible;
                ExitEditMode(execPathLabel, execPathInput, execPathEditBtn, execPathConfirmBtn, execPathInput.Text.Trim());
            }
            else
            {
                launchBtn.Visibility = Visibility.Collapsed;
                ExitEditMode(execPathLabel, execPathInput, execPathEditBtn, execPathConfirmBtn, "—");
            }
        }

        /*launch button -- launches VN executable*/
        private void LaunchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            string? path = currentEntry.GetVN().GetExecutablePath();
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not launch VN: {ex.Message}", "Launch Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*VNDB service -- handles all API communication*/
        private VNDBI_Api_Manager vndb = new VNDBI_Api_Manager();

        /*fetch VNDB data button -- pulls metadata for currently selected entry*/
        private async void FetchVNDBBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;
            if (!currentEntry.GetVN().IsLinkedToVNDB())
            {
                MessageBox.Show("This entry has no VNDB ID linked.", "No VNDB Link");
                return;
            }

            /*check API is reachable first*/
            bool reachable = await vndb.IsApiReachable();
            if (!reachable)
            {
                MessageBox.Show("VNDB API is currently unreachable. Try again later.", "API Unavailable");
                return;
            }

            /*fetch and populate VN data*/
            bool success = await vndb.FetchById(currentEntry.GetVN().GetVNDBID()!, currentEntry.GetVN());
            if (!success)
            {
                MessageBox.Show("Could not fetch data from VNDB.", "Fetch Failed");
                return;
            }

            /*load cover art if URL was set*/
            if (!string.IsNullOrEmpty(currentEntry.GetVN().GetCoverArtURL()))
            {
                BitmapImage? art = await vndb.LoadCoverArt(currentEntry.GetVN().GetCoverArtURL()!);
                if (art != null)
                    coverArtImage.Source = art;
            }

            /*update right panel display*/
            companyWriterLabel.Text = $"{currentEntry.GetVN().GetCompany()} · {currentEntry.GetVN().GetWriter()}";
            vndbRatingLabel.Text = currentEntry.GetVN().GetAverageRating().HasValue
                                      ? $"{currentEntry.GetVN().GetAverageRating()}/100"
                                      : "—";
            voiceActorsLabel.Text = currentEntry.GetVN().GetVoiceActors() ?? "—";
            vndbBadge.Visibility = Visibility.Visible;

            /*save updated VN data to database*/
            db.UpdateEntry(currentEntry, currentEntry.GetEntryId());

            MessageBox.Show("VNDB data fetched successfully!", "Success");
        }
    }
}