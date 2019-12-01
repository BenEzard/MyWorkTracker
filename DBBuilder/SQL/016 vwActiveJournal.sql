CREATE VIEW vwActiveJournal AS
SELECT		Journal_ID, 
			WorkItem_ID, 
			Header,
			[Entry],
			CreationDateTime,
			ModificationDateTime
FROM		Journal
WHERE		DeletionDateTime IS NULL;