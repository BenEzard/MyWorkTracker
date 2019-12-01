using System.Collections.Generic;
using System.Xml.Linq;

namespace MyWorkTracker.Code
{
    interface IDataImport
    {
        void ImportData(MWTController controller, string importFromVersion, Dictionary<PreferenceName, string> loadPreferences, List<WorkItemStatus> importStatuses, XDocument xml);
    }
}
