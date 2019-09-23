CREATE TABLE WorkItemStatus (
    WorkItemStatus_ID INTEGER  PRIMARY KEY AUTOINCREMENT,
    WorkItem_ID          INTEGER  REFERENCES WorkItem (WorkItem_ID),
    Status_ID          INTEGER  REFERENCES [Status] (Status_ID),
    CreationDateTime  DATETIME  NOT NULL DEFAULT(CURRENT_TIMESTAMP),
	DeletionDateTime DATETIME NULL 
);

INSERT INTO WorkItemStatus (WorkItem_ID, Status_ID) VALUES
	(1, 1);

INSERT INTO WorkItemStatus (WorkItem_ID, Status_ID, CreationDateTime) VALUES
	(2, 3, '2019-05-20 11:00:00.000');