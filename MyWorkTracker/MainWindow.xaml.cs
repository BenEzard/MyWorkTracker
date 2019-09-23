using MyWorkTracker.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyWorkTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MWTModel _model = null;
        private MWTController _controller = null;

        /// <summary>
        /// This is here (as opposed to in the model) because it's only related to the UI, not the application.
        /// </summary>
        Border _originalBorder = new Border();

        public MainWindow()
        {
            InitializeComponent();
            _model = new MWTModel();
            _controller = new MWTController(_model);

            _model.appEvent += AppEventListener;

            // Populate the Overview (the Active tab)
            Overview.ItemsSource = _model.GetActiveWorkItems();
            ClosedListView.ItemsSource = _model.GetClosedWorkItems();  //_model.GetWorkItems();

            WorkItemStatus.ItemsSource = _model.GetWorkItemStatuses();

            // Set the application name and version.
            this.Title = $"{_model.GetAppPreferenceValue(PreferenceName.APPLICATION_NAME)} [v{_model.GetAppPreferenceValue(PreferenceName.APPLICATION_VERSION)}]";

            DataContext = _model;

            ApplyWindowPreferences();
        }

        /// <summary>
        /// Apply the user-defined window preferences.
        /// (This doesn't use binding because I didn't want to have to expose the variables --- and not sure if there's another way)
        /// </summary>
        private void ApplyWindowPreferences()
        {
            string[] coords = _model.GetAppPreferenceValue(PreferenceName.APPLICATION_WINDOW_COORDS).Split(',');
            this.Left = double.Parse(coords[0]);
            this.Top = double.Parse(coords[1]);
            this.Width = double.Parse(coords[2]);
            this.Height = double.Parse(coords[3]);
        }

        /// <summary>
        /// Highlight a button with the specified background colour.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="backgroundBrush"></param>
        private void HighlightButton(Button b, Brush backgroundBrush)
        {
            b.Background = backgroundBrush;
            b.Foreground = Brushes.White;
        }

        /// <summary>
        /// A listener method for the AppEvents.
        /// This method handles all of the AppEvents that are fired from the model.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void AppEventListener(Object o, AppEventArgs args)
        {
            switch(args.Action)
            {
                case AppAction.CREATE_WORK_ITEM:
                    WorkItemStatus.SelectedItem = _controller.GetWorkItemStatus(_model.SelectedWorkItem.Status);
                    _model.IsBindingLoading = false;
                    SelectedTaskTitleField.Focus();

                    _model.SetApplicationMode(DataEntryMode.ADD);
                    break;

                case AppAction.SELECT_WORK_ITEM:
                    // Because the UI is divided into two lists (active and closed), we need to toggle the selections of both lists.

                    WorkItem wi = args.CurrentWorkItemSelection;

                    // The Work Item selection is of an 'Active' status.
                    // If the selection is Active, then we want to clear the Closed list selections.
                    if (wi.IsConsideredActive)
                    {
                        ClosedListView.SelectedItem = null;
                        // If an item isn't selected on the Overview list, and is in the list, then select it now.
                        if ((Overview.SelectedItem == null) && (Overview.Items.Contains(wi)))
                        {
                            Overview.SelectedItem = wi;
                        }
                    }
                    else
                    {
                        // The Work Item selection is of a 'Closed' status.
                        // If the selection is Closed, then we want to clear the Active list selections.
                        Overview.SelectedItem = null;
                        if ((ClosedListView.SelectedItem == null) && (ClosedListView.Items.Contains(wi)))
                        {
                            ClosedListView.SelectedItem = wi;
                        }
                    }

                    // Set the Work Item Status & the DueInDays control to the values of the selected WorkItem
                    WorkItemStatus.SelectedItem = _controller.GetWorkItemStatus(wi.Status);
                    DueInDaysTextField.Text = DateMethods.GenerateDateDifferenceLabel(DateTime.Now, _model.SelectedWorkItem.DueDate, true);

                    // Check to see if the Journal entries for this Work Item have been loaded. Load them if not.
                    if (_model.SelectedWorkItem.Meta.AreJournalItemsLoaded == false)
                    {
                        _controller.LoadJournalEntries(_model.SelectedWorkItem);
                    }
                    JournalEntryList.ItemsSource = _model.SelectedWorkItem.Journals;

                    _model.SetApplicationMode(DataEntryMode.EDIT);

                    _model.IsBindingLoading = false;

                    break;

                case AppAction.WORK_ITEM_ADDED:

                    SaveButton.Background = Brushes.SteelBlue;
                    SaveButton.Content = "Save";
                    break;

                case AppAction.WORK_ITEM_STATUS_CHANGED:
                    // Before processing any actions, check to see if Binding is loading.
                    // If it is, then do nothing.
                    if (_model.IsBindingLoading == false)
                    {
                        // Unpack the event
                        WorkItem wi2 = args.CurrentWorkItemSelection;
                        WorkItemStatus newWorkItemStatus = (WorkItemStatus)args.Object1;
                        WorkItemStatus oldWorkItemStatus = (WorkItemStatus)args.Object2;

                        // Check to see if the change in status is a change between active/closed
                        if (newWorkItemStatus.IsConsideredActive != oldWorkItemStatus.IsConsideredActive)
                        {
                            // Active-to-Closed
                            // If the status is not considered to be active, then disable the progress slider.
                            if (newWorkItemStatus.IsConsideredActive == false)
                            {
                                WorkItemProgressSlider.IsEnabled = false;
                                _model.SwapList(true, wi2);
                                //_model.SetSelectedWorkItem(null);
                            }
                            else // Closed-to-Active
                            {
                                _model.SwapList(false, wi2);
                                OverviewAreaTabs.SelectedIndex = 0;
                                // If it's Active (i.e. not Completed) and at 100% progress then set it back to 95%
                                // (This is to prevent a Completed-then-Active being auto-set back to Completed when loaded again)
                                WorkItemProgressSlider.IsEnabled = true;
                                if (wi2.Completed == 100)
                                {
                                    string strValue = _model.GetAppPreferenceValue(PreferenceName.STATUS_ACTIVE_TO_COMPLETE_PCN);
                                    wi2.Completed = int.Parse(strValue);
                                }
                            }
                        } else
                        {
                            // do nothing
                        }
                    }
                    break;

                case AppAction.SET_APPLICATION_MODE:
                    if (_model.GetApplicationMode() == DataEntryMode.ADD)
                    {
                        // Make the 'New Work Item' button unavailable.
                        // TODO: Not working
                        NewWorkItemButton.IsEnabled = false;

                        Overview.Background = Brushes.WhiteSmoke;

                        HighlightButton(SaveButton, Brushes.Green);
                        HighlightButton(CancelButton, Brushes.Red);
                        SaveButton.Content = "Create Work Item";
                    }
                    else if (_model.GetApplicationMode() == DataEntryMode.EDIT)
                    {
                        // Make the 'New Work Item' button available.
                        NewWorkItemButton.IsEnabled = true;
                        Overview.Background = Brushes.White;
                        SaveButton.Content = "Save";
                    }
                    break;

                case AppAction.PREFERENCE_CHANGED:
                    // Intentionally there is no in-built protection to stop a user from editing a non-editable value. (Keeping my options open)

                    // Change in memory.
                    Enum.TryParse(args.Object1.ToString(), out PreferenceName preferenceName);

                    _model.GetAppPreferenceCollection()[preferenceName].Value = args.Object3.ToString();

                    // Change in storage.
                    _controller.UpdateAppPreference(preferenceName, args.Object3.ToString());

                    break;

                case AppAction.JOURNAL_ENTRY_DELETED:
                    _controller.DeleteDBJournalEntry((JournalEntry)args.Object1);
                    break;

                case AppAction.JOURNAL_ENTRY_ADDED:
                    _controller.InsertDBJournalEntry(args.CurrentWorkItemSelection.Meta.WorkItem_ID, (JournalEntry)args.Object1);
                    _controller.AddJournalEntry(args.CurrentWorkItemSelection, (JournalEntry)args.Object1);
                    break;

                case AppAction.JOURNAL_ENTRY_EDITED:
                    _controller.UpdateDBJournalEntry((JournalEntry)args.Object2);
                    int indexOf = _model.SelectedWorkItem.Journals.IndexOf((JournalEntry)args.Object1);
                    _model.SelectedWorkItem.Journals.Remove((JournalEntry)args.Object1);
                    _model.SelectedWorkItem.Journals.Insert(indexOf, (JournalEntry)args.Object2);
                    break;

                case AppAction.DATA_EXPORT:
                    ExportWindow exportDialog = new ExportWindow(_controller.GetPreferencesBeginningWith("DATA_EXPORT"));
                    exportDialog.ShowDialog();

                    if (exportDialog.WasSubmitted)
                    {
                        string dbConn = null;
                        if (exportDialog.ExportFromSystemFile)
                            dbConn = _controller.DBConnectionString;
                        else
                        {
                            dbConn = "data source=" + exportDialog.ExportFile;

                            // Save the last directory where the export has come from.
                            int index = exportDialog.ExportFile.LastIndexOf('\\');
                            string exportDirectory = exportDialog.ExportFile.Substring(0, index - 1);
                            if (_model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_LAST_DIRECTORY).Equals(exportDirectory) == false)
                            {
                                _controller.UpdateAppPreference(PreferenceName.DATA_EXPORT_LAST_DIRECTORY, exportDirectory);
                            }
                        }

                        _controller.ExportToXML(dbConn, exportDialog.SaveLocation, exportDialog.IncludeSettings, exportDialog.WorkItemType, exportDialog.StaleNumber, 
                            exportDialog.IncludeDeleted, !exportDialog.AllStatuses, !exportDialog.AllDueDates);

                        MessageBox.Show($"Export has been saved to {exportDialog.SaveLocation}", "Export Complete");
                    }
                    break;

                case AppAction.DATA_IMPORT:
                    ImportWindow importDialog = new ImportWindow(_model.GetAppPreferenceValue(PreferenceName.APPLICATION_VERSION),
                        _model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_COPY_LOCATION));
                    importDialog.ShowDialog();

                    if ((importDialog.WasSubmitted) && (importDialog.ImportSelectionCount > 0))
                    {
                        Dictionary<PreferenceName, string> preferences = new Dictionary<PreferenceName, string>();
                        if (importDialog.ImportPreferencesSelected)
                        {
                            preferences = importDialog.LoadedPreferences;
                        }
                        _controller.ImportData(importDialog.GetImportVersion, preferences, importDialog.GetImportStatuses, importDialog.LoadedXMLDocument);
                    }
                    break;

                case AppAction.WORK_ITEM_DELETE_LOGICAL:
                        _controller.DeleteWorkItem(args.CurrentWorkItemSelection, true);
                    break;

                case AppAction.WORK_ITEM_DELETE_PHYSICAL:
                    _controller.DeleteWorkItem(args.CurrentWorkItemSelection, false);
                    break;
            }

        }

        /// <summary>
        /// When the 'Create a New Work Item' is selected, a dummy record will be created. 
        /// Once that dummy record has been satisfactorily filled out, then it will be generated as a new work item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewWorkItemSelected(object sender, RoutedEventArgs e)
        {
            _controller.CreateNewWorkItem();
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
            c.BorderThickness = new Thickness(4,0,0,0);
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

        /// <summary>
        /// This button is only enabled when the application is in add-mode, which means it's part way through adding a WorkItem.
        /// All other saving will happen passively (when the user changes WorkItem selection or closes the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            WorkItem selectedWorkItem = _model.SelectedWorkItem;

            // If the application is in 'add mode' then we want to insert a record.
            if (_model.GetApplicationMode() == DataEntryMode.ADD)
            {
                _controller.InsertDBWorkItem(selectedWorkItem);
                _model.AddWorkItem(selectedWorkItem, true, true);
                _model.SetApplicationMode(DataEntryMode.EDIT);
            }
        }

        /// <summary>
        /// Checks to see what aspects of a WorkItem might need updating and updates them accordingly to the database.
        /// </summary>
        private void UpdateWorkItemDBAsRequired()
        {
            WorkItem selectedWorkItem = _model.SelectedWorkItem;
            // Ensure that a WorkItem has been selected.
            if ((selectedWorkItem != null) && (_model.GetApplicationMode() == DataEntryMode.EDIT))
            {
                if (selectedWorkItem.Meta.WorkItemDBNeedsUpdate)
                {
                    _controller.UpdateDBWorkItem(selectedWorkItem);
                }
                if (selectedWorkItem.Meta.WorkItemStatusNeedsUpdate)
                {
                    var wis = (WorkItemStatus)WorkItemStatus.SelectedItem;
                    _controller.InsertDBWorkItemStatus(selectedWorkItem, wis.Status);
                }
            }
        }

        private void ActiveWorkItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If the application is in 'add mode' (ADD_WORK_ITEM) then don't allow any selections.
            if (_model.IsApplicationInAddMode)
            {
                SelectedTaskTitleField.Focus();
            }
            else
            {
                UpdateWorkItemDBAsRequired();
                Overview.Background = Brushes.White;
                WorkItem wi = (WorkItem)(sender as ListBox).SelectedItem;
                // The ActiveWorkItemSelectionChanged may be set to null (if a Closed selection is made), so need to handle that here.
                if (wi != null)
                {
                    _model.SetSelectedWorkItem(wi);
                }
            }
        }

        /// <summary>
        /// Event is called when any of the following controls fire an event to say they've been updated
        /// * Title
        /// * Description
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkItemChanged(object sender, TextChangedEventArgs e)
        {
            if (_model.SelectedWorkItem != null)
                _model.SelectedWorkItem.Meta.WorkItemDBNeedsUpdate = true;
        }

        /// <summary>
        /// Event is called when any of the following controls fire an event to say they've been updated:
        /// * Progress Bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkItemChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 100)
            {
                var s = _controller.GetWorkItemStatuses(false, true).ToArray();
                WorkItemStatus.SelectedValue = _controller.GetWorkItemStatuses(false, true).ToArray()[0];
            }

            if (_model.SelectedWorkItem != null)
                _model.SelectedWorkItem.Meta.WorkItemDBNeedsUpdate = true;
        }

        private void WorkItemStatusChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_model.SelectedWorkItem != null)
            {
                WorkItemStatus wis = (WorkItemStatus)WorkItemStatus.SelectedItem;
                _controller.SetWorkItemStatus(_model.SelectedWorkItem, wis);
            }
        }

        /// <summary>
        /// Select a new DueDate for the WorkItem.
        /// If the DueDate (from database) has been set within x mins of now, UPDATE the record instead of INSERTING it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DueDateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_model.SelectedWorkItem != null)
            {
                WorkItem selectedWI = _model.SelectedWorkItem;
                DateTime currentDueDate = selectedWI.DueDate;
                var ddDialog = new DueDateDialog(currentDueDate);
                ddDialog.Owner = this;
                ddDialog.ShowDialog();

                if (ddDialog.WasDialogSubmitted)
                {
                    if (ddDialog.NewDateTime.Equals(currentDueDate))
                    {
                        // Do nothing
                    }
                    else
                    {
                        int rowID = -1;
                        // If the DueDate (from database) has been set within x mins of now, UPDATE the record instead of INSERTING it.
                        int minutesSinceLastSet = DateTime.Now.Subtract(selectedWI.Meta.DueDateUpdateDateTime).Minutes;
                        if (minutesSinceLastSet < Convert.ToInt32(_controller.GetMWTModel().GetAppPreferenceValue(PreferenceName.DUE_DATE_SET_WINDOW_MINUTES)))
                        {
                            // Update
                            rowID = _controller.UpdateDBDueDate(selectedWI, ddDialog.NewDateTime, ddDialog.ChangeReason);
                        }
                        else
                        {
                            // Insert
                            rowID = _controller.InsertDBDueDate(selectedWI, ddDialog.NewDateTime, ddDialog.ChangeReason);
                        }

                        // Update the update/record change time.
                        selectedWI.Meta.DueDate_ID = rowID;
                        selectedWI.Meta.DueDateUpdateDateTime = DateTime.Now;
                        selectedWI.DueDate = ddDialog.NewDateTime;
                        selectedWI.Meta.DueDateUpdateDateTime = DateTime.Now;

                        // Refresh the time label
                        DueInDaysTextField.Text = DateMethods.GenerateDateDifferenceLabel(DateTime.Now, _model.SelectedWorkItem.DueDate, true);
                    }
                }
            }
        }

        /// <summary>
        /// Perform operations to close the application.
        /// - Save any changes required to the selected WorkItem.
        /// - If SAVE_WINDOW_COORDS_ON_EXIT == 1, save window position
        /// - If DATA_EXPORT_AUTOMATICALLY == 1 and either not done or today, create backup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Check to see if a Save is required based on the current selection
            if (_model.SelectedWorkItem != null)
            {
                UpdateWorkItemDBAsRequired();
            }

            // If set as a preference, save the window location.
            if (_model.GetAppPreferenceValue(PreferenceName.SAVE_WINDOW_COORDS_ON_EXIT).Equals("1")) {
                string coords = $"{this.Left},{this.Top},{this.Width},{this.Height}";
                _model.FireUpdateAppPreference(PreferenceName.APPLICATION_WINDOW_COORDS, coords);
            }

            if (_model.GetAppPreferenceAsBooleanValue(PreferenceName.DATA_EXPORT_AUTOMATICALLY))
            {
                int backupEveryDays = Int32.Parse(_model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_PERIOD_DAYS));        // Get how often backups should be done.
                DateTime backupLastDone = DateTime.Parse(_model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_LAST_DONE));   // Get when it was last done.
                double daysSinceLastBackup = (DateTime.Now.Date - backupLastDone.Date).TotalDays;                               // Get days since the backup was done.

                string exportLastDoneStr = _model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_LAST_DONE);
                if ((daysSinceLastBackup >= backupEveryDays) 
                    || ((_model.GetAppPreferenceAsBooleanValue(PreferenceName.DATA_EXPORT_SAME_DAY_OVERWRITE) && (daysSinceLastBackup == 0))))
                {
                    _controller.CreateBackupFile();
                }

            }
        }

        /// <summary>
        /// Cancel the creation of a new WorkItem.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _model.SelectedWorkItem = null;
            _model.SetApplicationMode(DataEntryMode.EDIT);
        }

        /// <summary>
        /// Open the Journal Dialog in readiness to add a record.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddJournalButton_Click(object sender, RoutedEventArgs e)
        {
            var jeDialog = new JournalDialog(_model.SelectedWorkItem, new JournalEntry(), DataEntryMode.ADD);
            jeDialog.Owner = this;
            jeDialog.ShowDialog();

            if (jeDialog.WasDialogSubmitted)
            {
                JournalEntry je = jeDialog.JournalEntry;
                _model.FireAddJournalEntry(_model.SelectedWorkItem, je);
            }
        }

        /// <summary>
        /// Open the Journal Dialog in readiness to edit a record.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditJournalButton_Click(object sender, RoutedEventArgs e)
        {
            JournalEntry oldJE = (JournalEntry)JournalEntryList.SelectedItem;
            var journalDialog = new JournalDialog(_model.SelectedWorkItem, oldJE, DataEntryMode.EDIT);
            journalDialog.Owner = this;
            journalDialog.ShowDialog();

            if (journalDialog.WasDialogSubmitted)
            {
                JournalEntry newJournalEntry = journalDialog.JournalEntry;
                newJournalEntry.ModificationDateTime = DateTime.Now;
                _model.FireEditJournalEntry(_model.SelectedWorkItem, oldJE, newJournalEntry);
            }
        }

        /// <summary>
        /// The user has selected to delete a journal item.
        /// If CONFIRM_JOURNAL_DELETION is set to true, the Journal Dialog will be opened to confirm the deletion.
        /// If not, the journal will be deleted.
        /// When the dialog is closed, CONFIRM_JOURNAL_DELETION value (from the dialog) is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteJournalButton_Click(object sender, RoutedEventArgs e)
        {
            JournalEntry je = (JournalEntry)JournalEntryList.SelectedItem;

            // Check to see if journal entry deletion should be confirmed.
            if (_model.GetAppPreferenceValue(PreferenceName.CONFIRM_JOURNAL_DELETION) == "1")
            {
                var journalDialog = new JournalDialog(_model.SelectedWorkItem, je, DataEntryMode.DELETE);
                journalDialog.Owner = this;
                journalDialog.ShowDialog();

                if (journalDialog.WasDialogSubmitted)
                {
                    if ((journalDialog.DontConfirmFutureDeletes.HasValue) && (journalDialog.DontConfirmFutureDeletes.Value))
                    {
                        _model.FireUpdateAppPreference(PreferenceName.CONFIRM_JOURNAL_DELETION, "0");
                    }
                    _model.FireDeleteJournalEntry(_model.SelectedWorkItem, je);
                }

            }
            else 
            {
                _model.FireDeleteJournalEntry(_model.SelectedWorkItem, je);
            }


        }

        private void JournalEntryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _model.SelectedJournalEntry = (JournalEntry)JournalEntryList.SelectedItem;
        }

        /// <summary>
        /// Open the Settings Dialog (User Preferences)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

            foreach (PreferenceName n in _model.GetAppPreferenceCollection().Keys)
            {
                Preference s = _model.GetAppPreferenceCollection()[n];
            }

            var sDialog = new PreferenceDialog(_model.GetAppPreferenceCollection());
            sDialog.Owner = this;
            sDialog.ShowDialog();

            // If the dialog was submitted
            if (sDialog.WasDialogSubmitted)
            {
                if (sDialog.WasSaveWindowCoordinatesSelected)
                {
                    string coords = $"{this.Left},{this.Top},{this.Width},{this.Height}";
                    _model.FireUpdateAppPreference(PreferenceName.APPLICATION_WINDOW_COORDS, coords);
                }

                if (sDialog.WasApplyDefaultsSelected)
                {
                    _controller.ResetAllPreferences();
                }
                else
                { // Apply changes
                    Dictionary<PreferenceName, string> settingChanges = sDialog.GetChanges;
                    foreach (PreferenceName name in settingChanges.Keys)
                    {
                        _model.FireUpdateAppPreference(name, settingChanges[name]);
                    }
                }
            }
        }

        private void HandleDoubleClickOnJournal(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _model.SelectedJournalEntry = (JournalEntry)JournalEntryList.SelectedItem;
            EditJournalButton_Click(sender, e);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            _model.FireDataExportRequest();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            _model.FireDataImportRequest();
        }

        private void DeleteWorkItemButton_Click(object sender, RoutedEventArgs e)
        {
            bool deleteLogically = true;
            if (_model.GetAppPreferenceValue(PreferenceName.DELETE_OPTION).StartsWith("physically"))
                deleteLogically = false;

            _model.FireWorkItemDelete(_model.SelectedWorkItem, deleteLogically);

        }
    }
}
