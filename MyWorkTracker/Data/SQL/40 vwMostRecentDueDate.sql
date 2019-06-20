CREATE VIEW vwMostRecentDueDates AS
SELECT DISTINCT DueDate.WorkItem_ID, 
             Latest.LatestDueDateID, 
             Latest.LatestDueDateTime, 
             Latest.LatestCreationDateTime, 
             SecondLatest.SecondLatestDueDateID, 
             SecondLatest.SecondLatestDueDateTime, 
             SecondLatest.SecondLatestCreationDateTime, 
             (strftime('%s', Latest.LatestCreationDateTime) - strftime('%s', SecondLatest.SecondLatestCreationDateTime)) / 60 AS CreationTimeDiffMins
FROM DueDates 
LEFT JOIN ( 
                SELECT DueDate.WorkItem_ID, 
                             DueDate.DueDate_ID AS LatestDueDateID, 
                             DueDate.DueDateTime AS LatestDueDateTime, 
                             DueDate.CreationDateTime AS LatestCreationDateTime 
                FROM DueDates 
                INNER JOIN ( 
                                SELECT WorkItem_ID, 
                                             MAX(CreationDateTime) AS mxCreate 
                                FROM DueDates 
                                GROUP BY WorkItem_ID 
                            ) AS LatestSub 
                ON DueDate.WorkItem_ID = LatestSub.WorkItem_ID 
                AND DueDate.CreationDateTime = LatestSub.mxCreate 
            ) AS Latest 
ON DueDate.WorkItem_ID = Latest.WorkItem_ID 
LEFT JOIN ( 
                SELECT DueDate.WorkItem_ID, 
                             DueDate.DueDate_ID AS SecondLatestDueDateID, 
                             DueDate.DueDateTime AS SecondLatestDueDateTime, 
                             DueDate.CreationDateTime AS SecondLatestCreationDateTime 
                FROM DueDates 
                INNER JOIN ( 
                                SELECT DueDate.WorkItem_ID, 
                                             MAX(CreationDateTime) AS mxCreate 
                                FROM DueDates 
                                LEFT JOIN ( 
                                                SELECT WorkItem_ID, 
                                                             MAX(CreationDateTime) AS mxCreate 
                                                FROM DueDates 
                                                GROUP BY WorkItem_ID 
                                            ) AS mx 
                                ON DueDate.WorkItem_ID = mx.WorkItem_ID 
                                WHERE DueDate.CreationDateTime < mx.mxCreate 
                                GROUP BY     DueDate.WorkItem_ID 
                            ) AS SecondLatestSub 
                ON DueDate.WorkItem_ID = SecondLatestSub.WorkItem_ID 
                AND DueDate.CreationDateTime = SecondLatestSub.mxCreate 
            ) AS SecondLatest 
ON DueDate.WorkItem_ID = SecondLatest.WorkItem_ID