﻿using MyWorkTracker.Data;
using System;
using System.Data.SQLite;

namespace MyWorkTracker.Code
{
    public class MWTController
    {
        private const string dbConnectionString = "data source=" + MWTModel.DatabaseFile;

        MWTModel _model = null;

        public MWTController(MWTModel model)
        {
            _model = model;

            //new DBInstaller(MWTModel.DatabaseFile, dbConnectionString);

            LoadApplicationSettingsFromDB(true);
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
            DateTime dueDate = DateTime.Now.AddDays(Double.Parse(_model.GetAppSetting(SettingName.DEFAULT_WORKITEM_LENGTH_DAYS)));
            //   2. + end of the work day
            dueDate = new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, 
                Int32.Parse(_model.GetAppSetting(SettingName.DEFAULT_WORKITEM_COB_HOURS)), 
                Int32.Parse(_model.GetAppSetting(SettingName.DEFAULT_WORKITEM_COB_MINS)), 
                0);
            wi.DueDate = dueDate;

            _model.FireCreateNewWorkItem(wi);
        }

        public MWTModel GetMWTModel()
        {
            return _model;
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
                            WorkItem t = new WorkItem();
                            t.Meta.WorkItem_ID = Convert.ToInt32(reader["WorkItem_ID"]);
                            t.Title = (string)reader["TaskTitle"];
                            if (reader["TaskDescription"].GetType() != typeof(DBNull))
                                t.TaskDescription = (string)reader["TaskDescription"];
                            t.Completed = Convert.ToInt32(reader["Complete"]);
                            t.Status = reader["StatusLabel"].ToString();
                            t.DueDate = DateTime.Parse(reader["DueDateTime"].ToString());
                            t.Meta.DueDateUpdateDateTime = DateTime.Parse(reader["DueDateCreationDateTime"].ToString());
                            _model.AddWorkItem(t, false, false);
                        }
                    }
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 
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
                            _model.AddWorkItemStatus(new WorkItemStatus(id, status));
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
            Console.WriteLine($"Trying to update status record for {wi.Title}");
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
                    cmd.Parameters.AddWithValue("@creation", FormatSQLDateTime(DateTime.Now));
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
        /// Load the application Settings from the database.
        /// </summary>
        /// <param name="minimalLoad">If minimalLoad=true, only load the name/value columns (and not the other fields in the table)</param>
        private void LoadApplicationSettingsFromDB(bool minimalLoad)
        {
            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();
                    cmd.CommandText = "SELECT * FROM Setting";

                    if (minimalLoad)
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Enum.TryParse((string)reader["Name"], out SettingName settingName);
                                string settingValue = (string)reader["Value"];
                                _model.AddAppSetting(settingName, settingValue);
                            }
                        }
                    }

                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Inserts a WorkTask into the database, returning the rowID of the inserted record.
        /// This does not add an associated DueDate or any Log records.
        /// </summary>
        /// <param name="wi"></param>
        /// <returns></returns>
        public int InsertDBWorkItem(WorkItem wi)
        {
            Console.WriteLine($"Trying to insert {wi.Title}");
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
            Console.WriteLine($"Trying to update record for {wi.Title}");
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
                    cmd.Parameters.AddWithValue("@NewDueDate", FormatSQLDateTime(newDueDate));
                    cmd.Parameters.AddWithValue("@ChangeReason", changeReason);
                    cmd.Parameters.AddWithValue("@CreationDateTime", FormatSQLDateTime(DateTime.Now));
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
                    cmd.Parameters.AddWithValue("@CreateDateTime", FormatSQLDateTime(DateTime.Now));
                    cmd.Parameters.AddWithValue("@DueDateID", workItem.Meta.DueDate_ID);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }

            workItem.Meta.DueDateUpdateDateTime = DateTime.Now;
            _model.FireDueDateChanged(workItem);

            return workItem.Meta.DueDate_ID;
        }

        /// <summary>
        /// 
        /// TODO: this should be replaced by something else pre-existing 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string FormatSQLDateTime(DateTime dt)
        {
            string rValue = $"{dt.Year}-";
            if (dt.Month < 10)
                rValue += "0";
            rValue += $"{dt.Month}-";
            if (dt.Day < 10)
                rValue += "0";
            rValue += $"{dt.Day} ";
            if (dt.Hour < 10)
                rValue += "0";
            rValue += $"{dt.Hour}:";
            if (dt.Minute < 10)
                rValue += "0";
            rValue += $"{dt.Minute}:";
            if (dt.Second < 10)
                rValue += "0";
            rValue += $"{dt.Second}.000";

            return rValue;
        }

        public static string GenerateDateDifferenceLabel(DateTime date1, DateTime date2, bool shortVersion)
        {
            string rValue = "";
            bool needComma = false;
            bool valueOutputted = false;    // This is set to true if shortVersion==true and some time-related value has been added to
                                            // the string

            TimeSpan? diffBetweenNowAndDue = date2 - date1;

            if (diffBetweenNowAndDue.HasValue)
            {
                // Check to see if it's future-dated or overdue
                if (diffBetweenNowAndDue.Value.TotalSeconds == 0)
                {
                    rValue = "(Due now)";
                    return rValue;
                }
                else if (diffBetweenNowAndDue.Value.TotalSeconds > 0)
                    rValue = "(Due in ";
                else
                {
                    rValue = "(Overdue by ";
                    diffBetweenNowAndDue = diffBetweenNowAndDue.Value.Negate();
                }

                if (diffBetweenNowAndDue.Value.Days != 0)
                {
                    if (shortVersion)
                        valueOutputted = true;

                    rValue += $"{diffBetweenNowAndDue.Value.Days} day";
                    needComma = true;
                    if (diffBetweenNowAndDue.Value.Days > 1)
                        rValue += "s";
                }

                if ((diffBetweenNowAndDue.Value.Hours != 0) && (valueOutputted == false))
                {
                    if (shortVersion)
                        valueOutputted = true;

                    if (needComma)
                        rValue += ", ";
                    rValue += $"{diffBetweenNowAndDue.Value.Hours} hour";
                    if (diffBetweenNowAndDue.Value.Hours > 1)
                        rValue += "s";
                }

                if ((diffBetweenNowAndDue.Value.Minutes != 0) && (valueOutputted == false))
                {
                    if (shortVersion)
                        valueOutputted = true;

                    if (needComma)
                        rValue += ", ";
                    rValue += $"{diffBetweenNowAndDue.Value.Minutes} minute";
                    if (diffBetweenNowAndDue.Value.Minutes > 1)
                        rValue += "s";
                }
            }
            rValue += ")";

            return rValue;
        }

    }

}