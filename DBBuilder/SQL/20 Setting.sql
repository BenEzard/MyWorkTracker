CREATE TABLE Setting ( 
    [Name] VARCHAR (30)  NOT NULL PRIMARY KEY,
    [Value] VARCHAR(500) NOT NULL,
    [DefaultValue] VARCHAR(500),
    [Description] VARCHAR(500) NOT NULL,
    [UserCanEdit] CHAR(1) DEFAULT ('N')
 );

INSERT INTO Setting ([Name], [Value], DefaultValue, [Description], UserCanEdit) 
VALUES	('APPLICATION_NAME', 'MyWorkTracker', 'MyWorkTracker', 'The name of the Application', 'N'),
		('APPLICATION_VERSION', '0.2.0', '0.2.0', 'The version number of the Application', 'N'),
		('DEFAULT_WORKITEM_LENGTH_DAYS', '1', '1', 'The default number of days that should be added to complete a WorkItem.', 'Y'),
		('DEFAULT_WORKITEM_COB_HOURS', '16', '16', 'The default Due Date Close of Business (COB) Hours (24hr clock).', 'Y'),
		('DEFAULT_WORKITEM_COB_MINS', '0', '0', 'The default Due Date Close of Business (COB) Minutes.', 'Y'),
		('DUE_DATE_SET_WINDOW_MINUTES', '1', '1', 'If the Due Date is altered within this time period of setting it, don''t record it as a change.', 'Y'),
		('DUE_DATE_CAN_BE_WEEKENDS', '0', '0', 'Should a Due Date on a Saturday or Sunday be considered legitimate?', 'Y'),
		('CONFIRM_JOURNAL_DELETION', '1', '1', 'Should a Journal Entry deletion be confirmed?', 'Y')
		;