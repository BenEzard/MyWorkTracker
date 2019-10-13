CREATE TABLE WorkItemStatus (
    WorkItemStatus_ID INTEGER  PRIMARY KEY AUTOINCREMENT,
    WorkItem_ID          INTEGER  REFERENCES WorkItem (WorkItem_ID),
    Status_ID          INTEGER  REFERENCES [Status] (Status_ID),
	CompletionAmount INTEGER NOT NULL DEFAULT 0, 
    CreationDateTime  DATETIME NOT NULL DEFAULT(CURRENT_TIMESTAMP),
	ModificationDateTime DATETIME NULL,
	DeletionDateTime DATETIME NULL 
);

