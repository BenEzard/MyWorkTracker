using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorkTracker.Code
{
    public class WorkItemStatusEntry
    {
        public int WorkItemStatusID { get; set; }
        public int WorkItemID { get; }
        public int StatusID { get; }
        public DateTime CreationDateTime {get; }

        public DateTime DeletionDateTime { get; }

        public WorkItemStatusEntry(int workItemID, int statusID, DateTime creationDateTime, DateTime? deletionDate)
        {
            WorkItemID = workItemID;
            StatusID = statusID;
            CreationDateTime = creationDateTime;
            if (deletionDate.HasValue)
                DeletionDateTime = deletionDate.Value;
        }
    }
}
