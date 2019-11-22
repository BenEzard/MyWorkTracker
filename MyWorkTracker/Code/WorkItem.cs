using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyWorkTracker.Code
{
    public class WorkItem : BaseDBElement
    {
        public WorkItemDBMeta Meta { get; set; } = new WorkItemDBMeta();

        private string _title = "";
        private string _taskDescription = "";

        [Obsolete("Deprecate; to be removed, if LINQ can access _wiStatus members.")]
        private string _status = "";

        /// <summary>
        /// The WorkItemStatus appropriate to the WorkItem.
        /// (Note that this should NOT be created 'new', and should instead reference the collection held in the model).
        /// </summary>
        public WorkItemStatus workItemStatus { get; set; }

        public WorkItemStatusEntry WorkItemStatusEntry { get; set; }
        
        private DateTime _dueDateTime = DateTime.MinValue;

        /// <summary>
        /// Is this WorkItem considered active?
        /// This variable is a convenience variable so this object can be selected using LINQ.
        /// </summary>
        [Obsolete("Deprecate; to be removed, if LINQ can access _wiStatus members.")]
        private bool _isConsideredActive = false;

        public ObservableCollection<JournalEntry> Journals = new ObservableCollection<JournalEntry>();

        private bool _areChecklistsLoaded = false;
        /// <summary>
        /// Confirms if an attempt has been made to load the CheckList for the WorkItem.
        /// Note this can be true, and the collection can still be empty (if there are no CheckListItems for the WorkItem).
        /// </summary>
        public bool AreCheckListsLoaded
        {
            get { return _areChecklistsLoaded; }
            set { 
                _areChecklistsLoaded = true; 
                OnPropertyChanged(); 
            }
        }

        public ObservableCollection<CheckListItem> CheckListItems = new ObservableCollection<CheckListItem>();

        public WorkItem()
        {

        }

        /// <summary>
        /// Create a new WorkItem, assigning the specified WorkItemStatus.
        /// </summary>
        /// <param name="wis"></param>
        public WorkItem(WorkItemStatus wis)
        {
            workItemStatus = wis;
            WorkItemStatusEntry = new WorkItemStatusEntry(wis.WorkItemStatusID);
        }

        /// <summary>
        /// Create a new WorkItem with the specified values.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="status"></param>
        /// <param name="amountComplete"></param>
        /// <param name="createDateTime"></param>
        /// <param name="dueDateTime"></param>
        public WorkItem(string title, string description, string status, int amountComplete, DateTime createDateTime, DateTime dueDateTime)
        {
            Title = title;
            TaskDescription = description;
            Status = status;
            Completed = amountComplete;
            CreationDateTime = createDateTime;
            DueDate = dueDateTime;
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public string TaskDescription
        {
            get { return _taskDescription; }
            set
            {
                _taskDescription = value;
                OnPropertyChanged();
            }
        }

        public DateTime DueDate
        {
            get { return _dueDateTime; }
            set
            {
                _dueDateTime = value;
                OnPropertyChanged();
            }
        }

        public bool IsConsideredActive
        {
            get { return _isConsideredActive; }
            set
            {
                _isConsideredActive = value;
                OnPropertyChanged();
            }
        }

        
/*        public bool IsConsideredActive
        {
            get { return workItemStatus.IsConsideredActive; }
            set
            {
                if (workItemStatus != null)
                {
                    workItemStatus.IsConsideredActive = value;
                    OnPropertyChanged();
                }
            }
        }*/

        /// <summary>
        /// 
        /// </summary>
        public int Completed
        {
            get { return WorkItemStatusEntry.CompletionAmount; }
            set
            {
                if (WorkItemStatusEntry == null)
                    WorkItemStatusEntry = new WorkItemStatusEntry();
                WorkItemStatusEntry.CompletionAmount = value;
                OnPropertyChanged();
            }
        }

    }
}
