using MyWorkTracker.Code;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MyWorkTracker
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class PreferenceDialog : Window
    {
        /// <summary>
        /// The settings as they were originally passed into the dialog.
        /// </summary>
        private Dictionary<PreferenceName, Preference> _originalPreferences = null;

        /// <summary>
        /// A collection of any changes; the setting name and the new value as a String.
        /// </summary>
        private Dictionary<PreferenceName, string> _settingChanges = new Dictionary<PreferenceName, string>();
        public Dictionary<PreferenceName, string> GetChanges
        {
            get
            {
                return _settingChanges;
            }
        }

        /// <summary>
        /// Indicates whether or not the dialog was submitted or cancelled.
        /// Note that 'Reset to Defaults' is considered submitted.
        /// </summary>
        public bool WasDialogSubmitted { get; set; } = false;

        /// <summary>
        /// Indicates whether or not 'Reset to Defaults' was selected.
        /// </summary>
        public bool WasApplyDefaultsSelected { get; set; } = false;

        /// <summary>
        /// Indicates whether or not the 'save window position and size' was selected.
        /// </summary>
        public bool WasSaveWindowCoordinatesSelected
        {
            get
            {
                var swc = SaveWindowCoordsCheckBox.IsChecked;
                if (swc.HasValue && swc.Value == true)
                    return true;
                else
                    return false;
            }
        }


        public PreferenceDialog(Dictionary<PreferenceName, Preference> originalPreferences)
        {
            InitializeComponent();
            _originalPreferences = originalPreferences;

            foreach (PreferenceName n in _originalPreferences.Keys)
            {
                Preference s = _originalPreferences[n];
                Console.WriteLine($"Preference {s.Name} = {_originalPreferences[n].Value}");
            }

            PopulateDialog();
        }

        /// <summary>
        /// This method populates the dialog with the current values of the Settings.
        /// (It's a bit of a hack)
        /// </summary>
        private void PopulateDialog()
        {
            if (GetOriginalSettingValue(PreferenceName.SAVE_WINDOW_COORDS_ON_EXIT) == "1")
                SaveWindowCoordsOnExitCheckBox.IsChecked = true;

            DaysToCompleteSlider.Value = double.Parse(GetOriginalSettingValue(PreferenceName.DEFAULT_WORKITEM_LENGTH_DAYS));

            SelectComboItem(HourCombo, GetOriginalSettingValue(PreferenceName.DEFAULT_WORKITEM_COB_HOURS));

            string cobMins = GetOriginalSettingValue(PreferenceName.DEFAULT_WORKITEM_COB_MINS);
            SelectComboItem(MinuteCombo, ((cobMins == "0") ? "00" : cobMins));

            if (GetOriginalSettingValue(PreferenceName.DUE_DATE_CAN_BE_WEEKENDS) == "1")
                DueDateOnWeekendsCheckBox.IsChecked = true;

            DueDateGracePeriodTextBox.Text = GetOriginalSettingValue(PreferenceName.DUE_DATE_SET_WINDOW_MINUTES);
            GracePeriodSlider.Value = double.Parse(GetOriginalSettingValue(PreferenceName.DUE_DATE_SET_WINDOW_MINUTES));

            if (GetOriginalSettingValue(PreferenceName.CONFIRM_JOURNAL_DELETION) == "1")
                ConfirmJournalCheckBox.IsChecked = true;

            LoadStaleDaysTextBox.Text = GetOriginalSettingValue(PreferenceName.LOAD_STALE_DAYS);

            ClosedToActiveAmountSlider.Value = double.Parse(GetOriginalSettingValue(PreferenceName.STATUS_ACTIVE_TO_COMPLETE_PCN));

            SelectComboItem(JournalSortingComboxBox, GetOriginalSettingValue(PreferenceName.JOURNAL_ORDERING));

            // --- Backup options -----------------------------------------------------------------------------------------------------
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_AUTOMATICALLY) == "1")
                AutomaticBackupCheckBox.IsChecked = true;

            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_SAME_DAY_OVERWRITE) == "1")
                OverwriteBackupCheckBox.IsChecked = true;

            SelectComboItem(BackupDaysComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_PERIOD_DAYS));

            SelectComboItem(WorkItemSelectionComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_WORKITEM_SELECTION));
            WorkItemClosedDaysTextBox.Text = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DAYS_STALE);
            SelectComboItem(DueDateComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DUEDATE_SELECTION));
            SelectComboItem(StatusComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_STATUS_SELECTION));

            SelectComboItem(DeleteOptionComboBox, GetOriginalSettingValue(PreferenceName.DELETE_OPTION));

            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_INCLUDE_DELETED) == "1")
                BackupIncludeDeletedCheckBox.IsChecked = true;

            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_INCLUDE_PREFERENCES) == "1")
                BackupIncludePreferencesCheckBox.IsChecked = true;

            string[] exportVersionStr = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_AVAILABLE_VERSIONS).Split(',');
            for (int i = 0; i < exportVersionStr.Length; i++)
            {
                ExportVersionComboBox.Items.Add(exportVersionStr[i]);
            }
            SelectStringComboItem(ExportVersionComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DEFAULT_VERSION));

            BackupSaveToTextBox.Text = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_SAVE_TO_LOCATION);
            BackupCopyToTextBox.Text = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_COPY_LOCATION);
        }

        /// <summary>
        /// Convenience method for accessing the value of a Setting (based on the original value passed in).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetOriginalSettingValue(PreferenceName name)
        {
            _originalPreferences.TryGetValue(name, out Preference sett);
            return sett.Value;
        }

        /// <summary>
        /// Convenience method for accessing the value of a Setting (based on the original value passed in), returned as a boolean value.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool GetOriginalSettingAsBooleanValue(PreferenceName name)
        {
            bool rValue = false;

            _originalPreferences.TryGetValue(name, out Preference sett);
            if (sett.Value.Equals("1"))
                rValue = true;

            return rValue;
        }

        /// <summary>
        /// Method compares the original settings and the current values, and populates a collection of changes.
        /// (It's a bit of a hack)
        /// </summary>
        private void GenerateDifferencesToCurrentValues()
        {
            string value = WorkDaysToCompleteTextBox.Text;
            if (GetOriginalSettingValue(PreferenceName.DEFAULT_WORKITEM_LENGTH_DAYS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DEFAULT_WORKITEM_LENGTH_DAYS, value);

            var cbItem = (ComboBoxItem)(HourCombo.SelectedValue);
            value = (string)cbItem.Content;
            if (GetOriginalSettingValue(PreferenceName.DEFAULT_WORKITEM_COB_HOURS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DEFAULT_WORKITEM_COB_HOURS, value);

            cbItem = (ComboBoxItem)(MinuteCombo.SelectedValue);
            value = (string)cbItem.Content;
            // A little bit of extra code used, so that 0 and 00 don't detect as a change.
            string originalValue = (GetOriginalSettingValue(PreferenceName.DEFAULT_WORKITEM_COB_MINS));
            originalValue = originalValue.Equals("0") ? "00" : originalValue;
            if (originalValue.Equals(value, StringComparison.OrdinalIgnoreCase) == false) 
                _settingChanges.Add(PreferenceName.DEFAULT_WORKITEM_COB_MINS, value);

            bool? val = DueDateOnWeekendsCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(PreferenceName.DUE_DATE_CAN_BE_WEEKENDS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DUE_DATE_CAN_BE_WEEKENDS, value);

            value = DueDateGracePeriodTextBox.Text;
            if (GetOriginalSettingValue(PreferenceName.DUE_DATE_SET_WINDOW_MINUTES).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DUE_DATE_SET_WINDOW_MINUTES, value);

            val = ConfirmJournalCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(PreferenceName.CONFIRM_JOURNAL_DELETION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.CONFIRM_JOURNAL_DELETION, value);

            val = SaveWindowCoordsOnExitCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(PreferenceName.SAVE_WINDOW_COORDS_ON_EXIT).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.SAVE_WINDOW_COORDS_ON_EXIT, value);

            value = LoadStaleDaysTextBox.Text;
            var isNumeric = int.TryParse(value, out int staleDays);
            if (isNumeric == false)
                value = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DAYS_STALE_DEFAULT);
            if (GetOriginalSettingValue(PreferenceName.LOAD_STALE_DAYS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.LOAD_STALE_DAYS, value);

            value = WISChangeTextBox.Text;
            if (GetOriginalSettingValue(PreferenceName.STATUS_ACTIVE_TO_COMPLETE_PCN).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.STATUS_ACTIVE_TO_COMPLETE_PCN, value);

            value = ((ComboBoxItem)JournalSortingComboxBox.SelectedValue).Content.ToString();
            if (GetOriginalSettingValue(PreferenceName.JOURNAL_ORDERING).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.JOURNAL_ORDERING, value);

            value = ((ComboBoxItem)DeleteOptionComboBox.SelectedValue).Content.ToString();
            if (GetOriginalSettingValue(PreferenceName.DELETE_OPTION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DELETE_OPTION, value);

            // -- Backup Options ------------------------------------------------------------------------------------------------------------------------------------
            // -- Automatic Backup Checkbox
            val = AutomaticBackupCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_AUTOMATICALLY).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_AUTOMATICALLY, value);

            // -- Overwrite Backup Checkbox
            val = OverwriteBackupCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_SAME_DAY_OVERWRITE).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_SAME_DAY_OVERWRITE, value);

            // -- Backup Days Combobox
            value = ((ComboBoxItem)BackupDaysComboBox.SelectedValue).Content.ToString();
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_PERIOD_DAYS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_PERIOD_DAYS, value);

            // -- WorkItem Selection Combobox
            value = ((ComboBoxItem)WorkItemSelectionComboBox.SelectedValue).Content.ToString();
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_WORKITEM_SELECTION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_WORKITEM_SELECTION, value);

            // -- WorkItem Closed Days Textbox
            value = WorkItemClosedDaysTextBox.Text;
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DAYS_STALE).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_DAYS_STALE, value);

            // -- Due Date Combobox
            value = ((ComboBoxItem)DueDateComboBox.SelectedValue).Content.ToString();
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DUEDATE_SELECTION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_DUEDATE_SELECTION, value);

            // -- Status Combobox
            value = ((ComboBoxItem)StatusComboBox.SelectedValue).Content.ToString();
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_STATUS_SELECTION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_STATUS_SELECTION, value);

            // -- Status ExportVersion ComboBox
            value = ExportVersionComboBox.SelectedValue.ToString();
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DEFAULT_VERSION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_DEFAULT_VERSION, value);

            // -- Backup SaveTo Textbox
            value = BackupSaveToTextBox.Text;
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_SAVE_TO_LOCATION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_SAVE_TO_LOCATION, value);

            // -- Backup CopyTo Textbox
            value = BackupCopyToTextBox.Text;
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_COPY_LOCATION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_COPY_LOCATION, value);

            // -- Include Deleted Checkbox
            val = BackupIncludeDeletedCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_INCLUDE_DELETED).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_INCLUDE_DELETED, value);

            // -- Include Preferences Checkbox
            val = BackupIncludePreferencesCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_INCLUDE_PREFERENCES).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.DATA_EXPORT_INCLUDE_PREFERENCES, value);

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
        /// When this button is selected the user wants any changes made to the settings to be saved.
        /// This method collates the changes, ready to be passed back to the main program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateDifferencesToCurrentValues();
            WasDialogSubmitted = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WasDialogSubmitted = false;
            this.Close();
        }

        private void ApplyDefaults_Click(object sender, RoutedEventArgs e)
        {
            WasApplyDefaultsSelected = true;
            WasDialogSubmitted = true;
            this.Close();
        }

        private void DaysToCompleteSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WorkDaysToCompleteTextBox != null)
                WorkDaysToCompleteTextBox.Text = Convert.ToString((int)DaysToCompleteSlider.Value);
        }

        private void GracePeriodSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DueDateGracePeriodTextBox != null)
                DueDateGracePeriodTextBox.Text = Convert.ToString((int)GracePeriodSlider.Value);
        }

        private void ClosedToActiveAmountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WISChangeTextBox != null)
                WISChangeTextBox.Text = Convert.ToString((int)ClosedToActiveAmountSlider.Value);
        }

    }
}
