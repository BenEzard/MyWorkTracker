using System;

namespace MyWorkTracker.Code
{
    public class WorkItemStatus
    {
        public int WorkItemStatusID { get; set; } = -1;

        private string _statusLabel = "";
        public string Status
        {
            get { return _statusLabel; }
            set { _statusLabel = value; }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbID"></param>
        /// <param name="label"></param>
        /// <param name="isActive"></param>
        /// <param name="isDefault"></param>
        /// <param name="deletionDate"></param>
        public WorkItemStatus(int dbID, string label, bool isActive, bool isDefault, DateTime deletionDate)
        {
            WorkItemStatusID = dbID;
            _statusLabel = label;
            IsConsideredActive = isActive;
            IsDefault = isDefault;
            DeletionDate = deletionDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbID"></param>
        /// <param name="label"></param>
        /// <param name="isActive"></param>
        /// <param name="isDefault"></param>
        public WorkItemStatus(int dbID, string label, bool isActive, bool isDefault)
        {
            WorkItemStatusID = dbID;
            _statusLabel = label;
            IsConsideredActive = isActive;
            IsDefault = isDefault;
        }

        public override string ToString()
        {
            return _statusLabel;
        }

        /// <summary>
        /// Checks to see if the object is materially the same.
        /// (Have not overridden Equals() because I might want that functionality untainted).
        /// </summary>
        /// <param name="wis"></param>
        /// <returns></returns>
        public bool IsSame(WorkItemStatus wis)
        {
            bool rValue = true;

            if ((this.Status.Equals(wis.Status) == false) ||
                    (this.IsConsideredActive != wis.IsConsideredActive) ||
                    (this.IsDefault != wis.IsDefault) ||
                    (this.DeletionDate.Equals(wis.DeletionDate) == false))
                rValue = false;

            return rValue;
        }
    }
}
