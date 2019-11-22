using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MyWorkTracker.Code
{
    public class MWTModel : INotifyPropertyChanged
    {
        public const string DatabaseFile = @"\Data\MyWorkTracker.db";
        public delegate void AppEventHandler(object obj, AppEventArgs e);
        public event AppEventHandler appEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This collection is the key data of the application; a list of active WorkItems.
        /// </summary>
        private ObservableCollection<WorkItem> _activeWorkItems = new ObservableCollection<WorkItem>();

        /// <summary>
        /// This collection is the key data of the application; a list of closed WorkItems.
        /// </summary>
        private ObservableCollection<WorkItem> _closedWorkItems = new ObservableCollection<WorkItem>();

        /// <summary>
        /// This variable keeps track of all of the Statuses that a WorkItem can take.
        /// </summary>
        private List<WorkItemStatus> _statuses = new List<WorkItemStatus>();

        /// <summary>
        /// Collection of application settings.
        /// </summary>
        private Dictionary<PreferenceName, Preference> _appSettings = new Dictionary<PreferenceName, Preference>();

        /// <summary>
        /// The selected work item.
        /// </summary>
        private WorkItem _selectedWorkItem = null;
        public WorkItem SelectedWorkItem
        {
            get { return _selectedWorkItem; }
            set { _selectedWorkItem = value; OnPropertyChanged(""); }
        }

        public ObservableCollection<NotebookTopic> NotebookTopics = new ObservableCollection<NotebookTopic>();

        /// <summary>
        /// Clear all WorkItems from both the active and closed collections (in memory).
        /// </summary>
        public void ClearAllWorkItems()
        {
            _activeWorkItems.Clear();
            _closedWorkItems.Clear();
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

        private JournalEntry _selectedJournalEntry = null;
        public JournalEntry SelectedJournalEntry
        {
            get { return _selectedJournalEntry; }
            set { _selectedJournalEntry = value; OnPropertyChanged(""); }
        }

        public bool IsJournalEntrySelected
        {
            get
            {
                bool rValue = false;

                if (_selectedJournalEntry != null)
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
        private DataEntryMode _appMode = DataEntryMode.NOT_SET;
        /// <summary>
        /// Returns whether or not the application is currently in 'add' mode.
        /// </summary>
        /// <returns></returns>
        public bool IsApplicationInAddMode
        {
            get
            {
                bool rValue = false;
                if (_appMode == DataEntryMode.ADD)
                    rValue = true;
                return rValue;
            }
        }

        public bool IsApplicationInEditMode
        {
            get
            {
                bool rValue = false;
                if (_appMode == DataEntryMode.EDIT)
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

        public Dictionary<PreferenceName, Preference> GetAppPreferenceCollection()
        {
            return _appSettings;
        }

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
            if (workItem.IsConsideredActive)
                _activeWorkItems.Add(workItem);
            else
                _closedWorkItems.Add(workItem);

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

        public void RemoveWorkItem(WorkItem workItem)
        {
            if (workItem.IsConsideredActive)
                _activeWorkItems.Remove(workItem);
            else
                _closedWorkItems.Remove(workItem);

        }

        public ObservableCollection<WorkItem> GetWorkItems()
        {
            return _activeWorkItems;
        }
        
        // Maybe it's failing because once you change their state they are no longer the same???
        internal void SwapList(bool activeToClosed, WorkItem wi)
        {
            if (activeToClosed)
            {
                _activeWorkItems.Remove(wi);
                _closedWorkItems.Add(wi);

            } 
            else
            {
                _closedWorkItems.Remove(wi);
                _activeWorkItems.Add(wi);
            }
        }

        public ObservableCollection<WorkItem> GetActiveWorkItems()
        {
            return _activeWorkItems;
        }

        public ObservableCollection<WorkItem> GetClosedWorkItems()
        {
            return _closedWorkItems;
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
        public IEnumerable<WorkItemStatus> GetWorkItemStatuses()
        {
            return _statuses;
        }

        /// <summary>
        /// Add the Application Setting to the Setting collection (in cache).
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="setting"></param>
        /// <returns>Returns true if added, false if already in the collection.</returns>
        public bool AddAppSetting(PreferenceName settingName, Preference setting)
        {
            bool rValue = false;
            if (_appSettings.ContainsKey(settingName) == false)
            {
                _appSettings.Add(settingName, setting);
                rValue = true;
            }
            return rValue;
        }

        public void ClearApplicationPreferences()
        {
            _appSettings.Clear();
        }

        /// <summary>
        /// Fire a notification that a new Work Item is being created (but has not yet been completed).
        /// </summary>
        /// <param name="workItem"></param>
        public void FireCreateNewWorkItem(WorkItem workItem)
        {
            IsBindingLoading = true;

            if (_selectedWorkItem != null)
            {
                _previouslySelectedWorkItem = _selectedWorkItem;
            }
            SelectedWorkItem = workItem;

            var eventArgs = new AppEventArgs(AppAction.CREATE_WORK_ITEM, workItem);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Fire a notification that a Work Item Status has changed for a WorkItem.
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="newStatus"></param>
        /// <param name="oldStatus"></param>
        public void FireWorkItemStatusChange(WorkItem wi, WorkItemStatus newStatus, WorkItemStatus oldStatus)
        {
            var eventArgs = new AppEventArgs(AppAction.WORK_ITEM_STATUS_CHANGED, wi, newStatus, oldStatus);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Fire a notification that a Work Item has been requested to be deleted./
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="logicalDelete"></param>
        public void FireWorkItemDelete(WorkItem wi, bool logicalDelete)
        {
            AppEventArgs eventArgs = null;
            if (logicalDelete)
                eventArgs = new AppEventArgs(AppAction.WORK_ITEM_DELETE_LOGICAL, wi);
            else
                eventArgs = new AppEventArgs(AppAction.WORK_ITEM_DELETE_PHYSICAL, wi);

            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Fire a notification that the due date has changed for a WorkItem.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="newDateTime">The new DateTime that has been set.</param>
        public void FireDueDateChanged(WorkItem workItem, DateTime newDateTime)
        {
            var eventArgs = new AppEventArgs(AppAction.DUE_DATE_CHANGED, workItem, newDateTime);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Sets the selected Work Item in the model, firing notification of the selection.
        /// </summary>
        /// <param name="wt"></param>
        public void SetSelectedWorkItem(WorkItem wt)
        {
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
        /// <param name="newMode"></param>
        public void SetApplicationMode(DataEntryMode newMode)
        {
            DataEntryMode oldMode = _appMode;
            _appMode = newMode;
            OnPropertyChanged("");
            var eventArgs = new AppEventArgs(AppAction.SET_APPLICATION_MODE, oldMode, newMode);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Return the mode that the application is currently in.
        /// </summary>
        /// <returns></returns>
        public DataEntryMode GetApplicationMode()
        {
            return _appMode;
        }

        /// <summary>
        /// Fire a notification that a Setting has been requested to change value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="newValue"></param>
        public void FireUpdateAppPreference(PreferenceName name, string newValue)
        {
            // PREFERENCE_CHANGED: PreferenceName, oldValue, newValue
            var eventArgs = new AppEventArgs(action:AppAction.PREFERENCE_CHANGED, object1:name, object2:_appSettings[name], object3:newValue);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Fire a notification that a data export is requested.
        /// </summary>
        public void FireDataExportRequest()
        {
            var eventArgs = new AppEventArgs(AppAction.DATA_EXPORT);
            appEvent?.Invoke(this, eventArgs);
        }

        public void FireDataImportRequest()
        {
            var eventArgs = new AppEventArgs(AppAction.DATA_IMPORT);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Fire a notification that a Journal Entry has been requested to be deleted.
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="je"></param>
        public void FireDeleteJournalEntry(WorkItem wi, JournalEntry je)
        {
            var eventArgs = new AppEventArgs(AppAction.JOURNAL_ENTRY_DELETED, wi, je);
            appEvent?.Invoke(this, eventArgs);
        }

        public void FireAddJournalEntry(WorkItem wi, JournalEntry je)
        {
            var eventArgs = new AppEventArgs(AppAction.JOURNAL_ENTRY_ADDED, wi, je);
            appEvent?.Invoke(this, eventArgs);
        }

        public void FireEditJournalEntry(WorkItem wi, JournalEntry oldEntry, JournalEntry newEntry)
        {
            var eventArgs = new AppEventArgs(AppAction.JOURNAL_ENTRY_EDITED, wi, oldEntry, newEntry);
            appEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Return an Application Preference from the collection (in cache), as a string value.
        /// </summary>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public string GetAppPreferenceValue(PreferenceName settingName)
        {
            _appSettings.TryGetValue(settingName, out Preference rValue);
            return rValue.Value;
        }


        /// <summary>
        /// Return an Application Preference from the collection (in cache), as a boolean value.
        /// </summary>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public bool GetAppPreferenceAsBooleanValue(PreferenceName settingName)
        {
            bool rValue = false;
            _appSettings.TryGetValue(settingName, out Preference stringValue);
            if (stringValue.Value.Equals("1"))
                rValue = true;

            return rValue;
        }


    }
}
