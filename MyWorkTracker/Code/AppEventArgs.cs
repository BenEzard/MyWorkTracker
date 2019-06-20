namespace MyWorkTracker.Code
{
    public class AppEventArgs
    {
        public AppAction Action { get; set; }
        public WorkItem CurrentSelection { get; set; }

        public AppEventArgs(AppAction action, WorkItem current)
        {
            Action = action;
            CurrentSelection = current;
        }
    }
}
