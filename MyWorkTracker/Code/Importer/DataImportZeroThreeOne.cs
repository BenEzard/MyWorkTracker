using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyWorkTracker.Code
{
    class DataImportZeroThreeOne : IDataImport
    {
        public void ImportData(MWTController controller, string importFromVersion, Dictionary<PreferenceName, string> loadPreferences, List<WorkItemStatus> importStatuses, XDocument xml)
        {
            MWTModel _model = controller.GetMWTModel();

            // Import any preference changes. 
            // (Note that the passing method provides an empty collection if we don't want this to happen).
            if (loadPreferences.Count > 0)
            {
                foreach (PreferenceName key in loadPreferences.Keys)
                {
                    if (_model.GetAppPreferenceValue(key).Equals(loadPreferences[key]) == false)
                        controller.UpdateAppPreference(key, loadPreferences[key]);
                }
            }

            // Import any WorkItemStatus requirements. 
            foreach (WorkItemStatus wis in importStatuses)
            {
                WorkItemStatus currentWIS = controller.GetWorkItemStatus(wis.Status);
                if (currentWIS != null)
                {
                    // If the two Statuses (system and import) exists, but are not the same...
                    if (currentWIS.IsSame(wis) == false)
                    {
                        wis.Status += " (2)";
                        wis.WorkItemStatusID = controller.InsertDBWorkItemStatus(wis);
                    }
                    else
                    {
                        // The Status exists, and is the same as the existing version; set the dbID to be the same as the existing one.
                        wis.WorkItemStatusID = controller.GetWorkItemStatus(wis.Status).WorkItemStatusID;
                    }
                }
                else
                { // If the Status doesn't exist, then we want to add it.
                    wis.WorkItemStatusID = controller.InsertDBWorkItemStatus(wis);
                }
            }

            // Import the Work Items.
            var query1 = from element in xml.Descendants("WorkItem")
                            select element;

            foreach (var el3 in query1)
            {
                int importedWorkID = Int32.Parse(el3.Attribute("WorkItem_ID").Value);
                WorkItem wi = new WorkItem
                {
                    Title = el3.Element("Title").Value,
                    TaskDescription = el3.Element("Description").Value,
                    CreationDateTime = DateTime.Parse(el3.Element("CreationDateTime").Value)
                };

                if ((el3.Element("DeletionDateTime").Value != null) && (el3.Element("DeletionDateTime").Value.Equals("") == false))
                    wi.DeletionDateTime = DateTime.Parse(el3.Element("DeletionDateTime").Value);

                wi.Meta.WorkItem_ID = controller.InsertDBWorkItem(wi, false);

                // Work Item Status
                var query2 = from element in el3.Descendants("StatusChange")
                                where Int32.Parse(el3.Attribute("WorkItem_ID").Value) == importedWorkID
                                select element;
                foreach (var el2 in query2)
                {
                    int statusID = Int32.Parse(el2.Element("Status_ID").Value);
                    int completionAmount = Int32.Parse(el2.Element("CompletionAmount").Value);
                    var wiseCreateDateTime = DateTime.Parse(el2.Element("CreationDateTime").Value);

                    DateTime? ModificationDateTime = null;
                    if ((el2.Element("ModificationDateTime").Value != null) && (el2.Element("ModificationDateTime").Value.Equals("") == false))
                        ModificationDateTime = DateTime.Parse(el2.Element("ModificationDateTime").Value);

                    DateTime? DeleteDateTime = null;
                    if ((el2.Element("DeletionDateTime").Value != null) && (el2.Element("DeletionDateTime").Value.Equals("") == false))
                        DeleteDateTime = DateTime.Parse(el2.Element("DeletionDateTime").Value);

                    WorkItemStatusEntry wise = new WorkItemStatusEntry(wi.Meta.WorkItem_ID, statusID, completionAmount, wiseCreateDateTime, DeleteDateTime);
                    Console.WriteLine($"A: About to insert for WorkItem {wi.Meta.WorkItem_ID} a Status ID of {statusID}");
                    controller.InsertDBWorkItemStatusEntry(wise);
                }

                // Due Dates
                var query3 = from element in el3.Descendants("DueDateChange")
                                where Int32.Parse(el3.Attribute("WorkItem_ID").Value) == importedWorkID
                                select element;
                foreach (var el2 in query3)
                {
                    DateTime dueDateTime = DateTime.Parse(el2.Element("DueDateTime").Value);
                    string changeReason = (string)el2.Element("ChangeReason").Value;

                    DateTime CreateDateTime = DateTime.Now;
                    if ((el3.Element("CreationDateTime").Value != null) && (el2.Element("CreationDateTime").Value.Equals("") == false))
                        CreateDateTime = DateTime.Parse(el2.Element("CreationDateTime").Value);

                    DateTime? DeleteDateTime = null;
                    if ((el3.Element("DeletionDateTime").Value != null) && (el2.Element("DeletionDateTime").Value.Equals("") == false))
                        DeleteDateTime = DateTime.Parse(el2.Element("DeletionDateTime").Value);
                    
                    controller.InsertDBDueDate(wi, dueDateTime, CreateDateTime, changeReason);
                }

                // Journals
                var query4 = from element in el3.Descendants("JournalEntry")
                                where Int32.Parse(el3.Attribute("WorkItem_ID").Value) == importedWorkID
                                select element;
                foreach (var el2 in query4)
                {
                    string journalHeader = "";
                    if (el2.Element("Header").Value != null)
                        journalHeader = (string)el2.Element("Header").Value;

                    string journalEntry = "";
                    if (el2.Element("Entry").Value != null)
                        journalEntry = (string)el2.Element("Entry").Value;

                    DateTime? creationDateTime = null;
                    if ((el2.Element("CreationDateTime").Value != null) && (el2.Element("CreationDateTime").Value.Equals("") == false))
                        creationDateTime = DateTime.Parse(el2.Element("CreationDateTime").Value);

                    DateTime? modificationDateTime = null;
                    if ((el2.Element("ModificationDateTime").Value != null) && (el2.Element("ModificationDateTime").Value.Equals("") == false))
                        modificationDateTime = DateTime.Parse(el2.Element("ModificationDateTime").Value);

                    DateTime? DeleteDateTime = null;
                    if ((el2.Element("DeletionDateTime").Value != null) && (el2.Element("DeletionDateTime").Value.Equals("") == false))
                        DeleteDateTime = DateTime.Parse(el2.Element("DeletionDateTime").Value);
                    
                    JournalEntry journal = new JournalEntry(journalHeader, journalEntry, creationDateTime, modificationDateTime, DeleteDateTime);

                    controller.InsertDBJournalEntry(wi.Meta.WorkItem_ID, journal);
                }

            } // end - foreach (var el3 in query3)
        }
    }
}
