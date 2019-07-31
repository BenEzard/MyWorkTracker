namespace MyWorkTracker.Code
{
    public class AppEventArgs
    {
        public AppAction Action { get; set; }
        public WorkItem CurrentWorkItemSelection { get; set; } = null;

        public object Object1 { get; set; } = null;
        public object Object2 { get; set; } = null;
        public object Object3 { get; set; } = null;

        public AppEventArgs(AppAction action, WorkItem current)
        {
            Action = action;
            CurrentWorkItemSelection = current;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="current"></param>
        /// <param name="object1"></param>
        public AppEventArgs(AppAction action, WorkItem current, object object1)
        {
            Action = action;
            CurrentWorkItemSelection = current;
            Object1 = object1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="current"></param>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        public AppEventArgs(AppAction action, WorkItem current, object object1, object object2)
        {
            Action = action;
            CurrentWorkItemSelection = current;
            Object1 = object1;
            Object2 = object2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        public AppEventArgs(AppAction action, object object1, object object2)
        {
            Action = action;
            Object1 = object1;
            Object2 = object2;
        }

        public AppEventArgs(AppAction action, object object1, object object2, object object3)
        {
            Action = action;
            Object1 = object1;
            Object2 = object2;
            Object3 = object3;
        }

    }
}
