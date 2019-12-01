using MyWorkTracker.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorkTracker
{
    public class CheckListItem : BaseDBElement
    {
        public int WorkItemID { get; set; }

        public int SortOrder { get; set; }

        public int Indent { get; set; }

        private DateTime? _completionDateTime;
        public DateTime? CompletionDateTime
        {
            get { return _completionDateTime; }
            set { 
                _completionDateTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _dueDateTime;
        public DateTime? DueDateTime
        {
            get { return _dueDateTime; }
            set
            {
                _dueDateTime = value;
                OnPropertyChanged();
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
            set
            {
                _isCompleted = value;
                if (value)
                {
                    _completionDateTime = DateTime.Now;
                }
                else
                {
                    _completionDateTime = null;
                }
                OnPropertyChanged();
            }
        }

        private string _task = null;
        public String Task
        {
            get { return _task; }
            set { _task = value; OnPropertyChanged(); }
        }

        private string _details = null;
        public String Details
        {
            get { return _details; }
            set { _details = value; OnPropertyChanged(); }
        }

        public CheckListItem() { }

        /// <summary>
        /// Create a new CheckListItem
        /// </summary>
        /// <param name="wiclID"></param>
        /// <param name="workItemID"></param>
        /// <param name="task"></param>
        /// <param name="details"></param>
        /// <param name="dueDate"></param>
        /// <param name="completionDate"></param>
        /// <param name="sortOrder"></param>
        /// <param name="createDate"></param>
        /// <param name="modifyDate"></param>
        /// <param name="deleteDate"></param>
        public CheckListItem(int wiclID, int workItemID, string task, string details, int indent, DateTime? dueDate, DateTime? completionDate, int sortOrder, DateTime createDate, DateTime? modifyDate, DateTime? deleteDate)
        {
            DatabaseID = wiclID;
            WorkItemID = workItemID;
            _task = task;
            _details = details;
            Indent = indent;
            _dueDateTime = dueDate;
            _completionDateTime = completionDate;
            SortOrder = sortOrder;
            CreationDateTime = createDate;
            ModificationDateTime = modifyDate;
            DeletionDateTime = deleteDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workItemID"></param>
        /// <param name="task"></param>
        /// <param name="details"></param>
        /// <param name="dueDate"></param>
        public CheckListItem(int workItemID, string task, string details, DateTime? dueDate)
        {
            WorkItemID = workItemID;
            _task = task;
            _details = details;
            _dueDateTime = dueDate;
        }


    }
}
