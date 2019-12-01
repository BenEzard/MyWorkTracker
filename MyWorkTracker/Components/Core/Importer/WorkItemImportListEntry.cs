using System;

namespace MyWorkTracker
{
    public class WorkItemImportListEntry
    {
        public int WorkItemID {get; set;}
        public string Title { get; set; }

        public DateTime CreationDate { get; set; }

        public string Status { get; set; }

        public WorkItemImportListEntry(int workItemID, string title, DateTime create, string status)
        {
            WorkItemID = workItemID;
            Title = title;
            CreationDate = create;
            Status = status;
        }
    }
}