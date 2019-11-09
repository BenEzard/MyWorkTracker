CREATE VIEW vwExtractLatestWorkItemStatus_ZeroThreeZero AS
SELECT WorkItemStatus.WorkItemStatus_ID,
        WorkItemStatus.WorkItem_ID,
           WorkItemStatus.Status_ID,
           CreationDateTime
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