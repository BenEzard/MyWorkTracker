using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

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

            LoadApplicationSettingsFromDB();
            LoadWorkItemStatusesFromDB();

            LoadWorkItemsFromDB();
        }

        /// <summary>
        /// Create a new WorkItem.
        /// The DueDate is set based on default Settings.
        /// </summary>
        public void CreateNewWorkItem()
        {
            var wi = new WorkItem();

            // Set the due date based on the default.
            // Calculate the due date
            //   1. This will be the current datetime + the default amount
            DateTime dueDate = DateTime.Now.AddDays(Double.Parse(_model.GetAppSettingValue(SettingName.DEFAULT_WORKITEM_LENGTH_DAYS)));
            //   2. + end of the work day
            dueDate = new DateTime(dueDate.Year, dueDate.Month, dueDate.Day,
                int.Parse(_model.GetAppSettingValue(SettingName.DEFAULT_WORKITEM_COB_HOURS)),
                int.Parse(_model.GetAppSettingValue(SettingName.DEFAULT_WORKITEM_COB_MINS)), 
                0);

            if (_model.GetAppSettingValue(SettingName.DUE_DATE_CAN_BE_WEEKENDS).Equals("0"))
            {
                if (dueDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    dueDate = dueDate.AddDays(2);
                }
                else if (dueDate.DayOfWeek == DayOfWeek.Sunday) {
                    dueDate = dueDate.AddDays(1);
                }
            }
            wi.DueDate = dueDate;

            // Get the default active Status (note due to DB constraints there can only be one).
            wi.Status = GetWorkItemStatuses(true, true).ToArray()[0].Status;

            _model.FireCreateNewWorkItem(wi);
        }

        public void AddJournalEntry(WorkItem wi, JournalEntry entry)
        {
            wi.Journals.Add(entry);
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

        public void SetWorkItemStatus(WorkItem wi, WorkItemStatus wis)
        {
            wi.Status = wis.Status;
            wi.Meta.WorkItemStatusNeedsUpdate = true;
            _model.FireWorkItemStatusChange(wi, wis);
        }

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

        public void LoadJournalsFromDB(WorkItem wi)
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
        /// Load the WorkItems from the database.
        /// </summary>
        private void LoadWorkItemsFromDB()
        {
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM vwWorkItem";

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WorkItem wi = new WorkItem();
                            wi.Meta.WorkItem_ID = Convert.ToInt32(reader["WorkItem_ID"]);
                            wi.Title = (string)reader["TaskTitle"];
                            if (reader["TaskDescription"].GetType() != typeof(DBNull))
                                wi.TaskDescription = (string)reader["TaskDescription"];
                            wi.Completed = Convert.ToInt32(reader["Complete"]);
                            wi.Status = reader["StatusLabel"].ToString();
                            wi.DueDate = DateTime.Parse(reader["DueDateTime"].ToString());
                            wi.Meta.DueDateUpdateDateTime = DateTime.Parse(reader["DueDateCreationDateTime"].ToString());
                            _model.AddWorkItem(wi, false, false);
                        }
                    }
                    connection.Close();
                }
            }
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
        /// Inserts a DB record for a WorkItemStatus.
        /// </summary>
        /// <param name="wi"></param>
        /// <param name="status"></param>
        /// <returns>Returns the WorkItemStatus ID on insert, or -1</returns>
        public int InsertDBWorkItemStatus(WorkItem wi, string status)
        {
            WorkItemStatus wis = GetWorkItemStatus(status);
            int workItemStatusID = -1;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO WorkItemStatus (WorkItem_ID, Status_ID, CreationDateTime) " +
                        "VALUES (@workItemID, @statusID, @creation)";
                    cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                    cmd.Parameters.AddWithValue("@statusID", wis.WorkItemStatusID);
                    cmd.Parameters.AddWithValue("@creation", DateMethods.FormatSQLDateTime(DateTime.Now));
                    cmd.ExecuteNonQuery();

                    // Get the identity value
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    workItemStatusID = (int)(Int64)cmd.ExecuteScalar();
                    wi.Meta.WorkItem_ID = workItemStatusID;

                    wi.Meta.WorkItemStatusNeedsUpdate = false;
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
                    Console.WriteLine($"Updating {entry.JournalID}, {entry.Title}");

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
        /// Load the application Settings from the database.
        /// </summary>
        private void LoadApplicationSettingsFromDB()
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
                            Enum.TryParse(settingNameStr, out SettingName settingName);
                            string settingValue = (string)reader["Value"];
                            string defaultValue = (string)reader["DefaultValue"];
                            string description = (string)reader["Description"];
                            string userCanEditChar = (string)reader["UserCanEdit"];
                            bool userCanEdit = (userCanEditChar.Equals("Y")) ? true : false;

                            _model.AddAppSetting(settingName, new Setting(settingName, settingValue, defaultValue, description, userCanEdit));
                        }
                    }
                    connection.Close();
                }
            }
        }

        public bool UpdateApplicationSettingDB(SettingName name, string value)
        {
            bool rValue = false;
            int result = -1;
            // Update database
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "UPDATE Setting SET [Value] = @value WHERE [Name]=@name AND UserCanEdit='Y'";
                    cmd.Parameters.AddWithValue("@name", name.ToString());
                    cmd.Parameters.AddWithValue("@value", value);
                    result = cmd.ExecuteNonQuery();
                }
                connection.Close();
            }

            if (result > 0)
            {
                // Update in memory
                var settings = _model.GetAppSettingCollection();
                settings[name].Value = value;
                rValue = true;
            }
            return rValue;
        }

        /// <summary>
        /// Inserts a WorkTask into the database, returning the rowID of the inserted record.
        /// This does not add an associated DueDate or any Log records.
        /// </summary>
        /// <param name="wi"></param>
        /// <returns></returns>
        public int InsertDBWorkItem(WorkItem wi)
        {
            int workItemID = -1;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO WorkItem (TaskTitle, TaskDescription, Complete, CreationDateTime) " +
                        "VALUES (@title, @desc, @completed, @creation)";
                    cmd.Parameters.AddWithValue("@title", wi.Title);
                    cmd.Parameters.AddWithValue("@desc", wi.TaskDescription);
                    cmd.Parameters.AddWithValue("@completed", wi.Completed);
                    cmd.Parameters.AddWithValue("@creation", wi.CreateDateTime);
                    cmd.ExecuteNonQuery();

                    // Get the identity value (to return)
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    workItemID = (int)(Int64)cmd.ExecuteScalar();
                    wi.Meta.WorkItem_ID = workItemID;

                    // Save the Due Date
                    int dueDateID = InsertDBDueDate(wi, wi.DueDate, "Initial WorkItem created.");
                    wi.Meta.DueDate_ID = dueDateID;

                    // Save the Status
                    InsertDBWorkItemStatus(wi, wi.Status);
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
        public int UpdateDBWorkItem(WorkItem wi)
        {
            int workItemID = -1;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "UPDATE WorkItem " +
                        "SET TaskTitle = @title, " +
                        "TaskDescription = @desc, " +
                        "Complete = @completed " +
                        "WHERE WorkItem_ID = @workItemID";
                    cmd.Parameters.AddWithValue("@title", wi.Title);
                    cmd.Parameters.AddWithValue("@desc", wi.TaskDescription);
                    cmd.Parameters.AddWithValue("@completed", wi.Completed);
                    cmd.Parameters.AddWithValue("@workItemID", wi.Meta.WorkItem_ID);
                    cmd.ExecuteNonQuery();

                    wi.Meta.WorkItemDBNeedsUpdate = false;
                }
                connection.Close();
            }
            return workItemID;
        }

        /// <summary>
        /// Inserts a DueDate record for the associated WorkItem, returning the rowID of the inserted record.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="newDueDate"></param>
        /// <param name="changeReason"></param>
        /// <returns></returns>
        public int InsertDBDueDate(WorkItem workItem, DateTime newDueDate, string changeReason)
        {
            int rowID = -1;
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "INSERT INTO DueDate (WorkItem_ID, DueDateTime, ChangeReason, CreationDateTime)" +
                        "VALUES (@WorkItemID, @NewDueDate, @ChangeReason, @CreationDateTime)";
                    cmd.Parameters.AddWithValue("@WorkItemID", workItem.Meta.WorkItem_ID);
                    cmd.Parameters.AddWithValue("@NewDueDate", DateMethods.FormatSQLDateTime(newDueDate));
                    cmd.Parameters.AddWithValue("@ChangeReason", changeReason);
                    cmd.Parameters.AddWithValue("@CreationDateTime", DateMethods.FormatSQLDateTime(DateTime.Now));
                    cmd.ExecuteNonQuery();

                    // Get the identity value
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    rowID = (int)(Int64)cmd.ExecuteScalar();

                }
                connection.Close();
            }
            _model.FireDueDateChanged(workItem);
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
                    cmd.Parameters.AddWithValue("@CreateDateTime", DateMethods.FormatSQLDateTime(DateTime.Now));
                    cmd.Parameters.AddWithValue("@DueDateID", workItem.Meta.DueDate_ID);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }

            workItem.Meta.DueDateUpdateDateTime = DateTime.Now;
            _model.FireDueDateChanged(workItem);

            return workItem.Meta.DueDate_ID;
        }

    }

}
