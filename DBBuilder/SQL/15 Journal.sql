CREATE TABLE Journal ( 
    Journal_ID INTEGER       PRIMARY KEY AUTOINCREMENT, 
    WorkItem_ID INTEGER       REFERENCES WorkItem (WorkItem_ID), 
    Header VARCHAR (500) NULL,
    [Entry] VARCHAR (5000) NOT NULL,
    CreationDateTime DATETIME NOT NULL DEFAULT(CURRENT_TIMESTAMP),
	ModificationDateTime DATETIME NULL,
	DeletionDateTime DATETIME NULL
);

