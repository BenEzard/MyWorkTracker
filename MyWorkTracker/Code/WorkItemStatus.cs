using System;

namespace MyWorkTracker.Code
{
    public class WorkItemStatus
    {
        public int WorkItemStatusID { get; } = -1;

        private string _statusLabel = "";
        public string Status
        {
            get { return _statusLabel; }
        }

        /// <summary>
        /// Is this Status considered to be 'active'? ('closed' is the other option)
        /// </summary>
        public bool IsConsideredActive { get; set; }

        /// <summary>
        /// Is this Status the default?
        /// Only 1 record should be default 'active' and 1 record default 'closed'
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Record a DateTime of when this Status was set to logically deleted.
        /// </summary>
        public DateTime DeletionDate { get; set; }

        public WorkItemStatus(int dbID, string label)
        {
            WorkItemStatusID = dbID;
            _statusLabel = label;
        }

        public override string ToString()
        {
            return _statusLabel;
        }


    }
}
