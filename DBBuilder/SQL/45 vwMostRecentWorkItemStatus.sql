CREATE VIEW vwMostRecentWorkItemStatus AS
SELECT 
    WorkItemStatus.WorkItemStatus_ID,
    WorkItemStatus.WorkItem_ID,
    WorkItemStatus.Status_ID,
    WorkItemStatus.CompletionAmount,
    WorkItemStatus.CreationDateTime,
    WorkItemStatus.ModificationDateTime,
    Status.StatusLabel,
    CASE
       WHEN WorkItemStatus.ModificationDateTime IS NOT NULL THEN WorkItemStatus.ModificationDateTime
       ELSE WorkItemStatus.CreationDateTime
    END AS StatusDateTime,
    CASE
       WHEN Status.IsConsideredActive = 0 THEN 
       cast ( (julianday('now') - julianday(CreationDateTime)) as int ) + ( (julianday('now') - julianday(CreationDateTime)) > cast ( (julianday('now') - julianday(CreationDateTime)) as int )) 
       ELSE -1
    END AS DaysSinceCompletion,
    Status.IsConsideredActive          
    FROM WorkItemStatus
    INNER JOIN
    (
        SELECT WorkItem_ID,
                MAX(CreationDateTime) AS mxCreationDateTime
        FROM WorkItemStatus
		WHERE DeletionDateTime IS NULL
        GROUP BY WorkItem_ID
    )
    AS subMax ON WorkItemStatus.WorkItem_ID = subMax.WorkItem_ID AND 
                WorkItemStatus.CreationDateTime = subMax.mxCreationDateTime
    LEFT JOIN
        Status ON WorkItemStatus.Status_ID = Status.Status_ID