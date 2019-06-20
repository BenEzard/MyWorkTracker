CREATE TABLE [Status] (
    Status_ID   INTEGER      PRIMARY KEY AUTOINCREMENT,
    StatusLabel VARCHAR (20) NOT NULL
);

INSERT INTO [Status] (StatusLabel) 
VALUES		('Active'),
			('Awaiting Feedback'),
			('Completed'),
			('Cancelled');