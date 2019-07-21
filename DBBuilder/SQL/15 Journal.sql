﻿CREATE TABLE Journal ( 
    Journal_ID INTEGER       PRIMARY KEY AUTOINCREMENT, 
    WorkItem_ID INTEGER       REFERENCES WorkItem (WorkItem_ID), 
    Header VARCHAR (500) NULL,
    [Entry] VARCHAR (5000) NOT NULL,
    CreationDateTime DATETIME NOT NULL DEFAULT(CURRENT_TIMESTAMP),
	ModificationDateTime DATETIME NULL,
	DeletionDateTime DATETIME NULL
);

INSERT INTO Journal (WorkItem_ID, Header, [Entry]) VALUES
	(1, 'First entry', 'With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata.'),
    (1, 'Second entry', 'With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-'),
    (1, 'Third entry', 'With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata.'),
    (1, 'Fourth entry', 'With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-'),
    (1, 'Special entry', 'With more details than a cat in the hat lead by a rat with a zat-nik-ata.



	With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata.'),
    (1, 'Sixth entry', 'With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-'),
    (1, 'Seventh entry', 'With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-nik-ata.'),
    (1, 'Last entry', 'With more details than a cat in the hat lead by a rat with a zat-nik-ata. With more details than a cat in the hat lead by a rat with a zat-')