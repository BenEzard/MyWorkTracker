using MyWorkTracker.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            
            
            // TODO test
/*            var list = new List<JournalEntry>();
            list.Add(new JournalEntry("This is a test entry", "With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata."));
            list.Add(new JournalEntry("This is a second entry", "With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-"));
            list.Add(new JournalEntry("This is a test entry", "With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata."));
            list.Add(new JournalEntry("This is a second entry", "With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-"));
            list.Add(new JournalEntry("This is a test entry", "With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata."));
            list.Add(new JournalEntry("This is a second entry", "With more details than a \n" +
                "cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-"));
            list.Add(new JournalEntry("This is a test entry", "With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata."));
            list.Add(new JournalEntry("This is a second entry", "With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-"));
            JournalEntryList.ItemsSource = list;*/

            // Set the application name and version.
            this.Title = $"{_model.GetAppSetting(SettingName.APPLICATION_NAME)} [v{_model.GetAppSetting(SettingName.APPLICATION_VERSION)}]";

            DataContext = _model;
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
            if (args.Action == AppAction.CREATE_NEW_WORK_ITEM)
            {
                WorkItemStatus.SelectedItem = _controller.GetWorkItemStatus(_model.SelectedWorkItem.Status);
                _model.IsBindingLoading = false;
                SelectedTaskTitleField.Focus();

                _model.SetApplicationMode(DataEntryMode.ADD_WORK_ITEM);
            }

            else if (args.Action == AppAction.SELECT_WORK_ITEM)
            {
                // Set to the Status of the selected WorkItem
                WorkItemStatus.SelectedItem = _controller.GetWorkItemStatus(args.CurrentSelection.Status);
                DueInDaysTextField.Text = DateMethods.GenerateDateDifferenceLabel(DateTime.Now, _model.SelectedWorkItem.DueDate, true);

                // Check to see if the Journal entries for this Work Item have been loaded
                if (_model.SelectedWorkItem.Meta.AreJournalItemsLoaded == false)
                {
                    _controller.LoadJournalsFromDB(_model.SelectedWorkItem);
                    JournalEntryList.ItemsSource = _model.SelectedWorkItem.Journals;
                }

                _model.IsBindingLoading = false;
            }

            else if (args.Action == AppAction.WORK_ITEM_ADDED)
            {
                SaveButton.Background = Brushes.SteelBlue;
                SaveButton.Content = "Save";
            }

            else if (args.Action == AppAction.WORK_ITEM_STATUS_CHANGED)
            {
                // Check to see if the Status has been set to a 'completed' type. If so, disable the progress bar.
                bool isCompletedStatus = false;
                foreach (WorkItemStatus wis in _controller.GetWorkItemStatuses(false))
                {
                    if (wis.Status.Equals(args.CurrentSelection.Status))
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
            }

            if (args.Action == AppAction.SET_APPLICATION_MODE)
            {
                if (_model.GetApplicationMode() == DataEntryMode.ADD_WORK_ITEM)
                {
                    // Make the 'New Work Item' button unavailable.
                    // TODO: Not working
                    NewWorkItemButton.IsEnabled = false;

                    GraphicalTaskView.Background = Brushes.WhiteSmoke;

                    HighlightButton(SaveButton, Brushes.Green);
                    HighlightButton(CancelButton, Brushes.Red);
                    SaveButton.Content = "Create Work Item";
                }
                else if (_model.GetApplicationMode() == DataEntryMode.EDIT_WORK_ITEM)
                {
                    // Make the 'New Work Item' button available.
                    NewWorkItemButton.IsEnabled = true;
                    GraphicalTaskView.Background = Brushes.White;
                    SaveButton.Content = "Save";
                }

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
            if (_model.GetApplicationMode() == DataEntryMode.ADD_WORK_ITEM)
            {
                _controller.InsertDBWorkItem(selectedWorkItem);
                _model.AddWorkItem(selectedWorkItem, true, true);
                _model.SetApplicationMode(DataEntryMode.EDIT_WORK_ITEM);
            }
        }

        /// <summary>
        /// Checks to see what aspects of a WorkItem might need updating and updates them accordingly to the database.
        /// </summary>
        private void UpdateWorkItemDBAsRequired()
        {
            WorkItem selectedWorkItem = _model.SelectedWorkItem;

            // Ensure that a WorkItem has been selected.
            if ((selectedWorkItem != null) && (_model.GetApplicationMode() == DataEntryMode.EDIT_WORK_ITEM))
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

                if (ddw.WasWindowSubmitted)
                {
                    if (ddw.NewDateTime.Equals(currentDueDate))
                    {
                        // Do nothing
                    }
                    else
                    {
                        // If the DueDate (from database) has been set within x mins of now, UPDATE the record instead of INSERTING it.
                        int minutesSinceLastSet = DateTime.Now.Subtract(_model.SelectedWorkItem.Meta.DueDateUpdateDateTime).Minutes;
                        if (minutesSinceLastSet < Convert.ToInt32(_controller.GetMWTModel().GetAppSetting(SettingName.DUE_DATE_SET_WINDOW_MINUTES)))
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
        }

        /// <summary>
        /// Cancel the creation of a new WorkItem.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _model.SelectedWorkItem = null;
            _model.SetApplicationMode(DataEntryMode.EDIT_WORK_ITEM);
        }

        private void AddJournalButton_Click(object sender, RoutedEventArgs e)
        {
            var journalEntry = new JournalDialog();
            journalEntry.Owner = this;
            journalEntry.ShowDialog();
        }
    }
}
