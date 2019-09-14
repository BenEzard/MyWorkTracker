using Microsoft.Win32;
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
    public partial class SettingsDialog : Window
    {
        /// <summary>
        /// The settings as they were originally passed into the dialog.
        /// </summary>
        private Dictionary<PreferenceName, Preference> _originalSettings = null;

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


        public SettingsDialog(Dictionary<PreferenceName, Preference> originalSettings)
        {
            InitializeComponent();
            _originalSettings = originalSettings;

            foreach (PreferenceName n in _originalSettings.Keys)
            {
                Preference s = _originalSettings[n];
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

            SelectComboItem(BackupDaysComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_PERIOD_DAYS));

            SelectComboItem(WorkItemSelectionComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_WORKITEM_SELECTION));
            WorkItemClosedDaysTextBox.Text = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DAYS_STALE);
            SelectComboItem(DueDateComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DUEDATE_SELECTION));
            SelectComboItem(StatusComboBox, GetOriginalSettingValue(PreferenceName.DATA_EXPORT_STATUS_SELECTION));

            if (GetOriginalSettingValue(PreferenceName.DATA_EXPORT_INCLUDE_DELETED) == "1")
                BackupIncludeDeletedCheckBox.IsChecked = true;

            BackupSaveToTextBox.Text = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_DEFAULT_LOCATION);
            BackupCopyToTextBox.Text = GetOriginalSettingValue(PreferenceName.DATA_EXPORT_COPY_LOCATION);
        }

        /// <summary>
        /// Convenience short-cut method for accessing the value of a Setting (based on the original value passed in).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetOriginalSettingValue(PreferenceName name)
        {
            _originalSettings.TryGetValue(name, out Preference sett);
            return sett.Value;
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

        }

        /// <summary>
        /// Method compares the original settings to the default values, and populates a collection of changes.
        /// </summary>
        private void GenerateDifferencesToDefaults()
        {
            foreach (PreferenceName name in _originalSettings.Keys)
            {
                Preference s = _originalSettings[name];
                if ((s.UserCanEdit) && (s.Value.Equals(s.DefaultValue, StringComparison.OrdinalIgnoreCase) == false)) {
                    _settingChanges.Add(name, s.DefaultValue);
                } 
            }

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
                value = "9999";
            if (GetOriginalSettingValue(PreferenceName.LOAD_STALE_DAYS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(PreferenceName.LOAD_STALE_DAYS, value);
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
            GenerateDifferencesToDefaults();
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
