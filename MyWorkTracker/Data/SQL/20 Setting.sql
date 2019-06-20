CREATE TABLE Setting ( 
    [Name] VARCHAR (30)  NOT NULL PRIMARY KEY,
    [Value] VARCHAR(500) NOT NULL,
    [DefaultValue] VARCHAR(500),
    [Description] VARCHAR(500) NOT NULL,
    [UserCanEdit] CHAR(1) DEFAULT ('N')
 );

INSERT INTO Setting ([Name], [Value], DefaultValue, [Description]) 
VALUES	('APPLICATION_NAME', 'MyWorkTracker', 'MyWorkTracker', 'The name of the Application'),
		('APPLICATION_VERSION', '0.1.0', '0.1.0', 'The version number of the Application'),
		('DEFAULT_WORKITEM_LENGTH_DAYS', '1', '1', 'The default number of days that should be added to complete a WorkItem.'),
		('DEFAULT_WORKITEM_COB_HOURS', '16', '16', 'The default Due Date Close of Business (COB) Hours.'),
		('DEFAULT_WORKITEM_COB_MINS', '0', '0', 'The default Due Date Close of Business (COB) Minutes.'),
		('DUE_DATE_SET_WINDOW_MINUTES', '1', '1', 'If the Due Date is altered within this time period of setting it, don''t record it as a change.')
		;