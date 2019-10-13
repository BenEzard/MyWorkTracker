CREATE VIEW vwExtractLatestWorkItemStatus AS
SELECT
	WorkItemStatus.WorkItemStatus_ID,
    WorkItemStatus.WorkItem_ID,
    WorkItemStatus.Status_ID,
	WorkItemStatus.CompletionAmount,
    WorkItemStatus.CreationDateTime,
	WorkItemStatus.ModificationDateTime,
	WorkItemStatus.DeletionDateTime
FROM WorkItemStatus
INNER JOIN (
	SELECT 
		WorkItem_ID,
		MAX(CreationDateTime) AS mxCreationDateTime
    FROM WorkItemStatus
    GROUP BY WorkItem_ID
) AS subMax 
ON WorkItemStatus.WorkItem_ID = subMax.WorkItem_ID 
	AND WorkItemStatus.CreationDateTime = subMax.mxCreationDateTime
