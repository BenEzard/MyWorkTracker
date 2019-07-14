namespace MyWorkTracker.Code
{
    public class AppEventArgs
    {
        public AppAction Action { get; set; }
        public WorkItem CurrentWorkItemSelection { get; set; }
        
        public Setting Setting { get; set; }

        public JournalEntry JournalEntry { get; set; }
        public JournalEntry JournalEntry2 { get; set; }

        public string StringValue { get; set; }

        public AppEventArgs(AppAction action, WorkItem current)
        {
            Action = action;
            CurrentWorkItemSelection = current;
        }

        public AppEventArgs(AppAction action, Setting setting, string newValue)
        {
            Action = action;
            Setting = setting;
            StringValue = newValue;
        }

        public AppEventArgs(AppAction action, WorkItem currentWorkItem, JournalEntry currentJournalEntry)
        {
            Action = action;
            CurrentWorkItemSelection = currentWorkItem;
            JournalEntry = currentJournalEntry;
        }

        public AppEventArgs(AppAction action, WorkItem currentWorkItem, JournalEntry oldEntry, JournalEntry newEntry)
        {
            Action = action;
            CurrentWorkItemSelection = currentWorkItem;
            JournalEntry = oldEntry;
            JournalEntry2 = newEntry;
        }
    }
}
