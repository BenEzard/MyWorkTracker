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

            // --- TODO start test data
            //LoadNotebookTopics();
            AddNotebookTopic(new NotebookTopic(1,-1,"New Notebook",""));
            AddNotebookTopic(new NotebookTopic(2,-1,"New Notebook",""));
            // --- end test data
        }

        /// <summary>
        /// Add a CheckListItem to a Work Item.
        /// Note that it cannot have the same text as another item
        /// </summary>
        /// <param name="item"></param>
        public void AddCheckListItem(CheckListItem item)
        {
            if (item == null) throw new ArgumentNullException("item");

            // Check to see if the text is identical to an existing CheckListItem on the WorkItem.
            bool canAdd = true;
            foreach (CheckListItem i in _model.CheckListItems)
            {
                if (i.Task.Equals(item.Task)) {
                    canAdd = false;
                    break;
                }
            }

            if (canAdd)
            {
                // TODO change to a call.
                //_model.CheckListItems.Add(item);
                InsertDBWorkItemCheckListItem(item);
                _model.FireWorkItemCheckListItemAdded(_model.SelectedWorkItem, item);

            }
        }

        public int InsertDBWorkItemCheckListItem(CheckListItem cli)
        {
            // Insert the record
            int rowID = -1;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO WorkItemCheckList (WorkItem_ID, TaskText, TaskDetails, DueDateTime, CreationDateTime)" +
                        "VALUES (@WorkItemID, @Task, @Details, @DueDateTime, @CreationDateTime)";
                    cmd.Parameters.AddWithValue("@WorkItemID", cli.WorkItemID);
                    cmd.Parameters.AddWithValue("@Task", cli.Task);
                    cmd.Parameters.AddWithValue("@Details", cli.Details);
                    cmd.Parameters.AddWithValue("@DueDateTime", cli.DueDateTime);
                    cmd.Parameters.AddWithValue("@CreationDateTime", DateTime.Now);
                    cmd.ExecuteNonQuery();

                    // Get the identity value
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    rowID = (int)(Int64)cmd.ExecuteScalar();
                    cli.CreationDateTime = DateTime.Now;
                    cli.DatabaseID = rowID;

                }
                connection.Close();
            }

            // Pull the record data to get all the values that are initialised with DEFAULT constraints.
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    string sql = "SELECT * FROM vwWorkItemCheckList_ConstraintData" +
                        " WHERE WorkItem_ID = @workItemID";
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@workItemID", rowID);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cli.Indent = Convert.ToInt32(reader["Indent"]);
                            cli.SortOrder = Convert.ToInt32(reader["ItemSortOrder"]);
                            DateTime creationDateTime = Convert.ToDateTime(reader["CreationDateTime"].ToString());
                        }
                    }
                    connection.Close();
                }
            }
            return rowID;
        }

        public void AddNotebookTopic(NotebookTopic topic)
        {
            _model.NotebookTopics.Add(topic);
        }

        /// <summary>
        /// Load the Notebook Topic (for the ViewTree)
        /// </summary>
        public void LoadNotebookTopics()
        {
        /*    using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM vwNotebookTopic";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Get the database results
                            int id = Convert.ToInt32(reader["NotebookTopic_ID"]);
                            string stringID = Convert.ToString(id).PadLeft(3, '0');
                            int parentID = -1;
                            if (!(reader["ParentTopic_ID"] is DBNull))
                            {
                                parentID = Convert.ToInt32(reader["ParentTopic_ID"]);
                            }
                            string topic = (string)reader["Topic"];
                            string treePath = (string)reader["TreePath"];

                            NotebookTopic topicObj = null;
                            if (parentID == -1) // If there is no parentID, then add it to the collection
                            {
                                _model.NotebookTopics.Add(stringID, new NotebookTopic(id, parentID, topic, treePath));
                                Console.WriteLine($"A: Adding {stringID} {topic}");
                            } else // If there is a parent ID...
                            {
                                string[] listofTreePathPortions = StringMethods.SplitStringByLength(treePath, 3); // Get the list of strings in the TreePath.

                                // Navigate down the TreePath
                                foreach (string keyPortion in listofTreePathPortions)
                                {
                                    if (topicObj == null)
                                    {
                                        Console.WriteLine($"1: Navigating down {keyPortion}");
                                        topicObj = _model.NotebookTopics[keyPortion];
                                    }
                                    else
                                    {
                                        Console.WriteLine($"2: Navigating down {keyPortion}");
                                        if (topicObj.HasKey(keyPortion))
                                            topicObj = topicObj.Topics[keyPortion];
/*                                        else
                                        {
                                            topicObj.Add(stringID, new NotebookTopic(id, parentID, topic, treePath));
                                            Console.WriteLine($"B: Adding {stringID} {topic}");
                                        }*/
                               /*     }
                                }
                                if (topicObj != null)
                                {
                                    string key = Convert.ToString(topicObj.DatabaseID).PadLeft(3, '0');
                                    Console.WriteLine($"C: Adding {key} {topic}");
                                    _model.NotebookTopics.Add(key, topicObj);
                                }

                            }

                        }
                    }
                    connection.Close();
                }
            }*/
        }

        /// <summary>
        /// Working here
        /// </summary>
        /// <param name="topicString"></param>
        public void AddNotebookTopics(string topicString)
        {
            if (topicString.Length > 0)
            {
                string[] topicsToAdd = topicString.Split('>');
                string[] valuesToAdd = new string[topicsToAdd.Length];
                for (int i = 0; i < topicsToAdd.Length; i++)
                {
                    valuesToAdd[i] = topicsToAdd[i].Trim();
                }
            }

            // SelectedNode
        }

        /// <summary>
        /// Load the Checklist for the specified WorkItem.
        /// Note that this isn't a Checklist *attached* to the WorkItem, but is stored in the main MWTModel.
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="loadDeleted"></param>
        internal void LoadWorkItemCheckLists(WorkItem wi, bool loadDeleted=false)
        {
            // --- Reset values ---------------------------------------------
            _model.CheckListItems.Clear();
            _model.ChecklistsLoadedForWorkItemID = wi.Meta.WorkItem_ID;
            _model.CheckListCompletedCount = 0;
            _model.CheckListOutstandingCount = 0;
            _model.CheckListOverdueCount = 0;

            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    string sql = "SELECT * FROM vwWorkItemCheckList" +
                        " WHERE WorkItem_ID = @workItemID";
                    if (loadDeleted == false)
                        sql += " AND DeletionDateTime IS NULL";
                    sql += " ORDER BY ItemSortOrder";
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CheckListItem cli = null;

                            int workItemCheckListID = Convert.ToInt32(reader["WorkItemCheckList_ID"]);
                            int workItemID = Convert.ToInt32(reader["WorkItem_ID"]);
                            string taskText = (string)reader["TaskText"];
                            int indent = Convert.ToInt32(reader["Indent"]);

                            string details = null;
                            if (reader["TaskDetails"] != DBNull.Value) {
                                details = (string)reader["TaskDetails"];
                            }

                            DateTime? dueDateTime = null;
                            if (reader["DueDateTime"] != DBNull.Value)
                                dueDateTime = Convert.ToDateTime(reader["DueDateTime"].ToString());

                            DateTime? completionDateTime = null;
                            if (reader["CompletionDateTime"] != DBNull.Value)
                            {
                                completionDateTime = Convert.ToDateTime(reader["CompletionDateTime"].ToString());
                            }

                            int sortOrder = Convert.ToInt32(reader["ItemSortOrder"]);
                            DateTime creationDateTime = Convert.ToDateTime(reader["CreationDateTime"].ToString());

                            DateTime? modifDateTime = null;
                            if (reader["ModificationDateTime"] != DBNull.Value)
                            {
                                modifDateTime = Convert.ToDateTime(reader["ModificationDateTime"].ToString());
                            }

                            if (loadDeleted == false)
                            {
                                DateTime? deleteDateTime = null;
                                if (reader["DeletionDateTime"] != DBNull.Value)
                                {
                                    deleteDateTime = Convert.ToDateTime(reader["DeletionDateTime"].ToString());
                                }
                                cli = new CheckListItem(workItemCheckListID, workItemID, taskText, details, indent, dueDateTime, completionDateTime, sortOrder, creationDateTime, modifDateTime, deleteDateTime);
                            }
                            else
                                cli = new CheckListItem(workItemCheckListID, workItemID, taskText, details, indent, dueDateTime, completionDateTime, sortOrder, creationDateTime, modifDateTime, null);

                            // --- Calculate totals -----------------------------------------
                            if (reader["CompletionDateTime"] != DBNull.Value)
                            {
                                ++_model.CheckListCompletedCount;
                            } 
                            else
                            {
                                if ((dueDateTime.HasValue) && (dueDateTime.Value < DateTime.Now))
                                    ++_model.CheckListOverdueCount;
                                else
                                    ++_model.CheckListOutstandingCount;
                            }
                            _model.FireWorkItemCheckListItemAdded(wi, cli);
                        }
                    }
                    connection.Close();
                }
            }
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

     /*   internal void UpdateDBWorkItemCheckListItem()
        {
            if (_model.SelectedCheckListItem == null)
            {
                _model.WorkItemCheckListDBNeedsUpdating = false;
                return;
            }

            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();

                    cmd.CommandText = "UPDATE WorkItemCheckList" +
                        " SET TaskText = @TaskText," +
                        " TaskDetails = @TaskDetails," +
                        " DueDateTime = @DueDate," +
                        " ModificationDateTime = @modDateTime" +
                        " WHERE WorkItemCheckList_ID = @dbID";
                    cmd.Parameters.AddWithValue("@TaskText", _model.SelectedCheckListItem.Task);
                    cmd.Parameters.AddWithValue("@TaskDetails", _model.SelectedCheckListItem.Details);
                    cmd.Parameters.AddWithValue("@DueDate", _model.SelectedCheckListItem.DueDateTime);
                    cmd.Parameters.AddWithValue("@modDateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@dbID", _model.SelectedCheckListItem.DatabaseID);
                    cmd.ExecuteNonQuery();

                    _model.WorkItemCheckListDBNeedsUpdating = false;
                }
            }
        }*/

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
            // When this is called, first clear any content of the WorkItem collections.
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

        internal void SetCheckListMode(DataEntryMode mode)
        {
            _model.CheckListItemMode = mode;
        }

        /// <summary>
        /// Move a CheckListItem's SortOrder position.
        /// </summary>
        /// <param name="itemToMove"></param>
        /// <param name="itemToReplace"></param>
        /// <param name="moveUp"></param>
        internal void MoveCheckListItem(CheckListItem itemToMove, CheckListItem itemToReplace, bool moveUp)
        {
            if (moveUp)
            {
                UpdateDBCheckListItem(itemToMove, -10);
                UpdateDBCheckListItem(itemToReplace, 10);
            }
            else
            {
                UpdateDBCheckListItem(itemToReplace, -10);
                UpdateDBCheckListItem(itemToMove, 10);
            }

            _model.FireCheckListItemMoved(itemToMove, moveUp);
        }

        /// <summary>
        /// Update the SortOrder of a CheckListItem.
        /// </summary>
        /// <param name="cli"></param>
        /// <param name="sortOrderChange"></param>
        internal void UpdateDBCheckListItem(CheckListItem cli, int sortOrderChange)
        {
            DateTime modDate = DateTime.Now;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    string sql = "UPDATE WorkItemCheckList" +
                        " SET TaskText = @TaskText," +
                        " TaskDetails = @TaskDetails," +
                        " DueDateTime = @DueDate," +
                        " ModificationDateTime = @modDateTime," +
                        " CompletionDateTime = @compDateTime";

                    if (sortOrderChange != 0)
                        sql += ", ItemSortOrder = ItemSortOrder + @newPosition ";
                       
                    sql += " WHERE WorkItemCheckList_ID = @dbID";

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@TaskText", cli.Task);
                    cmd.Parameters.AddWithValue("@TaskDetails", cli.Details);
                    cmd.Parameters.AddWithValue("@DueDate", cli.DueDateTime);
                    cmd.Parameters.AddWithValue("@modDateTime", modDate);
                    cmd.Parameters.AddWithValue("@compDateTime", cli.CompletionDateTime);
                    cmd.Parameters.AddWithValue("@dbID", cli.DatabaseID);
                    if (sortOrderChange != 0)
                        cmd.Parameters.AddWithValue("@newPosition", sortOrderChange);

                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
            cli.ModificationDateTime = modDate;
            _model.WorkItemCheckListDBNeedsUpdating = false;
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
