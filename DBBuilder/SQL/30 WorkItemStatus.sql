CREATE TABLE WorkItemStatus (
    WorkItemStatus_ID INTEGER  PRIMARY KEY AUTOINCREMENT,
    WorkItem_ID          INTEGER  REFERENCES WorkItem (WorkItem_ID),
    Status_ID          INTEGER  REFERENCES [Status] (Status_ID),
    CreationDateTime  DATETIME NOT NULL
);
