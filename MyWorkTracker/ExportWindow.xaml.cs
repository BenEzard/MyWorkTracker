using Microsoft.Win32;
using MyWorkTracker.Code;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MyWorkTracker
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private static string ACTIVE_PLUS_CLOSED = "ACTIVE PLUS CLOSED";
        Dictionary<PreferenceName, string> _backupPreferences = null;

        private bool _isSubmitted = false;
        /// <summary>
        /// Checks to see if the dialog box was submitted.
        /// The alternative is cancelled.
        /// </summary>
        public bool WasSubmitted
        {
            get
            {
                return _isSubmitted;
            }
        }

        /// <summary>
        /// Returns if the export is from the System file.
        /// </summary>
        public bool ExportFromSystemFile
        {
            get
            {
                if (SystemFile.IsChecked == true)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Returns the Export file.
        /// </summary>
        public string ExportFile
        {
            get
            {
                return ExportFileTextBox.Text;
            }
        }

        /// <summary>
        /// Returns the stale number; how many days worth of Closed tasks should be exported.
        /// </summary>
        public int StaleNumber
        {
            get
            {
                int rValue = -1;

                if (WorkItemType.Equals(ACTIVE_PLUS_CLOSED))
                    rValue = Int32.Parse(WorkItemClosedDaysTextBox.Text);
                else
                    rValue = Int32.Parse(GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DAYS_STALE_DEFAULT));

                return rValue;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        public string WorkItemType
        {
            get
            {
                var cbItem = (ComboBoxItem)(WorkItemSelectionComboBox.SelectedValue);
                string value = (string)cbItem.Content;
                return value.ToUpper();
            }
        }

        /// <summary>
        /// Returns a boolean value if all due date information should be exported.
        /// </summary>
        public bool AllDueDates
        {
            get
            {
                var cbItem = (ComboBoxItem)(DueDateComboBox.SelectedValue);
                string value = (string)cbItem.Content;
                if (value.Equals("full"))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Returns a boolean value if all status information should be exported.
        /// </summary>
        public bool AllStatuses
        {
            get
            {
                var cbItem = (ComboBoxItem)(StatusComboBox.SelectedValue);
                string value = (string)cbItem.Content;
                if (value.Equals("full"))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Returns a boolean value if deleted items should be included.
        /// </summary>
        public bool IncludeDeleted
        {
            get
            {
                if (BackupIncludeDeletedCheckBox.IsChecked == true)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Returns a boolean value if Preferences should be included.
        /// </summary>
        public bool IncludePreferences
        {
            get
            {
                if (BackupIncludeSettingsCheckBox.IsChecked == true)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Get the SaveLocation from the dialog
        /// </summary>
        public string SaveLocation
        {
            get
            {
                return BackupSaveToTextBox.Text;
            }
        }

        /// <summary>
        /// Get the selected ExportVersion from the dialog
        /// </summary>
        public string ExportVersion
        {
            get
            {
                return (string)ExportVersionComboBox.SelectedValue;
            }
        }

        public ExportWindow(Dictionary<PreferenceName, string> backupPreferences)
        {
            _backupPreferences = backupPreferences;
            InitializeComponent();
            PopulateDialog();
        }

        /// <summary>
        /// Get the dialog ready by populating it.
        /// </summary>
        private void PopulateDialog()
        {
            // --- Backup options -----------------------------------------------------------------------------------------------------
            string workItemSelection = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_WORKITEM_SELECTION);
            SelectComboItem(WorkItemSelectionComboBox, workItemSelection);

            if (workItemSelection.ToUpper().Equals(ACTIVE_PLUS_CLOSED))
            {
                WorkItemClosedDaysTextBox.Text = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DAYS_STALE);
            }

            SelectComboItem(DueDateComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DUEDATE_SELECTION));
            SelectComboItem(StatusComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_STATUS_SELECTION));
            
            string[] exportVersionStr = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_AVAILABLE_VERSIONS).Split(',');
            for (int i = 0; i < exportVersionStr.Length; i++)
            {
                ExportVersionComboBox.Items.Add(exportVersionStr[i]);
            }
            SelectStringComboItem(ExportVersionComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DEFAULT_VERSION));

            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_INCLUDE_DELETED) == "1")
                BackupIncludeDeletedCheckBox.IsChecked = true;

            BackupSaveToTextBox.Text = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_SAVE_TO_LOCATION);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="combo"></param>
        /// <param name="valueToSelect"></param>
        private void SelectStringComboItem(ComboBox combo, string valueToSelect)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                string value = Convert.ToString(combo.Items[i]);
                if (value.Equals(valueToSelect))
                {
                    combo.SelectedIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Select the specified value in the combo.
        /// </summary>
        /// <param name="combo"></param>
        /// <param name="valueToSelect"></param>
        private void SelectComboItem(ComboBox combo, string valueToSelect)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                string value = Convert.ToString(((ComboBoxItem)combo.Items[i]).Content.ToString());
                if (value.Equals(valueToSelect))
                {
                    combo.SelectedIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Convenience short-cut method for accessing the value of a Setting (based on the original value passed in).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetOriginalSettingValue(PreferenceName name)
        {
            _backupPreferences.TryGetValue(name, out string sett);
            return sett;
        }

        /// <summary>
        /// The event fired when the WorkItem Combobox is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkItemSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbItem = (ComboBoxItem)(WorkItemSelectionComboBox.SelectedValue);
            string value = (string)cbItem.Content;

            if (value.ToUpper().Equals(ACTIVE_PLUS_CLOSED))
            {
                WorkItemClosedDaysTextBox.IsEnabled = true;
                WorkItemClosedDaysTextBox.Text = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DAYS_STALE);
            }
            else
            {
                WorkItemClosedDaysTextBox.IsEnabled = false;
                WorkItemClosedDaysTextBox.Text = "";
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            _isSubmitted = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isSubmitted = false;
            this.Close();
        }

        private void ExportFileTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            AnotherFile.IsChecked = true;
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "db|*.db";
            _backupPreferences.TryGetValue(PreferenceName.DATA_EXPORT_LAST_DIRECTORY, out string initDirectory);
            openFileDialog.InitialDirectory = initDirectory;
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                AnotherFile.IsChecked = true;
                ExportFileTextBox.Text = openFileDialog.FileName;
            }
        }
    }
}
