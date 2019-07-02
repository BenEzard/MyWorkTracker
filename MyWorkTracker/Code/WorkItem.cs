using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyWorkTracker.Code
{
    public class WorkItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public WorkItemDBMeta Meta { get; set; } = new WorkItemDBMeta();

        private string _title = "";
        private string _taskDescription = "";
        private string _status = "";
        private int _amountComplete = 0;
        public DateTime CreateDateTime { get; set; } = DateTime.Now;
        private DateTime _dueDateTime;

        public List<JournalEntry> Journals = new List<JournalEntry>();

        public WorkItem()
        {

        }

        /// <summary>
        /// Create a new WorkItem with the specified values.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// /// <param name="status"></param>
        /// <param name="amountComplete"></param>
        /// <param name="createDateTime"></param>
        /// <param name="dueDateTime"></param>
        public WorkItem(string title, string description, string status, int amountComplete, DateTime createDateTime, DateTime dueDateTime)
        {
            Title = title;
            TaskDescription = description;
            Status = status;
            Completed = amountComplete;
            CreateDateTime = createDateTime;
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

        public int Completed
        {
            get { return _amountComplete; }
            set
            {
                _amountComplete = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
