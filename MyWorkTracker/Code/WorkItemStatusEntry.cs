using System;

namespace MyWorkTracker.Code
{
    public class WorkItemStatusEntry
    {
        public int WorkItemStatusEntryID { get; set; } = -1;
        public int WorkItemID { get; set; } = -1;
        public int StatusID { get; set; } = -1;

        /// <summary>
        /// Records the amount (0-100) that this WorkItem has been completed.
        /// </summary>
        public int CompletionAmount { get; set; } = 0;

        public DateTime? CreationDateTime { get; set; }

        public DateTime? ModificationDateTime { get; set; }

        public DateTime DeletionDateTime { get; }

        public WorkItemStatusEntry(int statusID)
        {
            StatusID = statusID;
        }

        /// <summary>
        /// Create a new WorkItemStatusEntry object.
        /// </summary>
        /// <param name="wiID"></param>
        /// <param name="statusID"></param>
        /// <param name="completionAmount"></param>
        /// <param name="creationDateTime"></param>
        public WorkItemStatusEntry(int wiID, int statusID, int completionAmount, DateTime? creationDateTime)
        {
            WorkItemID = wiID;
            StatusID = statusID;
            CompletionAmount = completionAmount;
            CreationDateTime = creationDateTime;
        }

        /// <summary>
        /// Create a new WorkItemStatusEntry object.
        /// </summary>
        /// <param name="wiseID"></param>
        /// <param name="wiID"></param>
        /// <param name="statusID"></param>
        /// <param name="completionAmount"></param>
        /// <param name="creationDateTime"></param>
        /// <param name="modificationDateTime"></param>
        public WorkItemStatusEntry(int wiseID, int wiID, int statusID, int completionAmount, DateTime? creationDateTime, DateTime? modificationDateTime)
        {
            WorkItemStatusEntryID = wiseID;
            WorkItemID = wiID;
            StatusID = statusID;
            CompletionAmount = completionAmount;
            CreationDateTime = creationDateTime;
            ModificationDateTime = modificationDateTime;
        }

        /// <summary>
        /// Identifies whether or not a record for this WorkItemStatusEntry exists yet.
        /// </summary>
        public bool RecordExists {
            get {
                if (WorkItemStatusEntryID == -1)
                    return false;
                else
                    return true;
            }
        }

        public WorkItemStatusEntry(int workItemID, int statusID, DateTime creationDateTime, DateTime? deletionDate)
        {
            WorkItemID = workItemID;
            StatusID = statusID;
            CompletionAmount = 0;
            CreationDateTime = creationDateTime;
            if (deletionDate.HasValue)
                DeletionDateTime = deletionDate.Value;
        }

        public WorkItemStatusEntry(int workItemID, int statusID, int completionAmount, DateTime creationDateTime, DateTime? deletionDate)
        {
            WorkItemID = workItemID;
            StatusID = statusID;
            CompletionAmount = completionAmount;
            CreationDateTime = creationDateTime;
            if (deletionDate.HasValue)
                DeletionDateTime = deletionDate.Value;
        }
    }
}
