namespace MyWorkTracker.Code
{
    public enum PreferenceName
    {
        APPLICATION_NAME,
        APPLICATION_VERSION,
        DEFAULT_WORKITEM_LENGTH_DAYS,
        DEFAULT_WORKITEM_COB_HOURS,
        DEFAULT_WORKITEM_COB_MINS,
        DUE_DATE_SET_WINDOW_MINUTES,
        DUE_DATE_CAN_BE_WEEKENDS,
        CONFIRM_JOURNAL_DELETION,
        APPLICATION_WINDOW_COORDS,
        SAVE_WINDOW_COORDS_ON_EXIT,
        LOAD_STALE_DAYS, /* Load this number of days-old inactive Work Items */
    }
}
