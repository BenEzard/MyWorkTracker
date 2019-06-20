CREATE TABLE DueDate ( 
    DueDate_ID INTEGER       PRIMARY KEY AUTOINCREMENT, 
    WorkItem_ID INTEGER       REFERENCES WorkItems (WorkItem_ID), 
    DueDateTime DATETIME      NOT NULL,
    ChangeReason VARCHAR (500),
    CreationDateTime DATETIME      DEFAULT(CURRENT_TIMESTAMP)
)