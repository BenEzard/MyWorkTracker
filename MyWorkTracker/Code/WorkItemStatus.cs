namespace MyWorkTracker.Code
{
    public class WorkItemStatus
    {
        public int WorkItemStatusID { get; } = -1;
        private string _statusLabel = "";

        public WorkItemStatus(int dbID, string label)
        {
            WorkItemStatusID = dbID;
            _statusLabel = label;
        }

        public override string ToString()
        {
            return _statusLabel;
        }

        public string Status
        {
            get { return _statusLabel; }
        }
    }
}
