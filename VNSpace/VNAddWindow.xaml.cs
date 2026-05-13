/*Michael Randazzo*/
/*Interaction logic for VNAddWindow.xaml*/

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace VNSpace
{
    public partial class AddVNWindow : Window
    {
        /*stores the result of the dialog -- null if cancelled*/
        internal UserEntry? Result { get; private set; }

        /*VNDB service -- handles search calls*/
        private VNDBI_Api_Manager vndb = new VNDBI_Api_Manager();

        /*holds the current list of search results -- indexed alongside searchResultsList*/
        private List<VNDBResult> currentSearchResults = new List<VNDBResult>();

        public AddVNWindow()
        {
            InitializeComponent();
        }

        /*search VNDB button -- searches by whatever is in the title field*/
        private async void SearchVNDBBtn_Click(object sender, RoutedEventArgs e)
        {
            string query = titleInput.Text.Trim();

            /*require at least something to search*/
            if (string.IsNullOrEmpty(query))
            {
                titleInput.BorderBrush = System.Windows.Media.Brushes.Red;
                return;
            }

            /*show loading state and hide any previous results*/
            searchVNDBBtn.IsEnabled = false;
            searchResultsList.Visibility = Visibility.Collapsed;
            searchStatusLabel.Text = "Searching VNDB...";
            searchStatusLabel.Visibility = Visibility.Visible;

            /*check API is reachable before searching*/
            bool reachable = await vndb.IsApiReachable();
            if (!reachable)
            {
                searchStatusLabel.Text = "VNDB is unreachable. Check your connection and try again.";
                searchVNDBBtn.IsEnabled = true;
                return;
            }

            /*run the search*/
            List<VNDBResult> results = await vndb.SearchByTitle(query);

            searchVNDBBtn.IsEnabled = true;

            /*handle no results*/
            if (results.Count == 0)
            {
                searchStatusLabel.Text = "No results found.";
                return;
            }

            /*populate results list -- store results so SelectionChanged can access them*/
            currentSearchResults = results;
            searchResultsList.ItemsSource = null;
            searchResultsList.ItemsSource = currentSearchResults;

            searchStatusLabel.Text = $"{results.Count} results — click one to fill in the details";
            searchResultsList.Visibility = Visibility.Visible;
        }

        /*selection changed -- auto-fills fields when user picks a search result*/
        private void SearchResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (searchResultsList.SelectedIndex < 0) return;

            VNDBResult selected = currentSearchResults[searchResultsList.SelectedIndex];

            /*fill title -- prefer alttitle (original script) if available, otherwise use main title*/
            titleInput.Text = selected.Title;

            /*fill company if the result has a developer*/
            if (!string.IsNullOrEmpty(selected.Developer))
                companyInput.Text = selected.Developer;

            /*fill VNDB ID*/
            vndbIdInput.Text = selected.VndbId;

            /*update status label to confirm selection*/
            searchStatusLabel.Text = $"Selected: {selected.Title} ({selected.VndbId})";
        }



        /*browse button -- opens file explorer to select a local image cover*/
        private void BrowseCoverBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            /*filter for standard image formats*/
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                /*set the path text box to the selected file*/
                coverPathInput.Text = openFileDialog.FileName;
            }
        }

        /*confirm button -- validates input and creates new UserEntry*/
        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            /*title is required -- warn user if empty*/
            if (string.IsNullOrWhiteSpace(titleInput.Text))
            {
                titleInput.BorderBrush = System.Windows.Media.Brushes.Red;
                return;
            }

            /*read inputs -- treat empty optional fields as null*/
            string title = titleInput.Text.Trim();
            string company = string.IsNullOrWhiteSpace(companyInput.Text) ? "Unknown" : companyInput.Text.Trim();
            string writer = string.IsNullOrWhiteSpace(writerInput.Text) ? "Unknown" : writerInput.Text.Trim();
            string? vndbId = string.IsNullOrWhiteSpace(vndbIdInput.Text) ? null : vndbIdInput.Text.Trim();

            /*get custom cover path if selected*/
            string? coverPath = string.IsNullOrWhiteSpace(coverPathInput.Text) ? null : coverPathInput.Text.Trim();

            /*construct VisualNovel and UserEntry from inputs*/
            VisualNovel vn;
            if (vndbId != null)
                vn = new VisualNovel(title, company, writer, coverPath, vndbId);
            else
                vn = new VisualNovel(title, company, writer, coverPath);

            Result = new UserEntry(vn, statusLabel.Planning);

            /*close dialog with success*/
            this.DialogResult = true;
            this.Close();
        }

        /*cancel button -- closes dialog without creating entry*/
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}