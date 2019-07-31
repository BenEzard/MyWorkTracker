namespace MyWorkTracker.Code
{
    public enum AppAction
    {
        /* Application-level events */
        SET_APPLICATION_MODE,
        PREFERENCE_CHANGED,

        CREATE_WORK_ITEM, // Create a new WorkItem (but before it's been added anywhere)
        DUE_DATE_CHANGED,
        SELECT_WORK_ITEM,
        WORK_ITEM_ADDED,
        WORK_ITEM_STATUS_CHANGED,

        /* Journal Events */
        JOURNAL_ENTRY_ADDED,
        JOURNAL_ENTRY_EDITED,
        JOURNAL_ENTRY_DELETED,

    }
}
