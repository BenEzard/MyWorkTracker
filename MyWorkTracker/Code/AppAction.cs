namespace MyWorkTracker.Code
{
    public enum AppAction
    {
        CREATE_NEW_WORK_ITEM, // Create a new WorkItem (but before it's been added anywhere)
        SELECT_WORK_ITEM,
        SET_APPLICATION_MODE,
        WORK_ITEM_ADDED,
        DUE_DATE_CHANGED,
        WORK_ITEM_STATUS_CHANGED,
    }
}
