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


        public bool IsCompleted
        {
            get
            {
                if (_completionDateTime.HasValue)
                    return true;
                else
                    return false;
            }
            set
            {
                _completionDateTime = DateTime.Now;
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

        /// <summary>
        /// Create a new CheckListItem
        /// </summary>
        /// <param name="wiclID"></param>
        /// <param name="task"></param>
        /// <param name="details"></param>
        /// <param name="dueDate"></param>
        /// <param name="completionDate"></param>
        /// <param name="sortOrder"></param>
        /// <param name="createDate"></param>
        /// <param name="modifyDate"></param>
        /// <param name="deleteDate"></param>
        public CheckListItem(int wiclID, string task, string details, int indent, DateTime? dueDate, DateTime? completionDate, int sortOrder, DateTime createDate, DateTime? modifyDate, DateTime? deleteDate)
        {
            DatabaseID = wiclID;
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

    }
}
