using System;

namespace MyWorkTracker.Code
{
    public class WorkItemDBMeta
    {
        public int WorkItem_ID { get; set; }
        public int DueDate_ID { get; set; }
        public DateTime DueDateUpdateDateTime { get; set; }

        /// <summary>
        /// Signifies whether or not the WorkItem database tables needs to be updated.
        /// </summary>
        public bool WorkItemDBNeedsUpdate { get; set; } = false;

        /// <summary>
        /// Signifies whether or not the WorkItemStatus table needs to be updated.
        /// </summary>
        public bool WorkItemStatusNeedsUpdate { get; set; } = false;

        /// <summary>
        /// Signifies whether or not the Journal items for this Work Item have been loaded.
        /// (They are lazy-loaded only as required).
        /// </summary>
        public bool AreJournalItemsLoaded { get; set; } = false;

        public bool DBUpdateNeeded()
        {
            bool rValue = false;

            if ((WorkItemDBNeedsUpdate) || (WorkItemStatusNeedsUpdate))
            {
                rValue = true;
            }

            return rValue;
        }

        public WorkItemDBMeta() { }

    }
}
