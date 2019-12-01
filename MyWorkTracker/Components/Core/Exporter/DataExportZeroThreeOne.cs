using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Xml.Linq;

namespace MyWorkTracker.Code
{
    class DataExportZeroThreeOne : IDataExport
    {
        public void ExportToXML(string appName, string appVersion, Dictionary<ExportSetting, string> exportSettings)
        {

            string dbConn = exportSettings[ExportSetting.DATABASE_CONNECTION];
            string exportTo = exportSettings[ExportSetting.EXPORT_TO_LOCATION] + @"\" + DateMethods.GetDateString(DateTime.Now) + "_MyWorkTracker.xml";
            bool exportPreferences = Boolean.Parse(exportSettings[ExportSetting.EXPORT_PREFERENCES]);
            string exportWorkItemOptionString = exportSettings[ExportSetting.EXPORT_WORK_ITEM_OPTION];
            int daysStale = Int32.Parse(exportSettings[ExportSetting.EXPORT_DAYS_STALE]);
            bool includeDeleted = Boolean.Parse(exportSettings[ExportSetting.EXPORT_INCLUDE_DELETED]);
            bool onlyLastStatus = Boolean.Parse(exportSettings[ExportSetting.EXPORT_INCLUDE_LAST_STATUS_ONLY]);
            bool onlyLastDueDate = Boolean.Parse(exportSettings[ExportSetting.EXPORT_INCLUDE_LAST_DUEDATE_ONLY]);

            string workItemIDString = "";

            // Add header stuff
            // TODO this should be XDocument at top level.
            XDocument doc = new XDocument();
            XElement allXML = new XElement(appName,
                new XAttribute("ApplicationVersion", appVersion),
                new XAttribute("ExtractDate", DateTime.Now.ToString()),
                new XAttribute("ExtractVersion", ExportVersion()));

            // Get the 'application' sub-tags
            XElement appXML = new XElement("Application");
            // --- Get all Settings ------------------------------------------------
            if (exportPreferences)
            {
                appXML.Add(GetPreferencesAsXML(dbConn));
            }
            // Statuses
            XElement allStatusesXML = GetStatusTableAsXML(dbConn);
            appXML.Add(allStatusesXML);
            allXML.Add(appXML);

            XElement workItemsXML = new XElement("WorkItems");

            // --- Export the data into XML collections ----------------------------
            // Start with the selected Work Items
            XElement allWorkItemsXML = null;
            if (exportWorkItemOptionString.ToUpperInvariant().Equals("ALL"))
            { // Export all WorkItems.
                allWorkItemsXML = GetWorkItemTableAsXML(out string str1, dbConn,
                    activeOnly: false, daysStale: 9999, includeDeleted: includeDeleted);
                workItemIDString = ""; // IGNORE the returned str1. We want to output all records, so pass through an empty string of IDs to the rest of the methods.
            }
            else if (exportWorkItemOptionString.ToUpperInvariant().Equals("ONLY ACTIVE"))
            { // Export all WorkItems.
                allWorkItemsXML = GetWorkItemTableAsXML(out string str2, dbConn,
                    activeOnly: true, daysStale: -1, includeDeleted: false);
                workItemIDString = str2;
            }
            else if (exportWorkItemOptionString.ToUpperInvariant().Equals("ACTIVE PLUS CLOSED"))
            { // Export all WorkItems.
                allWorkItemsXML = GetWorkItemTableAsXML(out string str3, dbConn, activeOnly: false, daysStale: daysStale, includeDeleted);
                workItemIDString = str3;
            }

            // Get other data
            XElement allStatusChangesXML = GetWorkItemStatusTableAsXML(dbConn, onlyLastStatus, workItemIDString);
            XElement allDueDateXML = GetDueDateTableAsXML(dbConn, workItemIDString, onlyLastDueDate);
            XElement allJournalsXML = GetJournalTableAsXML(dbConn, includeDeleted, workItemIDString);

            // --- Process the data into the format we want ------------------------
            IEnumerable<XElement> workItems = from wi in allWorkItemsXML.Elements("WorkItem")
                                              select wi;
            foreach (XElement workItemXML in workItems)
            {
                // Get all Statuses that relate to that WorkItem
                IEnumerable<XElement> wiStatusColl = from el2 in allStatusChangesXML.Elements("StatusChanges")
                                                     where (string)el2.Attribute("WorkItem_ID") == (string)workItemXML.Attribute("WorkItem_ID")
                                                     select el2;
                foreach (XElement workItemStatusXML in wiStatusColl)
                {
                    workItemXML.Add(workItemStatusXML);
                }

                // Get all the Due Dates that relate to the WorkItem
                IEnumerable<XElement> wiDueDateColl = from el3 in allDueDateXML.Elements("DueDateChanges")
                                                      where (string)el3.Attribute("WorkItem_ID") == (string)workItemXML.Attribute("WorkItem_ID")
                                                      select el3;
                foreach (XElement workItemDueDateXML in wiDueDateColl)
                {
                    workItemXML.Add(workItemDueDateXML);
                }

                // Get all the Journal Entries that relate to the WorkItem
                IEnumerable<XElement> wiJEColl = from el4 in allJournalsXML.Elements("JournalEntries")
                                                 where (string)el4.Attribute("WorkItem_ID") == (string)workItemXML.Attribute("WorkItem_ID")
                                                 select el4;
                foreach (XElement workItemJournalXML in wiJEColl)
                {
                    workItemXML.Add(workItemJournalXML);
                }

                workItemsXML.Add(workItemXML);
            }

            allXML.Add(workItemsXML);
            doc.Add(allXML);
            doc.Save(exportTo);
        }

        /// <summary>
        /// Export the Journal table as XML format.
        /// </summary>
        /// <param name="dbConnString"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="workItemIDs"></param>
        /// <returns></returns>
        private protected XElement GetJournalTableAsXML(string dbConnString, bool includeDeleted, string workItemIDs)
        {
            // --- Get all WorkItem Journals ---------------------------------------
            XElement allJournalsXML = new XElement("AllJournalEntries");
            XElement workItemJournalEntriesXML = null;
            XElement workItemJournalEntryXML = null;
            int previousWorkItemID = -1;
            using (var connection = new SQLiteConnection(dbConnString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM vwExtractAllJournal_ZeroThreeZero";
                    int whereConditions = 0;
                    if ((workItemIDs.Equals("") == false) || (includeDeleted == false))
                    {
                        cmd.CommandText += " WHERE";
                    }
                    if (workItemIDs.Equals("") == false)
                    {
                        ++whereConditions;
                        cmd.CommandText += " (WorkItem_ID IN (@wiIDs))";
                        cmd.Parameters.AddWithValue("@wiIDs", workItemIDs);
                    }
                    if (includeDeleted == false)
                    {
                        if (whereConditions > 0)
                            cmd.CommandText += " AND ";

                        cmd.CommandText += " JournalOrWorkItemDeleted = 0";
                    }
                    cmd.CommandText += " ORDER BY WorkItem_ID, CreationDateTime";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int journalID = Convert.ToInt32(reader["Journal_ID"]);
                            int currentWorkItemID = Convert.ToInt32(reader["WorkItem_ID"]);
                            string header = "";
                            string entry = "";
                            if (reader["Header"] != DBNull.Value)
                                header = (string)reader["Header"];
                            if (reader["Entry"] != DBNull.Value)
                                entry = (string)reader["Entry"];


                            if (currentWorkItemID != previousWorkItemID)
                            {
                                previousWorkItemID = currentWorkItemID;
                                if (workItemJournalEntriesXML != null)
                                    allJournalsXML.Add(workItemJournalEntriesXML);

                                workItemJournalEntriesXML = new XElement("JournalEntries", new XAttribute("WorkItem_ID", currentWorkItemID));
                            }

                            DateTime? creationDateTime = null;
                            if (reader["CreationDateTime"] != DBNull.Value)
                            {
                                creationDateTime = Convert.ToDateTime(reader["CreationDateTime"].ToString());
                            }
                            DateTime? journalModDateTime = null;
                            if (reader["ModificationDateTime"] != DBNull.Value)
                            {
                                journalModDateTime = Convert.ToDateTime(reader["ModificationDateTime"].ToString());
                            }
                            DateTime? journalDelDateTime = null;
                            if (reader["JournalDeletionDateTime"] != DBNull.Value)
                            {
                                journalDelDateTime = Convert.ToDateTime(reader["DeletionDateTime"].ToString());
                            }
                            DateTime? workItemDelDateTime = null;
                            if (reader["WorkItemDeletionDateTime"] != DBNull.Value)
                            {
                                workItemDelDateTime = Convert.ToDateTime(reader["WorkItemDeletionDateTime"].ToString());
                            }
                            bool isDeleted = Convert.ToBoolean(reader["JournalOrWorkItemDeleted"]);

                            if (journalID == -1)
                            {
                                // Do nothing
                            }
                            else
                            {
                                workItemJournalEntryXML = new XElement(new XElement("JournalEntry", new XAttribute("Journal_ID", journalID)));
                                workItemJournalEntryXML.Add(new XElement("Header", header),
                                    new XElement("Entry", entry),
                                    new XElement("CreationDateTime", creationDateTime),
                                    new XElement("ModificationDateTime", journalModDateTime),
                                    new XElement("DeletionDateTime", journalDelDateTime));
                                workItemJournalEntriesXML.Add(workItemJournalEntryXML);
                            }
                        }
                        // Add the last one
                        allJournalsXML.Add(workItemJournalEntriesXML);
                    }
                    connection.Close();
                }
            }
            return allJournalsXML;
        }

        /// <summary>
        /// Export the DueDate table as XML format.
        /// </summary>
        /// <param name="dbConnString"></param>
        /// <param name="workItemIDs"></param>
        /// <param name="onlyLastDueDate">If true, only the last DueDate per WorkItem will be included. If false, all Due Date changes.</param>
        /// <returns></returns>
        private protected XElement GetDueDateTableAsXML(string dbConnString, string workItemIDs, bool onlyLastDueDate)
        {
            /*
               <DueDateChanges WorkItem_ID="2">
                 <DueDateChange DueDate_ID="2">
                    <DueDateTime>2019-08-05T13:45:00</DueDateTime>
                    <ChangeReason />
                    <CreationDate>2019-08-12T09:22:59</CreationDate>
                    <DeletionDate>2019-08-12T09:22:59</DeletionDate>
                 </DueDateChange>
              </DueDateChanges>
             */

            // --- Get all WorkItem Due Dates --------------------------------------
            XElement allDueDatesXML = new XElement("AllDueDateChanges");
            XElement wiDueDatesXML = null;
            XElement workItemDueDateXML = null;
            int previousWorkItemID = -1;
            using (var connection = new SQLiteConnection(dbConnString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM ";
                    if (onlyLastDueDate)
                        cmd.CommandText += "vwExtractLatestDueDate_ZeroThreeZero ";
                    else
                        cmd.CommandText += "DueDate ";

                    if (workItemIDs.Equals("") == false)
                    {
                        cmd.CommandText += " WHERE WorkItem_ID IN (@ids)";
                        cmd.Parameters.AddWithValue("@ids", workItemIDs);
                    }
                    cmd.CommandText += " ORDER BY WorkItem_ID, CreationDateTime ASC";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int currentWorkItemID = Convert.ToInt32(reader["WorkItem_ID"]);

                            if (currentWorkItemID != previousWorkItemID)
                            {
                                previousWorkItemID = currentWorkItemID;
                                if (wiDueDatesXML != null)
                                    allDueDatesXML.Add(wiDueDatesXML);
                            }

                            DateTime? creationDateTime = null;
                            if (reader["CreationDateTime"] != DBNull.Value)
                            {
                                creationDateTime = Convert.ToDateTime(reader["CreationDateTime"].ToString());
                            }
                            DateTime? deletionDateTime = null;
                            if (reader["DeletionDateTime"] != DBNull.Value)
                            {
                                deletionDateTime = Convert.ToDateTime(reader["DeletionDateTime"].ToString());
                            }
                            string changeReason = null;
                            if (reader["ChangeReason"] != DBNull.Value)
                            {
                                changeReason = (string)(reader["ChangeReason"]);
                            }

                            wiDueDatesXML = new XElement("DueDateChanges", new XAttribute("WorkItem_ID", currentWorkItemID));

                            workItemDueDateXML = new XElement("DueDateChange", new XAttribute("DueDate_ID", Convert.ToInt32(reader["DueDate_ID"])));
                            workItemDueDateXML.Add(new XElement("DueDateTime", Convert.ToDateTime(reader["DueDateTime"])),
                                new XElement("ChangeReason", changeReason),
                                new XElement("CreationDateTime", creationDateTime),
                                new XElement("DeletionDateTime", deletionDateTime));

                            wiDueDatesXML.Add(workItemDueDateXML);
                        }
                        // Add the last one
                        allDueDatesXML.Add(wiDueDatesXML);
                    }
                    connection.Close();
                }
            }
            return allDueDatesXML;
        }

        /// <summary>
        /// Export the WorkItemStatus table as XML format.
        /// </summary>
        /// <param name="dbConnString"></param>
        /// <param name="workItemIDs"></param>
        /// <param name="onlyLastStatus">If true, only the last status per WorkItem will be included. If false, all Status changes.</param>
        /// <returns></returns>
        private protected XElement GetWorkItemStatusTableAsXML(string dbConnString, bool onlyLastStatus, string workItemIDs = "-1")
        {
            /*
             * <AllStatusChanges>
                  <StatusChanges WorkItem_ID="1">
                    <StatusChange WorkItemStatus_ID="1">
                      <Status_ID>1</Status_ID>
                      <CompletionAmount>100</CompletionAmount>
                      <CreationDate>2019-08-03T04:28:04</CreationDate>
                      <DeletionDate>2019-08-03T04:28:04</DeletionDate>
                    </StatusChange>
                    <StatusChange WorkItemStatus_ID="3">
                      <Status_ID>1</Status_ID>
                      <CompletionAmount>30</CompletionAmount>
                      <CreationDate>2019-08-05T08:27:30</CreationDate>
                    </StatusChange>
                ...
                </StatusChanges>
             </AllStatusChanges>
             */

            // Check to see if the query should be limited by a list of WorkItemIDs.
            bool isLimited = true;
            if (workItemIDs.Equals(""))
                isLimited = false;

            XElement allStatusChangesXML = new XElement("AllStatusChanges");
            XElement workItemStatusXML = null;
            using (var connection = new SQLiteConnection(dbConnString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();

                    cmd.CommandText = "SELECT * FROM ";
                    if (onlyLastStatus)
                        cmd.CommandText += "vwExtractLatestWorkItemStatus_ZeroThreeOne";
                    else
                        cmd.CommandText += "WorkItemStatus";

                    if (isLimited)
                    {
                        cmd.CommandText += " WHERE WorkItem_ID IN (@workItemIDs)";
                        cmd.Parameters.AddWithValue("@workItemIDs", workItemIDs);
                    }
                    cmd.CommandText += " ORDER BY WorkItem_ID";

                    if (onlyLastStatus == false)
                        cmd.CommandText += ", CreationDateTime ASC";

                    int previousWorkItemID = -1; // Can't find vwExtractWorkItemStatus
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int currentWorkItemID = Convert.ToInt32(reader["WorkItem_ID"]);

                            if (currentWorkItemID != previousWorkItemID)
                            {
                                previousWorkItemID = currentWorkItemID;

                                if (workItemStatusXML != null)
                                {
                                    allStatusChangesXML.Add(workItemStatusXML);
                                }

                                workItemStatusXML = new XElement("StatusChanges", new XAttribute("WorkItem_ID", currentWorkItemID));
                            }

                            DateTime? creationDateTime = null;
                            if (reader["CreationDateTime"] != DBNull.Value)
                            {
                                creationDateTime = Convert.ToDateTime(reader["CreationDateTime"].ToString());
                            }
                            DateTime? modificationDateTime = null;
                            if (reader["ModificationDateTime"] != DBNull.Value)
                            {
                                modificationDateTime = Convert.ToDateTime(reader["ModificationDateTime"].ToString());
                            }
                            DateTime? deletionDateTime = null;
                            if (reader["DeletionDateTime"] != DBNull.Value)
                            {
                                deletionDateTime = Convert.ToDateTime(reader["DeletionDateTime"].ToString());
                            }

                            XElement workItemStatusChangeXML = new XElement(new XElement("StatusChange", new XAttribute("WorkItemStatus_ID", Convert.ToInt32(reader["WorkItemStatus_ID"]))));
                            workItemStatusChangeXML.Add(
                                new XElement("Status_ID", Convert.ToInt32(reader["Status_ID"])),
                                new XElement("CompletionAmount", Convert.ToInt32(reader["CompletionAmount"])),
                                new XElement("CreationDateTime", creationDateTime),
                                new XElement("ModificationDateTime", modificationDateTime),
                                new XElement("DeletionDateTime", deletionDateTime));
                            workItemStatusXML.Add(workItemStatusChangeXML);
                        }
                        // Add last one.
                        allStatusChangesXML.Add(workItemStatusXML);
                    }
                    connection.Close();
                }
            }
            return allStatusChangesXML;
        }

        /// <summary>
        /// Export the WorkItem table as XML format.
        /// </summary>
        /// <param name="dbConnString"></param>
        /// <param name="activeOnly">Export only those WorkItems that are of an Open-kind</param>
        /// <param name="daysStale">If activeOnly=false, export only Open and those WorkItems Closed within x days.</param>
        /// <param name="includeDeleted"></param>
        /// <param name="wiIDString">Returns a comma-separated list of WorkItem IDs which match the parameter inputs.</param>
        /// <returns>
        /// <paramref name="wiIDString"/>
        /// </returns>
        private protected XElement GetWorkItemTableAsXML(out string wiIDString, string dbConnString, bool activeOnly = false, int daysStale = 9999, bool includeDeleted = false)
        {
            /*
             *  <WorkItems>
             *      <WorkItem WorkItem_ID="1">
             *      <Title>Application of Sensitivity Requested</Title>
             *      <Description>This is the description</Description>
             *      <CreationDate>2019-08-03T04:28:03</CreationDate>
             *      <DeletionDate />
             *      </WorkItem>
             *      ...
             *      </WorkItems>
             */
            wiIDString = "";
            XElement workItemXML = null;
            XElement allWorkItemsXML = new XElement("WorkItems");
            using (var connection = new SQLiteConnection(dbConnString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM vwExtractWorkItem_ZeroThreeOne WHERE DaysSinceCompletion <= @staleDays";
                    if (activeOnly)
                        cmd.CommandText += " AND IsConsideredActive = @activeOnly";
                    if (includeDeleted == false)
                        cmd.CommandText += " AND DeletionDateTime IS NULL";
                    cmd.CommandText += " ORDER BY WorkItem_ID";
                    cmd.Parameters.AddWithValue("@staleDays", daysStale);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int workItemID = Convert.ToInt32(reader["WorkItem_ID"]);
                            wiIDString += Convert.ToString(workItemID) + ",";

                            DateTime? deletionDateTime = null;
                            if (reader["DeletionDateTime"] != DBNull.Value)
                            {
                                deletionDateTime = Convert.ToDateTime(reader["DeletionDateTime"].ToString());
                            }
                            workItemXML = new XElement("WorkItem",
                                new XAttribute("WorkItem_ID", workItemID),
                                new XAttribute("LastWorkItemStatus_ID", Convert.ToInt32(reader["Status_ID"])),
                                new XElement("Title", (string)reader["TaskTitle"]),
                                new XElement("Description", (string)reader["TaskDescription"]),
                                new XElement("CreationDateTime", Convert.ToDateTime(reader["CreationDateTime"])),
                                new XElement("DeletionDateTime", deletionDateTime));
                            allWorkItemsXML.Add(workItemXML);
                        }
                    }
                    connection.Close();
                }
            }

            if (wiIDString.Length > 0)
                wiIDString = wiIDString.Substring(0, wiIDString.Length - 1);

            return allWorkItemsXML;
        }

        /// <summary>
        /// Export the user-controllable preferences as XML.
        /// </summary>
        /// <param name="dbConnString"></param>
        /// <returns></returns>
        private protected XElement GetPreferencesAsXML(string dbConnString)
        {
            XElement allPreferences = new XElement("Preferences");

            using (var connection = new SQLiteConnection(dbConnString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT Name, Value FROM Setting WHERE UserCanEdit = 'Y'";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            XElement preferenceXML = new XElement("Preference");
                            preferenceXML.Add(new XElement("Name", (string)(reader["Name"])));
                            preferenceXML.Add(new XElement("Value", (string)(reader["Value"])));
                            allPreferences.Add(preferenceXML);
                        }
                    }
                    connection.Close();
                }
            }

            return allPreferences;
        }

        /// <summary>
        /// Export the Status table as XML format.
        /// </summary>
        /// <param name="dbConnString"></param>
        /// <returns></returns>
        private protected XElement GetStatusTableAsXML(string dbConnString)
        {
            XElement allStatusesXML = new XElement("Statuses");
            using (var connection = new SQLiteConnection(dbConnString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM Status";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            XElement statusXML = new XElement("Status", new XAttribute("Status_ID", Convert.ToInt32(reader["Status_ID"])));
                            statusXML.Add(new XElement("StatusLabel", (string)(reader["StatusLabel"])),
                            new XElement("IsConsideredActive", Convert.ToBoolean(reader["IsConsideredActive"])),
                            new XElement("IsDefault", Convert.ToBoolean(reader["IsDefault"])));
                            DateTime? deletionDateTime = null;
                            if (reader["DeletionDateTime"] != DBNull.Value)
                            {
                                deletionDateTime = Convert.ToDateTime(reader["DeletionDateTime"].ToString());
                            }
                            statusXML.Add(new XElement("DeletionDateTime", deletionDateTime));

                            allStatusesXML.Add(statusXML);
                        }
                    }
                    connection.Close();
                }
            }
            return allStatusesXML;
        }

        public string ExportVersion()
        {
            return "0.3.1";
        }

    }
}
