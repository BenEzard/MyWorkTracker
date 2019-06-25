using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MyWorkTracker.Code
{
    public class MWTModel : INotifyPropertyChanged
    {
        public const string DatabaseFile = @"D:\Development\Repos\MyWorkTracker\MyWorkTracker\Data\MyWorkTracker.db";
        //public const string DatabaseFile = @"|DataDirectory|\Data\MyWorkTracker.db";
        public delegate void AppEventHandler(object obj, AppEventArgs e);
        public event AppEventHandler appEvent;

        /// <summary>
        /// This collection is the key data of the application; a list of WorkItems.
        /// </summary>
        private ObservableCollection<WorkItem> _workItems = new ObservableCollection<WorkItem>();

        /// <summary>
        /// This variable keeps track of all of the Statuses that a WorkItem can take.
        /// </summary>
        private List<WorkItemStatus> _statuses = new List<WorkItemStatus>();

        /// <summary>
        /// Collection of application settings.
        /// </summary>
        private Dictionary<SettingName, string> _appSettings = new Dictionary<SettingName, string>();

        /// <summary>
        /// The selected work item.
        /// </summary>
        private WorkItem _selectedWorkItem = null;
        public WorkItem SelectedWorkItem
        {
            get { return _selectedWorkItem; }
            set { _selectedWorkItem = value; OnPropertyChanged(""); }
        }

        public bool IsWorkItemSelected
        {
            get {
                bool rValue = false;
                if (_selectedWorkItem != null)
                    rValue = true;
                return rValue;
            }
        }

        /// <summary>
        /// Keeps track of the previously selected work item.
        /// </summary>
        private WorkItem _previouslySelectedWorkItem = null;

        /// <summary>
        /// Stores the current mode of the application.
        /// In ADD mode, the top portion of the window (Work Item Selection area) is disabled, focusing the user on completing
        /// the current in-creation WorkItem.
        /// </summary>
        private ApplicationMode _appMode = ApplicationMode.EDIT_WORK_ITEM;
        /// <summary>
        /// Returns whether or not the application is currently in 'add' mode.
        /// </summary>
        /// <returns></returns>
        public bool IsApplicationInAddMode
        {
            get
            {
                bool rValue = false;
                if (_appMode == ApplicationMode.ADD_WORK_ITEM)
                    rValue = true;
                return rValue;
            }
        }


        /// <summary>
        /// This flag is needed to determine if changes to controls are due to the data-binding, or if by user entry.
        /// Where IsBindingLoading=true the changes are a result of data-binding.
        /// </summary>
        public bool IsBindingLoading { get; set; }



        public MWTModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Add a Work Task to the collection.
        /// Optionally, can notify any listeners with the TaskAction of CREATED. The new object is passed through the first parameter.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="notifyListeners"></param>
        /// /// <param name="select">Should the WorkItem be selected?</param>
        public void AddWorkItem(WorkItem workItem, bool notifyListeners, bool select)
        {
            _workItems.Add(workItem);

            if (notifyListeners)
            {
                AppEventArgs eventArgs = new AppEventArgs(AppAction.WORK_ITEM_ADDED, workItem);
                appEvent?.Invoke(this, eventArgs);
            }

            if (select)
            {
                SetSelectedWorkItem(workItem);
            }
        }

        public ObservableCollection<WorkItem> GetWorkItems()
        {
            return _workItems;
        }

        /// <summary>
        /// Add a WorkItemStatus to the model.
        /// </summary>
        /// <param name="wis"></param>
        public void AddWorkItemStatus(WorkItemStatus wis)
        {
            if (_statuses.Contains(wis) == false) {
                _statuses.Add(wis);
            }
        }

        /// <summary>
        /// Get the list of potential WorkItem Statuses.
        /// </summary>
        /// <returns></returns>
        public List<WorkItemStatus> GetWorkItemStatuses()
        {
            return _statuses;
        }

        /// <summary>
        /// Add the Application Setting to the Setting collection (in cache).
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="settingValue"></param>
        /// <returns>Returns true if added, false if already in the collection.</returns>
        public bool AddAppSetting(SettingName settingName, string settingValue)
        {
            bool rValue = false;

            if (_appSettings.ContainsKey(settingName) == false)
            {
                _appSettings.Add(settingName, settingValue);
                rValue = true;
            }
            return rValue;
        }

        public void FireCreateNewWorkItem(WorkItem wi)
        {
            IsBindingLoading = true;

            if (_selectedWorkItem != null)
            {
                _previouslySelectedWorkItem = _selectedWorkItem;
            }
            SelectedWorkItem = wi;

            var eventArgs = new AppEventArgs(AppAction.CREATE_NEW_WORK_ITEM, wi);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Fire a notification that the due date has changed for a WorkItem.
        /// </summary>
        /// <param name="wi"></param>
        public void FireDueDateChanged(WorkItem wi)
        {
            var eventArgs = new AppEventArgs(AppAction.DUE_DATE_CHANGED, wi);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Sets the selected Work Item in the model, firing notification of the selection.
        /// </summary>
        /// <param name="wt"></param>
        public void SetSelectedWorkItem(WorkItem wt)
        {
            Console.WriteLine($" A new work item has been selected: {wt.Title}");
            IsBindingLoading = true;

            if (_selectedWorkItem != null)
            {
                _previouslySelectedWorkItem = _selectedWorkItem;
            }

            SelectedWorkItem = wt;
            var eventArgs = new AppEventArgs(AppAction.SELECT_WORK_ITEM, wt);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Set the mode the Application is in.
        /// </summary>
        /// <param name="mode"></param>
        public void SetApplicationMode(ApplicationMode mode)
        {
            _appMode = mode;
            OnPropertyChanged("");
            var eventArgs = new AppEventArgs(AppAction.SET_APPLICATION_MODE, null);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Return the mode that the application is currently in.
        /// </summary>
        /// <returns></returns>
        public ApplicationMode GetApplicationMode()
        {
            return _appMode;
        }

        /// <summary>
        /// Return an Application Setting from the collection (in cache).
        /// </summary>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public string GetAppSetting(SettingName settingName)
        {
            _appSettings.TryGetValue(settingName, out string rValue);
            return rValue;
        }


    }
}
