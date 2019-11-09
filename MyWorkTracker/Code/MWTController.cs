using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace MyWorkTracker.Code
{
    public class MWTController
    {
        private string dbConnectionString = "data source=" + MWTModel.DatabaseFile;

        MWTModel _model = null;

        public MWTController(MWTModel model)
        {
            _model = model;

            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = (System.IO.Path.GetDirectoryName(executable));

            dbConnectionString = dbConnectionString.Replace("=", "=" + path);
            Console.WriteLine($"Loading data from {dbConnectionString}");

            LoadApplicationPreferences();
            LoadWorkItemStatusesFromDB();

            LoadWorkItems(Int32.Parse(_model.GetAppPreferenceValue(PreferenceName.LOAD_STALE_DAYS)));
        }

        /// <summary>
        /// Import data from an XML backup into the system.
        /// </summary>
        /// <param name="importFromVersion">The version of the file that we're trying to import.</param>
        /// <param name="loadPreferences"></param>
        /// <param name="importStatuses"></param>
        /// <param name="xml"></param>
        public void ImportData(string importFromVersion, Dictionary<PreferenceName, string> loadPreferences, List<WorkItemStatus> importStatuses, XDocument xml)
        {
            IDataImport importerHandler;
            switch (importFromVersion)
            {
                case "0.3.0":
                    importerHandler = new DataImportZeroThreeZero();
                    break;
                case "0.3.1":
                    importerHandler = new DataImportZeroThreeOne();
                    break;
                default:
                    importerHandler = null;
                    MessageBox.Show($"There is no support for importing from this version", "Cannot Import");
                    break;
            }

            if (importerHandler != null)
            {
                importerHandler.ImportData(this, importFromVersion, loadPreferences, importStatuses, xml);
                LoadWorkItems(Int32.Parse(_model.GetAppPreferenceValue(PreferenceName.LOAD_STALE_DAYS)));
            }

        }

        public string DBConnectionString => dbConnectionString;

        /// <summary>
        /// Create a new WorkItem. This creates the WorkItem in memory and initialises some of it, but doesn't write anything to the DB.
        /// The DueDate is set based on default Settings.
        /// </summary>
        public void CreateNewWorkItem()
        {
            // Get the default active Status. (Note due to DB constraints there can only be one type that is active and default).
            WorkItemStatus wis = GetWorkItemStatuses(isConsideredActive: true, isDefault: true).ToArray()[0];

            var wi = new WorkItem(wis);
            wi.Status = wis.Status;                         // TODO: Remove; should be deprecated
            wi.IsConsideredActive = wis.IsConsideredActive; // TODO: Remove; should be deprecated

            // Set the due date based on the default.
            wi.DueDate = CalculateDefaultDueDate();

            _model.FireCreateNewWorkItem(wi);
        }

        /// <summary>
        /// Calculate the default Due Date (based on today + default Preferences)
        /// </summary>
        /// <returns></returns>
        private DateTime CalculateDefaultDueDate()
        {
            // Calculate the due date
            //   1. This will be the current datetime + the default amount
            DateTime dueDate = DateTime.Now.AddDays(Double.Parse(_model.GetAppPreferenceValue(PreferenceName.DEFAULT_WORKITEM_LENGTH_DAYS)));
            //   2. + end of the work day
            dueDate = new DateTime(dueDate.Year, dueDate.Month, dueDate.Day,
                int.Parse(_model.GetAppPreferenceValue(PreferenceName.DEFAULT_WORKITEM_COB_HOURS)),
                int.Parse(_model.GetAppPreferenceValue(PreferenceName.DEFAULT_WORKITEM_COB_MINS)),
                0);

            if (_model.GetAppPreferenceAsBooleanValue(PreferenceName.DUE_DATE_CAN_BE_WEEKENDS))
            {
                if (dueDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    dueDate = dueDate.AddDays(2);
                }
                else if (dueDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    dueDate = dueDate.AddDays(1);
                }
            }

            return dueDate;
        }

        public void AddJournalEntry(WorkItem wi, JournalEntry entry)
        {
            switch (_model.GetAppPreferenceValue(PreferenceName.JOURNAL_ORDERING))
            {
                case "bottom":
                    wi.Journals.Add(entry);
                    break;
                case "top":
                    wi.Journals.Insert(0, entry);
                    break;
            }
        }

        /// <summary>
        /// Export data as XML.
        /// </summary>
        /// <param name="exportVersion"></param>
        /// <param name="exportSettings"></param>
        public void ExportToXML(string exportVersion, Dictionary<ExportSetting, string> exportSettings)
        {
            IDataExport exporterHandler;
            switch (exportVersion)
            {
                case "0.3.0":
                    exporterHandler = new DataExportZeroThreeZero();
                    break;
                case "0.3.1":
                    exporterHandler = new DataExportZeroThreeOne();
                    break;
                default:
                    exporterHandler = new DataExportZeroThreeZero();
                    break;
            }

            exporterHandler.ExportToXML(_model.GetAppPreferenceValue(PreferenceName.APPLICATION_NAME),
                _model.GetAppPreferenceValue(PreferenceName.APPLICATION_VERSION), exportSettings);
        }
        
        public Dictionary<PreferenceName, string> GetPreferencesBeginningWith(string startsWith)
        {
            Dictionary<PreferenceName, string> rValue = new Dictionary<PreferenceName, string>();
            foreach (PreferenceName pName in _model.GetAppPreferenceCollection().Keys)
            {
                if (pName.ToString().StartsWith(startsWith))
                {
                    rValue.Add(pName, _model.GetAppPreferenceValue(pName));
                }
            }
            return rValue;
        }

/*        /// <summary>
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
                    cmd.CommandText = "SELECT * FROM vwExtractAllJournal";
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
                                    new XElement("JournalDeletionDateTime", journalDelDateTime));
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
/*
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
                        cmd.CommandText += "vwExtractLatestDueDate ";
                    else
                        cmd.CommandText += "DueDate ";

                    if (workItemIDs.Equals("") == false) {
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
                                new XElement("CreationDate", creationDateTime),
                                new XElement("DeletionDate", deletionDateTime));

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
/*
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
                        cmd.CommandText += "vwExtractWorkItemStatus";
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

                    int previousWorkItemID = -1;
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
                                new XElement("CreationDate", creationDateTime),
                                new XElement("ModificationDateTime", modificationDateTime),
                                new XElement("DeletionDate", deletionDateTime));
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
        /*    wiIDString = "";
            XElement workItemXML = null;
            XElement allWorkItemsXML = new XElement("WorkItems");
            using (var connection = new SQLiteConnection(dbConnString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM vwExtractWorkItem WHERE DaysSinceCompletion <= @staleDays";
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
                                new XElement("CreationDate", Convert.ToDateTime(reader["CreationDateTime"])), 
                                new XElement("DeletionDate", deletionDateTime));
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
                            statusXML.Add(new XElement("DeletionDate", deletionDateTime));

                            allStatusesXML.Add(statusXML);
                        }
                    }
                    connection.Close();
                }
            }
            return allStatusesXML;
        }*/

        /// <summary>
        /// Return WorkItemStatus(es) based on their IsConsideredActive or IsDefault value.
        /// </summary>
        /// <param name="isConsideredActive"></param>
        /// <param name="isDefault"></param>
        /// <returns></returns>
        public IEnumerable<WorkItemStatus> GetWorkItemStatuses(bool? isConsideredActive = null, bool? isDefault = null)
        {
            IEnumerable<WorkItemStatus> rValue;

            // Return based on isActive
            if ((isConsideredActive.HasValue == true) && (isDefault.HasValue == false))
                rValue = _model.GetWorkItemStatuses().Where(u => u.IsConsideredActive == isConsideredActive);
            // Return based on IsDefault (2 return items possible)
            else if ((isConsideredActive.HasValue == false) && (isDefault.HasValue == true))
                rValue = _model.GetWorkItemStatuses().Where(u => u.IsDefault == isDefault);
            // Return based on isActive and IsDefault
            else if ((isConsideredActive.HasValue == true) && (isDefault.HasValue == true))
                rValue = _model.GetWorkItemStatuses().Where(u => u.IsConsideredActive == isConsideredActive && u.IsDefault == isDefault);
            // Return all
            else
                rValue = _model.GetWorkItemStatuses();

            return rValue;
        }

        public MWTModel GetMWTModel()
        {
            return _model;
        }

        /// <summary>
        /// Sets the WorkItemStatus on a WorkItem.
        /// Also sets the IsConsideredActive convenience variable and flags the WorkItemStatus as needing an update.
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="newWorkItemStatus"></param>
        public void SetWorkItemStatus(WorkItem wi, WorkItemStatus newWorkItemStatus)
        {
            WorkItemStatus oldStatus = GetWorkItemStatus(wi.Status);
            wi.Status = newWorkItemStatus.Status;
            if (_model.IsBindingLoading == false)
            {
                wi.Meta.WorkItemStatusNeedsUpdate = true;
            }
            wi.IsConsideredActive = newWorkItemStatus.IsConsideredActive;
            _model.FireWorkItemStatusChange(wi, newWorkItemStatus, oldStatus);
        }

        /// <summary>
        /// Returns the WorkItemStatus as specified by the status ID.
        /// </summary>
        /// <param name="statusID"></param>
        /// <returns></returns>
        public WorkItemStatus GetWorkItemStatus(int statusID)
        {
            WorkItemStatus rValue = null;

            // TODO replace with LINQ
            foreach (WorkItemStatus wis in _model.GetWorkItemStatuses())
            {
                if (wis.WorkItemStatusID == statusID)
                {
                    rValue = wis;
                    break;
                }
            }

            return rValue;
        }

        /// <summary>
        /// Returns the WorkItemStatus as specified by the status string.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public WorkItemStatus GetWorkItemStatus(string status)
        {
            WorkItemStatus rValue = null;

            // TODO replace with LINQ
            foreach (WorkItemStatus wis in _model.GetWorkItemStatuses())
            {
                if (wis.Status.Equals(status))
                {
                    rValue = wis;
                    break;
                }
            }

            return rValue;
        }

        /// <summary>
        /// Load the Journal Entries for the specified Work Item.
        /// </summary>
        /// <param name="wi"></param>
        public void LoadJournalEntries(WorkItem wi)
        {
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM vwActiveJournal WHERE WorkItem_ID = @workItemID";
                    cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int jID = Convert.ToInt32(reader["Journal_ID"]);
                            string title = (string)reader["Header"];
                            string entry = (string)reader["Entry"];
                            DateTime create = Convert.ToDateTime(reader["CreationDateTime"].ToString());
                            DateTime? modifDateTime = null;
                            if (reader["ModificationDateTime"] != DBNull.Value)
                            {
                                modifDateTime = Convert.ToDateTime(reader["ModificationDateTime"].ToString());
                            }

                            wi.Journals.Add(new JournalEntry(jID, title, entry, create, modifDateTime));
                        }
                    }
                    connection.Close();
                }
            }
            wi.Meta.AreJournalItemsLoaded = true;
        }

        /// <summary>
        /// Return the loaded Work Items with the specified active/inactive status.
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        public IEnumerable<WorkItem> GetWorkItems(bool active)
        {
            return _model.GetWorkItems().Where(u => u.IsConsideredActive == active);
        }

        /// <summary>
        /// Load selected Work Items from the database into memory.
        /// Note that this doesn't include Journal entries, which are lazy-loaded on selection.
        /// </summary>
        /// <param name="loadAgedDays">Used to selectively load Work Items based on the number of days since they've been put in a Closed state.
        /// Use -1 to load only active Work Items
        /// Use 0 to load only active Work Items + those Completed today. 
        /// Use 10 would give you active + any Work Items completed in the last 10 calendar days.</param>
        private void LoadWorkItems(int loadAgedDays)
        {
            // When this is called, first clear any content.
            _model.ClearAllWorkItems();

            Console.WriteLine($"loadAgedDays={loadAgedDays}");

            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM vwWorkItem ORDER BY DaysSinceCompletion ASC, DueDateTime ASC";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int daysStale = Convert.ToInt32(reader["DaysSinceCompletion"]);

                            // Only create a WorkItem and add it to the model if DaysSinceCompletion <= loadAgedDays
                            if (daysStale <= loadAgedDays)
                            {
                                WorkItem wi = new WorkItem();
                                wi.Meta.WorkItem_ID = Convert.ToInt32(reader["WorkItem_ID"]);
                                wi.Title = (string)reader["TaskTitle"];
                                wi.CreationDateTime = DateTime.Parse(reader["CreationDateTime"].ToString());
                                if (reader["TaskDescription"].GetType() != typeof(DBNull))
                                    wi.TaskDescription = (string)reader["TaskDescription"];

                                if (reader["DueDateTime"] != DBNull.Value)
                                    wi.DueDate = DateTime.Parse(reader["DueDateTime"].ToString());

                                if (reader["DueDateCreationDateTime"] != DBNull.Value)
                                wi.Meta.DueDateUpdateDateTime = DateTime.Parse(reader["DueDateCreationDateTime"].ToString());

                                wi.Status = reader["wisStatusLabel"].ToString();
                                wi.Meta.StatusUpdateDateTime = DateTime.Parse(reader["wisStatusDateTime"].ToString());
                                wi.IsConsideredActive = Boolean.Parse(reader["wisIsConsideredActive"].ToString());

                                // --- WorkItemStatus ----
                                int statusID = Convert.ToInt32(reader["wisStatus_ID"]);
                                wi.workItemStatus = GetWorkItemStatus(statusID);

                                // --- WorkItemStatusEntry ----
                                DateTime? wiseCreationDateTime = null;
                                if (reader["wisCreationDateTime"] != DBNull.Value)
                                    wiseCreationDateTime = DateTime.Parse(reader["wisCreationDateTime"].ToString());

                                DateTime? wiseModificationDateTime = null;
                                if (reader["wisModificationDateTime"] != DBNull.Value)
                                    wiseModificationDateTime = DateTime.Parse(reader["wisModificationDateTime"].ToString());

                                WorkItemStatusEntry wise = new WorkItemStatusEntry(wiseID: Convert.ToInt32(reader["wis_ID"]),
                                    wiID: Convert.ToInt32(reader["WorkItem_ID"]),
                                    statusID: statusID,
                                    completionAmount: Convert.ToInt32(reader["CompletionAmount"]),
                                    creationDateTime: wiseCreationDateTime,
                                    modificationDateTime: wiseModificationDateTime
                                );
                                wi.WorkItemStatusEntry = wise;
                                Console.WriteLine($"Loading {(string)reader["TaskTitle"]}");
                                _model.AddWorkItem(wi, false, false);
                            }
                            else
                            {
                                Console.WriteLine($"Not loading {(string)reader["TaskTitle"]} because it's stale: {daysStale}.");
                            }
                        }
                    }
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Delete a WorkItem; either physically or logically.
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="logicallyDelete"></param>
        public void DeleteWorkItem(WorkItem wi, bool logicallyDelete)
        {
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    if (logicallyDelete)
                    {
                        // WorkItem
                        cmd.CommandText = "UPDATE WorkItem SET DeletionDateTime = @now WHERE WorkItem_ID = @workItemID";
                        cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.ExecuteNonQuery();

                        // WorkItemStatus
                        cmd.CommandText = "UPDATE WorkItemStatus SET DeletionDateTime = @now WHERE WorkItem_ID = @workItemID";
                        cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.ExecuteNonQuery();

                        // Due Date
                        cmd.CommandText = "UPDATE DueDate SET DeletionDateTime = @now WHERE WorkItem_ID = @workItemID";
                        cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.ExecuteNonQuery();

                        // Journal
                        cmd.CommandText = "UPDATE Journal SET DeletionDateTime = @now WHERE WorkItem_ID = @workItemID";
                        cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                    else // Physical delete
                    {
                        // WorkItem
                        cmd.CommandText = "DELETE FROM WorkItem WHERE WorkItem_ID = @workItemID";
                        cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                        cmd.ExecuteNonQuery();

                        // WorkItemStatus
                        cmd.CommandText = "DELETE FROM WorkItemStatus WHERE WorkItem_ID = @workItemID";
                        cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                        cmd.ExecuteNonQuery();

                        // Due Date
                        cmd.CommandText = "DELETE FROM DueDate WHERE WorkItem_ID = @workItemID";
                        cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                        cmd.ExecuteNonQuery();

                        // Journal
                        cmd.CommandText = "DELETE FROM Journal WHERE WorkItem_ID = @workItemID";
                        cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }

            // Delete it (in memory)
            _model.RemoveWorkItem(wi);
        }

        /// <summary>
        /// Reset all Preferences (in database and memory)
        /// </summary>
        public void ResetAllPreferences()
        {
            // On Database
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "UPDATE Setting SET Value = DefaultValue WHERE UserCanEdit='Y'";
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }

            // In memory
            _model.ClearApplicationPreferences();
            LoadApplicationPreferences();
        }

        /// <summary>
        /// Create a Backup file as an XML.
        /// Optionally, also creates a copy and saves the file.
        /// </summary>
        public void CreateBackupFile()
        {
            string exportToFilename = DateMethods.GetDateString(DateTime.Now) + "_MyWorkTracker.xml";
            string exportToPathAndFile = @_model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_SAVE_TO_LOCATION) + '\\' + exportToFilename;
            bool includeDeleted = _model.GetAppPreferenceAsBooleanValue(PreferenceName.DATA_EXPORT_INCLUDE_DELETED);
//            bool overwriteFile = _model.GetAppPreferenceAsBooleanValue(PreferenceName.DATA_EXPORT_SAME_DAY_OVERWRITE);

            bool onlyLastStatus = false;
            if (_model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_STATUS_SELECTION).Equals("latest"))
                onlyLastStatus = true;

            bool onlyLastDueDate = false;
            if (_model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_DUEDATE_SELECTION).Equals("latest"))
                onlyLastDueDate = true;

            var exportSettings = new Dictionary<ExportSetting, string>();
            exportSettings.Add(ExportSetting.DATABASE_CONNECTION, dbConnectionString);
            exportSettings.Add(ExportSetting.EXPORT_TO_LOCATION, exportToPathAndFile);
            exportSettings.Add(ExportSetting.EXPORT_PREFERENCES, _model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_INCLUDE_PREFERENCES));
            exportSettings.Add(ExportSetting.EXPORT_WORK_ITEM_OPTION, _model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_WORKITEM_SELECTION));
            exportSettings.Add(ExportSetting.EXPORT_DAYS_STALE, _model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_DAYS_STALE));
            exportSettings.Add(ExportSetting.EXPORT_INCLUDE_DELETED, Convert.ToString(includeDeleted));
            exportSettings.Add(ExportSetting.EXPORT_INCLUDE_LAST_STATUS_ONLY, Convert.ToString(onlyLastStatus));
            exportSettings.Add(ExportSetting.EXPORT_INCLUDE_LAST_DUEDATE_ONLY, Convert.ToString(onlyLastDueDate));
            ExportToXML(_model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_DEFAULT_VERSION), exportSettings);

            if (_model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_COPY_LOCATION).Equals("") == false)
            {
                string copyLocation = @_model.GetAppPreferenceValue(PreferenceName.DATA_EXPORT_COPY_LOCATION) + '\\' + exportToFilename;
                System.IO.File.Copy(exportToPathAndFile, copyLocation, true);
            }

            UpdateAppPreference(PreferenceName.DATA_EXPORT_LAST_DONE, DateMethods.FormatSQLDate(DateTime.Now.Date));
        }

        /// <summary>
        /// Load WorkItem Statuses from the database.
        /// </summary>
        private void LoadWorkItemStatusesFromDB()
        {
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM Status";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["Status_ID"]);
                            string status = (string)reader["StatusLabel"];
                            WorkItemStatus wis = new WorkItemStatus(id, status);
                            wis.IsConsideredActive = (bool)reader["IsConsideredActive"];
                            wis.IsDefault = (bool)reader["IsDefault"];
                            if (reader["DeletionDateTime"] != DBNull.Value)
                                wis.DeletionDate = Convert.ToDateTime(reader["DeletionDateTime"].ToString());
                            _model.AddWorkItemStatus(wis);
                        }
                    }

                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Inserts a database record for a WorkItemStatus.
        /// CompletionAmount is automatically set to 0.
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="status"></param>
        /// <returns>Returns the WorkItemStatus ID on insert, or -1</returns>
        public int InsertDBWorkItemStatusEntry(WorkItem wi, string status)
        {
            WorkItemStatus wis = GetWorkItemStatus(status);
            int workItemStatusID = -1;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO WorkItemStatus (WorkItem_ID, Status_ID, CompletionAmount, CreationDateTime) " +
                        "VALUES (@workItemID, @statusID, @completionAmount, @creation)";
                    cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                    cmd.Parameters.AddWithValue("@statusID", wis.WorkItemStatusID);
                    cmd.Parameters.AddWithValue("@completionAmount", 0);
                    cmd.Parameters.AddWithValue("@creation", DateTime.Now);
                    cmd.ExecuteNonQuery();

                    // Get the identity value
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    workItemStatusID = (int)(Int64)cmd.ExecuteScalar();
                    wi.Meta.WorkItemStatus_ID = workItemStatusID;

                    wi.Meta.WorkItemStatusNeedsUpdate = false;
                }
                connection.Close();
            }
            return workItemStatusID;
        }

        /// <summary>
        /// Inserts a DB record for a WorkItemStatusEntry.
        /// </summary>
        /// <param name="wise"></param>
        /// <returns></returns>
        public int InsertDBWorkItemStatusEntry(WorkItemStatusEntry wise)
        {
            if (wise.CreationDateTime.HasValue == false)
            {
                wise.CreationDateTime = DateTime.Now;
            }

            int workItemStatusID = -1;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO WorkItemStatus (WorkItem_ID, Status_ID, CompletionAmount, CreationDateTime) " +
                        "VALUES (@workItemID, @statusID, @completionAmount, @creation)";
                    cmd.Parameters.AddWithValue("@workItemID", wise.WorkItemID);
                    cmd.Parameters.AddWithValue("@statusID", wise.StatusID);
                    cmd.Parameters.AddWithValue("@completionAmount", wise.CompletionAmount);
                    cmd.Parameters.AddWithValue("@creation", wise.CreationDateTime);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"B: Inserted for WorkItem {wise.WorkItemID} a Status ID of {wise.StatusID}");
                    // Get the identity value
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    workItemStatusID = (int)(Int64)cmd.ExecuteScalar();
                    wise.WorkItemStatusEntryID = workItemStatusID;
                }
                connection.Close();
            }
            return workItemStatusID;
        }

        /// <summary>
        /// Update a DB record for a WorkItemStatusEntry.
        /// Note that updates should occur when it's the same day.
        /// </summary>
        /// <param name="wise"></param>
        private void UpdateDBWorkItemStatusEntry(WorkItemStatusEntry wise)
        {
            if (wise.RecordExists == false) throw new ArgumentException("UpdateDBWorkItemStatusEntry() cannot be called on a record not yet in the database.");

            wise.ModificationDateTime = DateTime.Now;

            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "UPDATE WorkItemStatus " +
                        "SET Status_ID = @statusID, " +
                        "CompletionAmount = @completionAmount, " +
                        "ModificationDateTime = @modificationDateTime " +
                        "WHERE WorkItemStatus_ID = @workItemStatusID";
                    cmd.Parameters.AddWithValue("@statusID", wise.StatusID);
                    cmd.Parameters.AddWithValue("@completionAmount", wise.CompletionAmount);
                    cmd.Parameters.AddWithValue("@modificationDateTime", wise.ModificationDateTime);
                    cmd.Parameters.AddWithValue("@workItemStatusID", wise.WorkItemStatusEntryID);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="completionAmount"></param>
        /// <param name="statusID"></param>
        /// <returns></returns>
        public int InsertOrUpdateDBWorkItemStatusEntry(WorkItem wi, int completionAmount, int statusID)
        {
            int rValue = -1;

            // If the WorkItemStatusEntry hasn't been entered into the database yet,
            //      or if it has, but on a different day... then INSERT
            if ((wi.WorkItemStatusEntry == null)
                || ((wi.WorkItemStatusEntry.ModificationDateTime.HasValue) && (wi.WorkItemStatusEntry.ModificationDateTime.Value.Date != DateTime.Now.Date)))
            { // No record exists, so insert one.
                InsertDBWorkItemStatusEntry(new WorkItemStatusEntry(wi.Meta.WorkItem_ID, wi.WorkItemStatusEntry.StatusID, completionAmount, DateTime.Now));
            }
            else
            {
                WorkItemStatusEntry wise = wi.WorkItemStatusEntry;
                wise.CompletionAmount = completionAmount;
                wise.StatusID = statusID;
                wise.ModificationDateTime = DateTime.Now;
                UpdateDBWorkItemStatusEntry(wise);
            }

            return rValue;
        }

        /// <summary>
        /// Insert a WorkItemStatus to the database.
        /// (This is NOT related to a WorkItem; see InsertDBWorkItemStatusEntry() for that).
        /// </summary>
        /// <param name="wis"></param>
        /// <returns></returns>
        public int InsertDBWorkItemStatus(WorkItemStatus wis)
        {
            int workItemStatusID = -1;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO [Status] (StatusLabel, IsConsideredActive, IsDefault, DeletionDateTime) " +
                        "VALUES (@label, @active, @default, @deletion)";
                    cmd.Parameters.AddWithValue("@label", wis.Status);
                    cmd.Parameters.AddWithValue("@active", wis.IsConsideredActive);
                    cmd.Parameters.AddWithValue("@default", wis.IsDefault);
                    cmd.Parameters.AddWithValue("@deletion", wis.DeletionDate);
                    cmd.ExecuteNonQuery();

                    // Get the identity value
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    workItemStatusID = (int)(Int64)cmd.ExecuteScalar();
                }
                connection.Close();
            }
            return workItemStatusID;
        }

        /// <summary>
        /// Insert a Journal Entry into the database.
        /// </summary>
        /// <param name="workItemID"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public int InsertDBJournalEntry(int workItemID, JournalEntry entry)
        {
            int journalID = -1;

            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO Journal (WorkItem_ID, Header, Entry) " +
                        "VALUES (@workItemID, @header, @entry)";
                    cmd.Parameters.AddWithValue("@workItemID", workItemID);
                    cmd.Parameters.AddWithValue("@header", entry.Title);
                    cmd.Parameters.AddWithValue("@entry", entry.Entry);
                    cmd.ExecuteNonQuery();

                    // Get the identity value
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    journalID = (int)(Int64)cmd.ExecuteScalar();
                }
                connection.Close();
            }
            return journalID;
        }

        /// <summary>
        /// Update the Journal entry
        /// </summary>
        /// <param name="entry"></param>
        public void UpdateDBJournalEntry(JournalEntry entry)
        {
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "UPDATE Journal SET Header=@header, Entry=@entry, ModificationDateTime=@modDateTime WHERE Journal_ID = @journalID";
                    cmd.Parameters.AddWithValue("@journalID", entry.JournalID);
                    cmd.Parameters.AddWithValue("@header", entry.Title);
                    cmd.Parameters.AddWithValue("@entry", entry.Entry);
                    cmd.Parameters.AddWithValue("@modDateTime", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        /// <summary>
        /// Delete a Journal Entry from the database.
        /// </summary>
        /// <param name="entry"></param>
        public void DeleteDBJournalEntry(JournalEntry entry)
        {
            // Delete from the database
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "UPDATE Journal SET DeletionDateTime = @now WHERE Journal_ID = @journalID";
                    cmd.Parameters.AddWithValue("@journalID", entry.JournalID);
                    cmd.Parameters.AddWithValue("@now", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }

            // Delete from memory
            _model.SelectedWorkItem.Journals.Remove(_model.SelectedJournalEntry);
            _model.SelectedJournalEntry = null;
        }

        /// <summary>
        /// Load the application Preferences from the database.
        /// For a list of Preferences, see PreferenceName.cs
        /// </summary>
        private void LoadApplicationPreferences()
        {
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM Setting";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string settingNameStr = (string)reader["Name"];
                            Enum.TryParse(settingNameStr, out PreferenceName preferenceName);
                            string preferenceValue = (string)reader["Value"];
                            string defaultValue = (string)reader["DefaultValue"];
                            string description = (string)reader["Description"];
                            string userCanEditChar = (string)reader["UserCanEdit"];
                            bool userCanEdit = (userCanEditChar.Equals("Y")) ? true : false;

                            _model.AddAppSetting(preferenceName, new Preference(preferenceName, preferenceValue, defaultValue, description, userCanEdit));
                        }
                    }
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Update an Application Preference (in DB and memory).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool UpdateAppPreference(PreferenceName name, string value)
        {
            bool rValue = false;
            int result = -1;

            // Update database
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "UPDATE Setting SET [Value] = @value WHERE [Name]=@name";
                    cmd.Parameters.AddWithValue("@name", name.ToString());
                    cmd.Parameters.AddWithValue("@value", value);
                    result = cmd.ExecuteNonQuery();
                }
                connection.Close();
            }

            if (result > 0)
            {
                // Update in memory
                var settings = _model.GetAppPreferenceCollection();
                settings[name].Value = value;
                rValue = true;
            }
            return rValue;
        }

        /// <summary>
        /// Inserts a WorkItem into the database, returning the rowID of the inserted record.
        /// This method also adds associated DueDate and WorkItemStatus records.
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="addWorkItemStatus"></param>
        /// <returns></returns>
        public int InsertDBWorkItem(WorkItem wi, bool addWorkItemStatus)
        {
            int workItemID = -1;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO WorkItem (TaskTitle, TaskDescription, CreationDateTime) " +
                        "VALUES (@title, @desc, @creation)";
                    cmd.Parameters.AddWithValue("@title", wi.Title);
                    cmd.Parameters.AddWithValue("@desc", wi.TaskDescription);
                    cmd.Parameters.AddWithValue("@creation", wi.CreationDateTime);
                    cmd.ExecuteNonQuery();

                    // Get the identity value (to return)
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    workItemID = (int)(Int64)cmd.ExecuteScalar();
                    wi.Meta.WorkItem_ID = workItemID;

                    if (wi.WorkItemStatusEntry == null)
                        wi.WorkItemStatusEntry = new WorkItemStatusEntry();

                    wi.WorkItemStatusEntry.WorkItemID = workItemID; // <-- here

                    // Save the Due Date
                    if (wi.DueDate > DateTime.MinValue)
                    {
                        wi.Meta.DueDate_ID = InsertDBDueDate(wi, wi.DueDate, "Initial WorkItem created.");
                    }

                    if ((addWorkItemStatus) && (wi.WorkItemStatusEntry.WorkItemStatusEntryID == -1))
                    {
                        Console.WriteLine("InsertDBWorkItem()...InsertDBWorkItemStatusEntry");
                        InsertDBWorkItemStatusEntry(wi.WorkItemStatusEntry);
                    }
                }
                connection.Close();
            }
            return workItemID;
        }

        /// <summary>
        /// Update the WorkItem in the database.
        /// </summary>
        /// <param name="wi"></param>
        /// <returns></returns>
        public void UpdateDBWorkItem(WorkItem wi)
        {
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "UPDATE WorkItem " +
                        "SET TaskTitle = @title, " +
                        "TaskDescription = @desc " +
                        "WHERE WorkItem_ID = @workItemID";
                    cmd.Parameters.AddWithValue("@title", wi.Title);
                    cmd.Parameters.AddWithValue("@desc", wi.TaskDescription);
                    cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                    cmd.ExecuteNonQuery();

                    wi.Meta.WorkItemDBNeedsUpdate = false;
                }
                connection.Close();
            }
        }

        /// <summary>
        /// Inserts a DueDate record for the associated WorkItem, returning the rowID of the inserted record.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="newDueDate"></param>
        /// <param name="changeReason">The reason the user gives for the due date change.</param>
        /// <returns></returns>
        public int InsertDBDueDate(WorkItem workItem, DateTime newDueDate, string changeReason)
        {
            int rowID = -1;
            if (changeReason.Equals(""))
                changeReason = null;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO DueDate (WorkItem_ID, DueDateTime, ChangeReason, CreationDateTime)" +
                        "VALUES (@WorkItemID, @NewDueDate, @ChangeReason, @CreationDateTime)";
                    cmd.Parameters.AddWithValue("@WorkItemID", workItem.Meta.WorkItem_ID);
                    cmd.Parameters.AddWithValue("@NewDueDate", newDueDate);
                    cmd.Parameters.AddWithValue("@ChangeReason", changeReason);
                    cmd.Parameters.AddWithValue("@CreationDateTime", DateTime.Now);
                    cmd.ExecuteNonQuery();

                    // Get the identity value
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    rowID = (int)(Int64)cmd.ExecuteScalar();

                }
                connection.Close();
            }

            _model.FireDueDateChanged(workItem, newDueDate);
            return rowID;
        }

        /// <summary>
        /// Inserts a DueDate record for the associated WorkItem, returning the rowID of the inserted record.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="newDueDate"></param>
        /// <param name="creationDate"></param>
        /// <param name="changeReason"></param>
        /// <returns></returns>
        public int InsertDBDueDate(WorkItem workItem, DateTime newDueDate, DateTime creationDate, string changeReason)
        {
            int rowID = -1;
            if (changeReason.Equals(""))
                changeReason = null;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO DueDate (WorkItem_ID, DueDateTime, ChangeReason, CreationDateTime)" +
                        "VALUES (@WorkItemID, @NewDueDate, @ChangeReason, @CreationDateTime)";
                    cmd.Parameters.AddWithValue("@WorkItemID", workItem.Meta.WorkItem_ID);
                    cmd.Parameters.AddWithValue("@NewDueDate", newDueDate);
                    cmd.Parameters.AddWithValue("@ChangeReason", changeReason);
                    cmd.Parameters.AddWithValue("@CreationDateTime", creationDate);
                    cmd.ExecuteNonQuery();

                    // Get the identity value
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    rowID = (int)(Int64)cmd.ExecuteScalar();

                }
                connection.Close();
            }

            _model.FireDueDateChanged(workItem, newDueDate);
            return rowID;
        }

        /// <summary>
        /// Update the Due Date record.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="newDueDate"></param>
        /// <param name="changeReason"></param>
        /// <returns></returns>
        public int UpdateDBDueDate(WorkItem workItem, DateTime newDueDate, string changeReason)
        {
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "UPDATE DueDate " +
                        "SET DueDateTime = @DueDate, " +
                        "ChangeReason = @ChangeReason, " +
                        "CreationDateTime = @CreateDateTime " +
                        "WHERE DueDate_ID = @DueDateID";
                    cmd.Parameters.AddWithValue("@DueDate", newDueDate);
                    cmd.Parameters.AddWithValue("@ChangeReason", changeReason);
                    cmd.Parameters.AddWithValue("@CreateDateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@DueDateID", workItem.Meta.DueDate_ID);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }

            _model.FireDueDateChanged(workItem, newDueDate);

            return workItem.Meta.DueDate_ID;
        }

    }

}
