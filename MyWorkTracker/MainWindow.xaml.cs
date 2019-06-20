using MyWorkTracker.Code;
using System;
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

            // Set the application name and version.
            this.Title = $"{_model.GetAppSetting(SettingName.APPLICATION_NAME)} [v{_model.GetAppSetting(SettingName.APPLICATION_VERSION)}]";

            // TODO: Temp. Create a dummy record.
/*            var wi = new WorkItem("A test run", "This is a sample test", "Active", 50, DateTime.Now, DateTime.Now);
            _model.AddWorkItem(wi, true, true);
            _model.SetSelectedWorkItem(wi);
            DataContext = _model.GetSelectedWorkItem();*/

            SaveButton.IsEnabled = true;
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
                Console.WriteLine("*** A work item has been created");
                DataContext = args.CurrentSelection;
                _model.IsBindingLoading = false;
                SelectedTaskTitleField.Focus();

                _model.SetApplicationMode(ApplicationMode.ADD_WORK_ITEM);
                NewWorkItemButton.IsEnabled = true;
            }

            else if (args.Action == AppAction.SELECT_WORK_ITEM)
            {
                Console.WriteLine($"*** A work item has been selected: {args.CurrentSelection.Title}");

                // Set to the Status of the selected WorkItem
                WorkItemStatus.SelectedItem = _controller.GetWorkItemStatus(args.CurrentSelection.Status);

                DataContext = args.CurrentSelection;
                _model.IsBindingLoading = false;

            }

            else if (args.Action == AppAction.WORK_ITEM_ADDED)
            {
                SaveButton.Background = Brushes.SteelBlue;
                SaveButton.Content = "Save";
                SaveButton.IsEnabled = false;
            }

            if (args.Action == AppAction.SET_APPLICATION_MODE)
            {
                if (_model.GetApplicationMode() == ApplicationMode.ADD_WORK_ITEM)
                {
                    // Make the 'New Work Item' button unavailable.
                    // TODO: Not working
                    NewWorkItemButton.IsEnabled = false;

                    GraphicalTaskView.Background = Brushes.WhiteSmoke;

                    HighlightButton(SaveButton, Brushes.Green);
                    HighlightButton(CancelButton, Brushes.Red);
                    SaveButton.Content = "Create Work Item";
                }
                else if (_model.GetApplicationMode() == ApplicationMode.EDIT_WORK_ITEM)
                {
                    // Make the 'New Work Item' button available.
                    NewWorkItemButton.IsEnabled = true;
                    SaveButton.IsEnabled = true;
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

        private void DebugOut(object sender, RoutedEventArgs e)
        {
            var wi = _model.GetSelectedWorkItem();
            Console.WriteLine($"Title={wi.Title}");
            Console.WriteLine($"count of work items {_model.GetWorkItems().Count}");
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
        /// When the Save button is pressed, then either the Work Item needs to be INSERTED (if the application is in ADD_WORK_ITEM),
        /// or UPDATED if the WorkItem already exists.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButtonPressed(object sender, RoutedEventArgs e)
        {
            WorkItem selectedWorkItem = _model.GetSelectedWorkItem();

            // If the application is in 'add mode' then we want to insert a record.
            if (_model.GetApplicationMode() == ApplicationMode.ADD_WORK_ITEM)
            {
                _controller.InsertDBWorkItem(selectedWorkItem);
                _model.AddWorkItem(selectedWorkItem, true, true);
                _model.SetApplicationMode(ApplicationMode.EDIT_WORK_ITEM);
            }
            else if (_model.GetApplicationMode() == ApplicationMode.EDIT_WORK_ITEM)
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
            if (_model.IsApplicationInAddMode())
            {
                GraphicalTaskView.IsEnabled = false;
            }
            // If the application is in 'add mode' (ADD_WORK_ITEM) then don't allow any selections.
            else
            {
                GraphicalTaskView.IsEnabled = true;
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
            _model.GetSelectedWorkItem().Meta.WorkItemDBNeedsUpdate = true;
        }

        private void WorkItemChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _model.GetSelectedWorkItem().Meta.WorkItemDBNeedsUpdate = true;
        }

        private void WorkItemStatusChanged(object sender, SelectionChangedEventArgs e)
        {
            WorkItemStatus wis = (WorkItemStatus)WorkItemStatus.SelectedItem;
            _model.GetSelectedWorkItem().Status = wis.Status;
            _model.GetSelectedWorkItem().Meta.WorkItemStatusNeedsUpdate = true;
        }

        private void DueDateButtonSelected(object sender, RoutedEventArgs e)
        {
            var ddw = new DueDateWindow(_controller);
            ddw.Owner = this;
            ddw.Show();
        }
    }
}
