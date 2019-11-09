﻿namespace MyWorkTracker.Code
{
    public enum PreferenceName
    {
        APPLICATION_NAME,
        APPLICATION_VERSION,
        DEFAULT_WORKITEM_LENGTH_DAYS,
        DEFAULT_WORKITEM_COB_HOURS,
        DEFAULT_WORKITEM_COB_MINS,
        DELETE_OPTION,
        DUE_DATE_SET_WINDOW_MINUTES,
        DUE_DATE_CAN_BE_WEEKENDS,
        CONFIRM_JOURNAL_DELETION,
        APPLICATION_WINDOW_COORDS,
        SAVE_WINDOW_COORDS_ON_EXIT,
        LOAD_STALE_DAYS, /* Load this number of days-old inactive Work Items */
        STATUS_ACTIVE_TO_COMPLETE_PCN,
        JOURNAL_ORDERING,

        DATA_EXPORT_AUTOMATICALLY,
        DATA_EXPORT_COPY_LOCATION,
        DATA_EXPORT_DAYS_STALE,
        DATA_EXPORT_DAYS_STALE_DEFAULT,
        DATA_EXPORT_DUEDATE_SELECTION,
        DATA_EXPORT_INCLUDE_DELETED,
        DATA_EXPORT_INCLUDE_PREFERENCES,
        DATA_EXPORT_LAST_DONE,
        DATA_EXPORT_LAST_DIRECTORY,
        DATA_IMPORT_LAST_DIRECTORY,
        DATA_EXPORT_PERIOD_DAYS,
        DATA_EXPORT_SAVE_TO_LOCATION,
        DATA_EXPORT_SAME_DAY_OVERWRITE,
        DATA_EXPORT_STATUS_SELECTION,
        DATA_EXPORT_WORKITEM_SELECTION,
        DATA_EXPORT_AVAILABLE_VERSIONS,
        DATA_EXPORT_DEFAULT_VERSION,
    }
}
