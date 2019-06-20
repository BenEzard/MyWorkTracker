CREATE TABLE Journal ( 
    Journal_ID INTEGER       PRIMARY KEY AUTOINCREMENT, 
    WorkItem_ID INTEGER       REFERENCES WorkItems (WorkItem_ID), 
    Header VARCHAR (500),
    [Entry] VARCHAR (5000),
    CreationDateTime DATETIME      DEFAULT(CURRENT_TIMESTAMP)
)