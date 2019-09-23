CREATE TABLE DueDate ( 
    DueDate_ID INTEGER       PRIMARY KEY AUTOINCREMENT, 
    WorkItem_ID INTEGER       REFERENCES WorkItems (WorkItem_ID), 
    DueDateTime DATETIME      NOT NULL,
    ChangeReason VARCHAR (500) NULL,
    CreationDateTime DATETIME NOT NULL DEFAULT(CURRENT_TIMESTAMP),
	DeletionDateTime DATETIME NULL 
);

CREATE INDEX IDX_DueDate ON [DueDate](WorkItem_ID);

INSERT INTO DueDate (WorkItem_ID, DueDateTime) VALUES
	(1, '2019-08-05 13:30:00.000'),
	(2, '2019-08-05 13:45:00.000');