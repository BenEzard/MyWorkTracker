CREATE TABLE Setting ( 
    [Name] VARCHAR (30)  NOT NULL PRIMARY KEY,
    [Value] VARCHAR(500) NOT NULL,
    [DefaultValue] VARCHAR(500),
    [Description] VARCHAR(500) NOT NULL,
    [UserCanEdit] CHAR(1) DEFAULT ('N')
 );

INSERT INTO Setting ([Name], [Value], DefaultValue, [Description], UserCanEdit) 
VALUES	/* Application Values - Non configurable */
		('APPLICATION_NAME', 'MyWorkTracker', 'MyWorkTracker', 'The name of the Application', 'N'),
		('APPLICATION_VERSION', '0.3.1', '0.3.1', 'The version number of the Application', 'N'),

		/* Application Values - Configurable */
		('APPLICATION_WINDOW_COORDS', '100,0,750,750', '100,0,750,750', 'The window''s location and size (left, top, width, height)', 'Y'),
		('SAVE_WINDOW_COORDS_ON_EXIT', '1', '1', 'Should the window''s location and size be saved when the application exits?', 'Y'),
		('LOAD_STALE_DAYS', '100', '100', 'Load Work Items that were completed this many days ago.', 'Y'),

		/* Due Date variables */
		('DEFAULT_WORKITEM_LENGTH_DAYS', '35', '5', 'The default number of days that should be added to complete a WorkItem.', 'Y'),
		('DEFAULT_WORKITEM_COB_HOURS', '16', '16', 'The default Due Date Close of Business (COB) Hours (24hr clock).', 'Y'),
		('DEFAULT_WORKITEM_COB_MINS', '0', '0', 'The default Due Date Close of Business (COB) Minutes.', 'Y'),
		('DUE_DATE_SET_WINDOW_MINUTES', '10', '10', 'If the Due Date is altered within this time period of setting it, don''t record it as a change.', 'Y'),
		('DUE_DATE_CAN_BE_WEEKENDS', '0', '0', 'Should a Due Date on a Saturday or Sunday be considered legitimate?', 'Y'),

		/* Work Item Status */
		('STATUS_ACTIVE_TO_COMPLETE_PCN', '75', '75', 'When a WorkItem is moved from active-to-complete, set the Completion percent.', 'Y'),

		/* Journals */
		('CONFIRM_JOURNAL_DELETION', '1', '1', 'Should a Journal Entry deletion be confirmed?', 'Y'),
		('JOURNAL_ORDERING', 'bottom', 'bottom', 'Should new Journal Entries appear at bottom or top of list?', 'Y'),

		('DELETE_OPTION', 'logically (leave trace)', 'logically (leave trace)', 'Should deletion be logical or physical. Options logically (leave trace), physically (permanent)', 'Y'),

		/* Backup options */
		('DATA_EXPORT_LAST_DONE', '2019-09-13', '2019-09-13', 'When the backup was last done (a date)', 'N'),
		('DATA_EXPORT_LAST_DIRECTORY', 'C:\', '', 'The directory where the last export file was chosen from', 'N'),
		('DATA_IMPORT_LAST_DIRECTORY', 'C:\', '', 'The directory where the last import file was chosen from', 'N'),
		('DATA_EXPORT_AUTOMATICALLY', '0', '1', 'Should a backup be done automatically? (1 or 0)', 'Y'),
		('DATA_EXPORT_PERIOD_DAYS', '1', '1', 'How often should the backup be done (in days)?', 'Y'),
		('DATA_EXPORT_WORKITEM_SELECTION', 'all', 'all', 'Which WorkItems should be backed up? Options are all, active only and active plus closed', 'Y'),
		('DATA_EXPORT_DAYS_STALE', '100', '100', 'When exporting Closed Work Items, take this many days?', 'Y'),
		('DATA_EXPORT_DAYS_STALE_DEFAULT', '9999', '9999', 'If no value is selected for DATA_EXPORT_DAYS_STALE, what should it be defaulted to?', 'Y'),
		('DATA_EXPORT_DUEDATE_SELECTION', 'full', 'full', 'What Due Date info should be backed up? Options are "full" and "latest"', 'Y'),
		('DATA_EXPORT_STATUS_SELECTION', 'full', 'full', 'What Status info should be backed up? Options are "full" and "latest"', 'Y'),
		('DATA_EXPORT_INCLUDE_DELETED', '0', '0', 'Should deleted Work Items be included in the backup? Options are 0 or 1', 'Y'),
		('DATA_EXPORT_INCLUDE_PREFERENCES', '0', '0', 'Should Preferences be included in the backup? Options are 0 or 1', 'Y'),
		('DATA_EXPORT_SAVE_TO_LOCATION', 'D:\Work\MyWorkTracker Backup', 'D:\Work\MyWorkTracker Backup', 'Location where backup files should be placed.', 'Y'),
		('DATA_EXPORT_SAME_DAY_OVERWRITE', '1', '1', 'Overwrite the backup if from the same day. Options are 0 or 1', 'Y'),
		('DATA_EXPORT_COPY_LOCATION', 'D:\Work\MyWorkTracker Copy', 'D:\Work\MyWorkTracker Copy', 'Location where backup files should be copied (duplicated) to.', 'Y'),
		('DATA_EXPORT_AVAILABLE_VERSIONS', '0.3.1,0.3.0', '0.3.1,0.3.0', 'Available Export options (comma separated)', 'N'),
		('DATA_EXPORT_DEFAULT_VERSION', '0.3.1', '0.3.1', 'Default Export version', 'Y')
		;