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
        private Dictionary<SettingName, Setting> _originalSettings = null;

        /// <summary>
        /// A collection of any changes; the setting name and the new value as a String.
        /// </summary>
        private Dictionary<SettingName, string> _settingChanges = new Dictionary<SettingName, string>();
        public Dictionary<SettingName, string> GetChanges
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


        public SettingsDialog(Dictionary<SettingName, Setting> originalSettings)
        {
            InitializeComponent();
            _originalSettings = originalSettings;

            foreach (SettingName n in _originalSettings.Keys)
            {
                Setting s = _originalSettings[n];
            }

            PopulateDialog();
        }

        /// <summary>
        /// This method populates the dialog with the current values of the Settings.
        /// (It's a bit of a hack)
        /// </summary>
        private void PopulateDialog()
        {
            if (GetOriginalSettingValue(SettingName.SAVE_WINDOW_COORDS_ON_EXIT) == "1")
                SaveWindowCoordsOnExitCheckBox.IsChecked = true;

            DaysToCompleteSlider.Value = double.Parse(GetOriginalSettingValue(SettingName.DEFAULT_WORKITEM_LENGTH_DAYS));

            SelectComboItem(HourCombo, GetOriginalSettingValue(SettingName.DEFAULT_WORKITEM_COB_HOURS));

            string cobMins = GetOriginalSettingValue(SettingName.DEFAULT_WORKITEM_COB_MINS);
            SelectComboItem(MinuteCombo, ((cobMins == "0") ? "00" : cobMins));

            if (GetOriginalSettingValue(SettingName.DUE_DATE_CAN_BE_WEEKENDS) == "1")
                DueDateOnWeekendsCheckBox.IsChecked = true;

            DueDateGracePeriodTextBox.Text = GetOriginalSettingValue(SettingName.DUE_DATE_SET_WINDOW_MINUTES);
            GracePeriodSlider.Value = double.Parse(GetOriginalSettingValue(SettingName.DUE_DATE_SET_WINDOW_MINUTES));

            if (GetOriginalSettingValue(SettingName.CONFIRM_JOURNAL_DELETION) == "1")
                ConfirmJournalCheckBox.IsChecked = true;
        }

        /// <summary>
        /// Convenience short-cut method for accessing the value of a Setting (based on the original value passed in).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetOriginalSettingValue(SettingName name)
        {
            _originalSettings.TryGetValue(name, out Setting sett);
            return sett.Value;
        }

        /// <summary>
        /// Method compares the original settings and the current values, and populates a collection of changes.
        /// (It's a bit of a hack)
        /// </summary>
        private void GenerateDifferencesToCurrentValues()
        {
            string value = WorkDaysToCompleteTextBox.Text;
            if (GetOriginalSettingValue(SettingName.DEFAULT_WORKITEM_LENGTH_DAYS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.DEFAULT_WORKITEM_LENGTH_DAYS, value);

            var cbItem = (ComboBoxItem)(HourCombo.SelectedValue);
            value = (string)cbItem.Content;
            if (GetOriginalSettingValue(SettingName.DEFAULT_WORKITEM_COB_HOURS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.DEFAULT_WORKITEM_COB_HOURS, value);

            cbItem = (ComboBoxItem)(MinuteCombo.SelectedValue);
            value = (string)cbItem.Content;
            // A little bit of extra code used, so that 0 and 00 don't detect as a change.
            string originalValue = (GetOriginalSettingValue(SettingName.DEFAULT_WORKITEM_COB_MINS));
            originalValue = originalValue.Equals("0") ? "00" : originalValue;
            if (originalValue.Equals(value, StringComparison.OrdinalIgnoreCase) == false) 
                _settingChanges.Add(SettingName.DEFAULT_WORKITEM_COB_MINS, value);

            bool? val = DueDateOnWeekendsCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(SettingName.DUE_DATE_CAN_BE_WEEKENDS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.DUE_DATE_CAN_BE_WEEKENDS, value);

            value = DueDateGracePeriodTextBox.Text;
            if (GetOriginalSettingValue(SettingName.DUE_DATE_SET_WINDOW_MINUTES).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.DUE_DATE_SET_WINDOW_MINUTES, value);

            val = ConfirmJournalCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(SettingName.CONFIRM_JOURNAL_DELETION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.CONFIRM_JOURNAL_DELETION, value);

            val = SaveWindowCoordsOnExitCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(SettingName.SAVE_WINDOW_COORDS_ON_EXIT).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.SAVE_WINDOW_COORDS_ON_EXIT, value);
        }

        /// <summary>
        /// Method compares the original settings to the default values, and populates a collection of changes.
        /// </summary>
        private void GenerateDifferencesToDefaults()
        {
            foreach (SettingName name in _originalSettings.Keys)
            {
                Setting s = _originalSettings[name];
                if ((s.UserCanEdit) && (s.Value.Equals(s.DefaultValue, StringComparison.OrdinalIgnoreCase) == false)) {
                    _settingChanges.Add(name, s.DefaultValue);
                } 
            }

            string value = WorkDaysToCompleteTextBox.Text;
            if (GetOriginalSettingValue(SettingName.DEFAULT_WORKITEM_LENGTH_DAYS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.DEFAULT_WORKITEM_LENGTH_DAYS, value);

            var cbItem = (ComboBoxItem)(HourCombo.SelectedValue);
            value = (string)cbItem.Content;
            if (GetOriginalSettingValue(SettingName.DEFAULT_WORKITEM_COB_HOURS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.DEFAULT_WORKITEM_COB_HOURS, value);

            cbItem = (ComboBoxItem)(MinuteCombo.SelectedValue);
            value = (string)cbItem.Content;
            // A little bit of extra code used, so that 0 and 00 don't detect as a change.
            string originalValue = (GetOriginalSettingValue(SettingName.DEFAULT_WORKITEM_COB_MINS));
            originalValue = originalValue.Equals("0") ? "00" : originalValue;
            if (originalValue.Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.DEFAULT_WORKITEM_COB_MINS, value);

            bool? val = DueDateOnWeekendsCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(SettingName.DUE_DATE_CAN_BE_WEEKENDS).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.DUE_DATE_CAN_BE_WEEKENDS, value);

            value = DueDateGracePeriodTextBox.Text;
            if (GetOriginalSettingValue(SettingName.DUE_DATE_SET_WINDOW_MINUTES).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.DUE_DATE_SET_WINDOW_MINUTES, value);

            val = ConfirmJournalCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(SettingName.CONFIRM_JOURNAL_DELETION).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.CONFIRM_JOURNAL_DELETION, value);

            val = SaveWindowCoordsOnExitCheckBox.IsChecked;
            value = (val.HasValue && val.Value) ? "1" : "0";
            if (GetOriginalSettingValue(SettingName.SAVE_WINDOW_COORDS_ON_EXIT).Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                _settingChanges.Add(SettingName.SAVE_WINDOW_COORDS_ON_EXIT, value);
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
    }
}
