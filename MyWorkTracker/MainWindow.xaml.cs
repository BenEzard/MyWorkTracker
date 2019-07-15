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

            GraphicalTaskView.ItemsSource = _model.GetWorkItems();
            WorkItemStatus.ItemsSource = _model.GetWorkItemStatuses();

            // Set the application name and version.
            this.Title = $"{_model.GetAppSettingValue(SettingName.APPLICATION_NAME)} [v{_model.GetAppSettingValue(SettingName.APPLICATION_VERSION)}]";

            DataContext = _model;

            ApplyWindowPreferences();
        }

        /// <summary>
        /// Apply the user-defined window preferences.
        /// (This doesn't use binding because I didn't want to have to expose the variables --- and not sure if there's another way)u
        /// </summary>
        private void ApplyWindowPreferences()
        {
            string[] coords = _model.GetAppSettingValue(SettingName.APPLICATION_WINDOW_COORDS).Split(',');
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
                case AppAction.CREATE_NEW_WORK_ITEM:
                    WorkItemStatus.SelectedItem = _controller.GetWorkItemStatus(_model.SelectedWorkItem.Status);
                    _model.IsBindingLoading = false;
                    SelectedTaskTitleField.Focus();

                    _model.SetApplicationMode(DataEntryMode.ADD);
                    break;

                case AppAction.SELECT_WORK_ITEM:
                    // Set to the Status of the selected WorkItem
                    WorkItemStatus.SelectedItem = _controller.GetWorkItemStatus(args.CurrentWorkItemSelection.Status);
                    DueInDaysTextField.Text = DateMethods.GenerateDateDifferenceLabel(DateTime.Now, _model.SelectedWorkItem.DueDate, true);

                    // Check to see if the Journal entries for this Work Item have been loaded
                    if (_model.SelectedWorkItem.Meta.AreJournalItemsLoaded == false)
                    {
                        _controller.LoadJournalsFromDB(_model.SelectedWorkItem);
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
                    // Check to see if the Status has been set to a 'completed' type. If so, disable the progress bar.
                    bool isCompletedStatus = false;
                    foreach (WorkItemStatus wis in _controller.GetWorkItemStatuses(false))
                    {
                        if (wis.Status.Equals(args.CurrentWorkItemSelection.Status))
                        {
                            isCompletedStatus = true;
                            break;
                        }
                    }

                    if (isCompletedStatus)
                    {
                        WorkItemProgressSlider.IsEnabled = false;
                    }
                    else
                    {
                        WorkItemProgressSlider.IsEnabled = true;
                    }
                    break;

                case AppAction.SET_APPLICATION_MODE:
                    if (_model.GetApplicationMode() == DataEntryMode.ADD)
                    {
                        // Make the 'New Work Item' button unavailable.
                        // TODO: Not working
                        NewWorkItemButton.IsEnabled = false;

                        GraphicalTaskView.Background = Brushes.WhiteSmoke;

                        HighlightButton(SaveButton, Brushes.Green);
                        HighlightButton(CancelButton, Brushes.Red);
                        SaveButton.Content = "Create Work Item";
                    }
                    else if (_model.GetApplicationMode() == DataEntryMode.EDIT)
                    {
                        // Make the 'New Work Item' button available.
                        NewWorkItemButton.IsEnabled = true;
                        GraphicalTaskView.Background = Brushes.White;
                        SaveButton.Content = "Save";
                    }
                    break;

                case AppAction.APPLICATION_SETTING_CHANGED:
                    // Intentionally there is no in-built protection to stop a user from editing a non-editable value. (Keeping my options open)

                    // Change in memory.
                    _model.GetAppSettingCollection()[args.Setting.Name].Value = args.StringValue;

                    // Change in storage.
                    _controller.UpdateApplicationSettingDB(args.Setting.Name, args.StringValue);

                    break;

                case AppAction.JOURNAL_ENTRY_DELETED:
                    _controller.DeleteDBJournalEntry(args.JournalEntry);
                    break;

                case AppAction.JOURNAL_ENTRY_ADDED:
                    _controller.InsertDBJournalEntry(args.CurrentWorkItemSelection.Meta.WorkItem_ID, args.JournalEntry);
                    _controller.AddJournalEntry(args.CurrentWorkItemSelection, args.JournalEntry);
                    break;

                case AppAction.JOURNAL_ENTRY_EDITED:
                    _controller.UpdateDBJournalEntry(args.JournalEntry2);
                    int indexOf = _model.SelectedWorkItem.Journals.IndexOf(args.JournalEntry);
                    _model.SelectedWorkItem.Journals.Remove(args.JournalEntry);
                    _model.SelectedWorkItem.Journals.Insert(indexOf, args.JournalEntry2);
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
            if (_model.IsApplicationInAddMode)
            {
                SelectedTaskTitleField.Focus();
            }
            // If the application is in 'add mode' (ADD_WORK_ITEM) then don't allow any selections.
            else
            {
                UpdateWorkItemDBAsRequired();
                GraphicalTaskView.Background = Brushes.White;
                WorkItem wi = (WorkItem)(sender as ListBox).SelectedItem;
                _model.SetSelectedWorkItem(wi);
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
                DateTime currentDueDate = _model.SelectedWorkItem.DueDate;
                var ddw = new DueDateDialog(currentDueDate);
                ddw.Owner = this;
                ddw.ShowDialog();

                if (ddw.WasDialogSubmitted)
                {
                    if (ddw.NewDateTime.Equals(currentDueDate))
                    {
                        // Do nothing
                    }
                    else
                    {
                        // If the DueDate (from database) has been set within x mins of now, UPDATE the record instead of INSERTING it.
                        int minutesSinceLastSet = DateTime.Now.Subtract(_model.SelectedWorkItem.Meta.DueDateUpdateDateTime).Minutes;
                        if (minutesSinceLastSet < Convert.ToInt32(_controller.GetMWTModel().GetAppSettingValue(SettingName.DUE_DATE_SET_WINDOW_MINUTES)))
                        {
                            // Update
                            _controller.UpdateDBDueDate(_model.SelectedWorkItem, ddw.NewDateTime, ddw.ChangeReason);
                        }
                        else
                        {
                            // Insert
                            _controller.InsertDBDueDate(_model.SelectedWorkItem, ddw.NewDateTime, ddw.ChangeReason);
                        }

                        // Update the update/record change time.
                        _model.SelectedWorkItem.DueDate = ddw.NewDateTime;
                        _model.SelectedWorkItem.Meta.DueDateUpdateDateTime = DateTime.Now;

                        // Refresh the time label
                        DueInDaysTextField.Text = DateMethods.GenerateDateDifferenceLabel(DateTime.Now, _model.SelectedWorkItem.DueDate, true);
                    }
                }
            }
        }

        /// <summary>
        /// When the window closes, make sure that a selected WorkItem is saved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_model.SelectedWorkItem != null)
                UpdateWorkItemDBAsRequired();

            if (_model.GetAppSettingValue(SettingName.SAVE_WINDOW_COORDS_ON_EXIT).Equals("1")) {
                string coords = $"{this.Left},{this.Top},{this.Width},{this.Height}";
                _model.FireUpdateAppSetting(SettingName.APPLICATION_WINDOW_COORDS, coords);
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
            if (_model.GetAppSettingValue(SettingName.CONFIRM_JOURNAL_DELETION) == "1")
            {
                var journalDialog = new JournalDialog(_model.SelectedWorkItem, je, DataEntryMode.DELETE);
                journalDialog.Owner = this;
                journalDialog.ShowDialog();

                if (journalDialog.WasDialogSubmitted)
                {
                    if ((journalDialog.DontConfirmFutureDeletes.HasValue) && (journalDialog.DontConfirmFutureDeletes.Value))
                    {
                        _model.FireUpdateAppSetting(SettingName.CONFIRM_JOURNAL_DELETION, "0");
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

            foreach (SettingName n in _model.GetAppSettingCollection().Keys)
            {
                Setting s = _model.GetAppSettingCollection()[n];
            }


            var sDialog = new SettingsDialog(_model.GetAppSettingCollection());
            sDialog.Owner = this;
            sDialog.ShowDialog();

            // If the dialog was submitted
            if (sDialog.WasDialogSubmitted)
            {
                if (sDialog.WasSaveWindowCoordinatesSelected)
                {
                    string coords = $"{this.Left},{this.Top},{this.Width},{this.Height}";
                    _model.FireUpdateAppSetting(SettingName.APPLICATION_WINDOW_COORDS, coords);
                }

                Dictionary<SettingName, string> settingChanges = sDialog.GetChanges;
                foreach(SettingName name in settingChanges.Keys)
                {
                    _model.FireUpdateAppSetting(name, settingChanges[name]);
                }
            }
        }
    }
}
