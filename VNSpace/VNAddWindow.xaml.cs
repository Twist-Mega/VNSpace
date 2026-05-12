using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VNSpace
{
    public partial class AddVNWindow : Window
    {
        /*stores the result of the dialog -- null if cancelled*/
        public UserEntry? Result { get; private set; }

        public AddVNWindow()
        {
            InitializeComponent();
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

            /*construct VisualNovel and UserEntry from inputs*/
            VisualNovel vn;
            if (vndbId != null)
                vn = new VisualNovel(title, company, writer, null, vndbId);
            else
                vn = new VisualNovel(title, company, writer);

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