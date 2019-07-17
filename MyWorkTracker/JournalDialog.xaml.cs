using MyWorkTracker.Code;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyWorkTracker
{
    /// <summary>
    /// Interaction logic for JournalDialog.xaml
    /// </summary>
    public partial class JournalDialog : Window
    {
        /// <summary>
        /// This is here (as opposed to in the model) because it's only related to the UI, not the application.
        /// </summary>
        private Border _originalBorder = new Border();
        public JournalEntry Entry = null;
        private WorkItem _workItem = null;

        public DataEntryMode DialogMode { get; }

        public bool? DontConfirmFutureDeletes
        {
            get
            {
                return DontConfirmDeletionCheckBox.IsChecked;
            }
        }

        /// <summary>
        /// Set to true if the "Ok" button is pressed; otherwise false.
        /// </summary>
        public bool WasDialogSubmitted { get; set; } = false;

        public JournalEntry JournalEntry
        {
            get
            {
                return new JournalEntry(Entry.JournalID, JournalHeaderTextBox.Text, JournalEntryTextBox.Text, Entry.CreationDateTime, Entry.ModificationDateTime);
            }
        }

        /// <summary>
        /// Return true if the form is in add mode.
        /// </summary>
        public bool IsInAddMode
        {
            get {
                if (DialogMode == DataEntryMode.ADD)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Return true if the form is in edit mode.
        /// </summary>
        public bool IsInEditMode
        {
            get {
            if (DialogMode == DataEntryMode.EDIT)
                return true;
            else
                return false;
            }
        }

        /// <summary>
        /// Return true if the form is in delete mode.
        /// </summary>
        public bool IsInDeleteMode
        {
            get {
                if (DialogMode == DataEntryMode.DELETE)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Return true if the form is NOT in delete mode.
        /// </summary>
        public bool IsNotDeleteMode
        {
            get
            {
                if (DialogMode == DataEntryMode.DELETE)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Set the helper text at the top of the form (depending on the mode the form is in)
        /// </summary>
        public string HelperText
        {
            get
            {
                string rValue = "";
                switch (DialogMode)
                {
                    case DataEntryMode.ADD:
                        rValue = "Enter a new Journal entry";
                        break;
                    case DataEntryMode.EDIT:
                        rValue = "Edit the Journal entry";
                        break;
                    case DataEntryMode.DELETE:
                        rValue = "Confirm Deletion of Journal entry?";
                        break;
                }
                return rValue;
            }
        }

        /// <summary>
        /// Get the "Ok" button image path (depending on the mode the form is in)
        /// </summary>
        public string GetApplyButtonImagePath {
            get
            {
                string rValue = @"Images\";
                switch (DialogMode)
                {
                    case DataEntryMode.ADD:
                        rValue += "journal_add.png";
                        break;
                    case DataEntryMode.EDIT:
                        rValue += "journal_edit.png";
                        break;
                    case DataEntryMode.DELETE:
                        rValue += "journal_delete.png";
                        break;
                }
                return rValue;
            }
        }

        /// <summary>
        /// Get the "Ok" button label (depending on the mode the form is in)
        /// </summary>
        public string GetApplyButtonLabel
        {
            get
            {
                string rValue = "";
                switch (DialogMode)
                {
                    case DataEntryMode.ADD:
                        rValue = "Add";
                        break;
                    case DataEntryMode.EDIT:
                        rValue = "Edit";
                        break;
                    case DataEntryMode.DELETE:
                        rValue = "Delete";
                        break;
                }
                return rValue;
            }
        }

        public JournalDialog(WorkItem workItem, JournalEntry entry, DataEntryMode mode)
        {
            _workItem = workItem;

            if (entry != null)
                Entry = entry;

            DialogMode = mode;
            InitializeComponent();
            DataContext = this;

            // Dodgy implementation; should be binding
            if ((mode == DataEntryMode.EDIT) || (mode == DataEntryMode.DELETE))
            {
                JournalHeaderTextBox.Text = entry.Title;
                JournalEntryTextBox.Text = entry.Entry;
            }
        }

        /// <summary>
        /// To highlight the selected control, add a left border.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlGainsFocus(object sender, RoutedEventArgs e)
        {
            Control c = e.Source as Control;
            _originalBorder.BorderThickness = c.BorderThickness;
            _originalBorder.BorderBrush = c.BorderBrush;

            c.BorderBrush = Brushes.DodgerBlue;
            c.BorderThickness = new Thickness(4, 0, 0, 0);
        }

        /// <summary>
        /// Reset the formerly-highlighted control by restoring it's borders.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlLosesFocus(object sender, RoutedEventArgs e)
        {
            Control c = e.Source as Control;
            c.BorderBrush = _originalBorder.BorderBrush;
            c.BorderThickness = _originalBorder.BorderThickness;
        }

        private void ApplyChange_Click(object sender, RoutedEventArgs e)
        {
            WasDialogSubmitted = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WasDialogSubmitted = false;
            this.Close();
        }

    }
}
