CREATE TABLE WorkItemCheckList (
    WorkItemCheckList_ID INTEGER       PRIMARY KEY AUTOINCREMENT,
    WorkItem_ID          INTEGER       REFERENCES WorkItem (WorkItem_ID) 
                                       NOT NULL,
    TaskText             VARCHAR (100) NOT NULL,
    TaskDetails          VARCHAR (500),
	Indent				 INT DEFAULT (0),
    DueDateTime          DATETIME,
    CompletionDateTime   DATETIME,
    ItemSortOrder        INTEGER,
    CreationDateTime     DATETIME,
    ModificationDateTime DATETIME,
    DeletionDateTime     DATETIME
);

CREATE TRIGGER IF NOT EXISTS WorkItemCheckList_Insert
         AFTER INSERT
            ON WorkItemCheckList
      FOR EACH ROW
BEGIN
UPDATE WorkItemCheckList
       SET ItemSortOrder = (
               SELECT IFNULL( (MAX(ItemSortOrder) + 10), 10) 
                 FROM WorkItemCheckList
                 WHERE WorkItem_ID = new.WorkItem_ID
           )
WHERE WorkItemCheckList_ID = new.WorkItemCheckList_ID;
END;


