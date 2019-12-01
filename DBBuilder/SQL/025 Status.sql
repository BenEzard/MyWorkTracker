CREATE TABLE [Status] (
    Status_ID   INTEGER PRIMARY KEY AUTOINCREMENT,
    StatusLabel VARCHAR (20) NOT NULL,
	IsConsideredActive BIT NOT NULL,
	IsDefault BIT NOT NULL,
	DeletionDateTime DATETIME NULL,
	UNIQUE(IsConsideredActive, IsDefault, DeletionDateTime) ON CONFLICT FAIL
);

CREATE INDEX IDX_Status ON [Status](StatusLabel);

INSERT INTO [Status] (StatusLabel, IsConsideredActive, IsDefault, DeletionDateTime) 
VALUES		('Active', 1, 1, NULL),
			('Awaiting Feedback', 1, 0, NULL),
			('Completed', 0, 1, NULL),
			('Cancelled', 0, 0, NULL);