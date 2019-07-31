CREATE VIEW vwWorkItem AS 
SELECT WorkItem.WorkItem_ID, 
	WorkItem.TaskTitle, 
	WorkItem.CreationDateTime,
	WorkItem.TaskDescription, 
	WorkItem.Complete, 
	DueDate.DueDateTime,
	DueDate.DueDateCreationDateTime,
	vwMostRecentWorkItemStatus.Status_ID,
	vwMostRecentWorkItemStatus.StatusLabel,
	vwMostRecentWorkItemStatus.StatusDateTime,
	vwMostRecentWorkItemStatus.IsConsideredActive,
	vwMostRecentWorkItemStatus.DaysSinceCompletion
FROM WorkItem
LEFT JOIN vwMostRecentWorkItemStatus 
	ON WorkItem.WorkItem_ID = vwMostRecentWorkItemStatus.WorkItem_ID
LEFT JOIN ( 
    SELECT DueDate.DueDate_ID, 
    DueDate.DueDateTime,
    DueDate.CreationDateTime AS DueDateCreationDateTime,
    DueDate.ChangeReason, 
    DueDate.WorkItem_ID 
    FROM DueDate 
    INNER JOIN ( 
        SELECT DueDate.WorkItem_ID, 
        MAX(DueDate.CreationDateTime) AS mxCreateDate 
        FROM DueDate 
        GROUP BY DueDate.WorkItem_ID 
    ) 
    AS mxDueDate ON mxDueDate.WorkItem_ID = DueDate.WorkItem_ID AND 
    mxDueDate.mxCreateDate = DueDate.CreationDateTime 
) AS DueDate 
	ON DueDate.WorkItem_ID = WorkItem.WorkItem_ID
WHERE WorkItem.DeletionDateTime IS NULL