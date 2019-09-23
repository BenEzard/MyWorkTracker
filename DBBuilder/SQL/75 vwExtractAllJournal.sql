CREATE VIEW vwExtractAllJournal AS
SELECT      CASE WHEN J.Journal_ID IS NULL THEN -1 ELSE J.Journal_ID END AS Journal_ID,
            WorkItem.WorkItem_ID,
            J.Header,
            J.Entry,
            J.CreationDateTime,
            J.ModificationDateTime,
            WorkItem.DeletionDateTime AS WorkItemDeletionDateTime,
            J.DeletionDateTime AS JournalDeletionDateTime,
			CASE
				WHEN WorkItem.DeletionDateTime IS NOT NULL OR J.DeletionDateTime IS NOT NULL THEN 1
				ELSE 0
			END AS JournalOrWorkItemDeleted
FROM        WorkItem
LEFT JOIN    (
                SELECT        Journal_ID, 
		   WorkItem_ID, 
		   Header,
		   [Entry],
		   CreationDateTime,
		   ModificationDateTime,
                             DeletionDateTime
                 FROM	   Journal
             ) AS J 
ON WorkItem.WorkItem_ID = J.WorkItem_ID