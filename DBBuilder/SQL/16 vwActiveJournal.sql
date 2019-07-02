CREATE VIEW vwActiveJournal AS
SELECT		Journal_ID, 
			WorkItem_ID, 
			Header,
			[Entry],
			CreationDateTime
FROM		Journal
WHERE		DeletionDateTime IS NULL;