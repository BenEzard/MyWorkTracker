CREATE VIEW vwExtractLatestDueDate_ZeroThreeZero AS 
SELECT DISTINCT DueDate.*
From DueDate
INNER JOIN ( 
    SELECT WorkItem_ID, 
           MAX(CreationDateTime) AS mxCreate 
    FROM DueDate 
    GROUP BY WorkItem_ID 
) AS LatestSub 
ON DueDate.WorkItem_ID = LatestSub.WorkItem_ID 
AND DueDate.CreationDateTime = LatestSub.mxCreate 
