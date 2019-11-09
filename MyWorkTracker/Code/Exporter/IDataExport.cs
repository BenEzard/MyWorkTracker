using System.Collections.Generic;

namespace MyWorkTracker.Code
{
    interface IDataExport
    {
        void ExportToXML(string appName, string appVersion, Dictionary<ExportSetting, string> exportSettings);

        string ExportVersion();
    }

    public enum ExportSetting
    {
        DATABASE_CONNECTION,
        EXPORT_TO_LOCATION,
        EXPORT_PREFERENCES,
        EXPORT_WORK_ITEM_OPTION,
        EXPORT_DAYS_STALE,
        EXPORT_INCLUDE_DELETED,
        EXPORT_INCLUDE_LAST_STATUS_ONLY,
        EXPORT_INCLUDE_LAST_DUEDATE_ONLY,
    }
}
