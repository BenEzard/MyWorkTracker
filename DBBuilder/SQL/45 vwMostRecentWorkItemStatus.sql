CREATE VIEW vwMostRecentWorkItemStatus AS
SELECT WorkItemStatus.WorkItem_ID,
           WorkItemStatus.Status_ID,
           StatusLabel,
           CreationDateTime AS StatusDateTime,
            CASE
               WHEN IsConsideredActive = 0 THEN 
               cast ( (julianday('now') - julianday(CreationDateTime)) as int ) + ( (julianday('now') - julianday(CreationDateTime)) > cast ( (julianday('now') - julianday(CreationDateTime)) as int )) 
               ELSE -1
           END AS DaysSinceCompletion,
           IsConsideredActive
      FROM WorkItemStatus
           INNER JOIN
           (
               SELECT WorkItem_ID,
                      MAX(CreationDateTime) AS mxCreationDateTime
                 FROM WorkItemStatus
                GROUP BY WorkItem_ID
           )
           AS subMax ON WorkItemStatus.WorkItem_ID = subMax.WorkItem_ID AND 
                        WorkItemStatus.CreationDateTime = subMax.mxCreationDateTime
           LEFT JOIN
           Status ON WorkItemStatus.Status_ID = Status.Status_ID