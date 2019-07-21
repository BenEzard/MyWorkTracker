 CREATE TABLE WorkItem ( 
    WorkItem_ID INTEGER PRIMARY KEY AUTOINCREMENT,
    TaskTitle VARCHAR(50) NOT NULL,
	TaskDescription VARCHAR(8000) NULL,
    Complete INTEGER NOT NULL DEFAULT 0, 
    CreationDateTime DATETIME NOT NULL DEFAULT(CURRENT_TIMESTAMP),
	DeletionDateTime DATETIME NULL
);


INSERT INTO WorkItem (TaskTitle, TaskDescription) VALUES
	('A Sample Work Item', 'Noteworthy Cards (https://www.noteworthy.cards/) is an Australian social enterprise selling greeting and encouragement cards while donating 100% of profits to fund life-changing gifts for people suffering in poverty.

About seven billion greeting cards are purchased worldwide every year, with annual retail sales estimated at almost $10 billion AUD. Imagine how many lives could be changed around the world if 100% of those profits were used to help people in need!

By purchasing our cards you will help fund gifts that support community, education, good food, good health, safe water and sanitation in multiple countries around the world.');

INSERT INTO WorkItem (TaskTitle, TaskDescription, Complete) VALUES
	('Another Work Item', 'Some example text', 100);
