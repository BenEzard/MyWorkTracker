CREATE VIEW vwMostRecentWorkItemStatus AS
-- Get a list of all the WorkItems.
WITH Base_CTE AS (
    SELECT
        DISTINCT wis.WorkItem_ID
    FROM 
        WorkItemStatus wis
    WHERE
        DeletionDateTime IS NULL
),

MaxStatusID_CTE AS (
    SELECT
        wis.WorkItemStatus_ID,
        wis.WorkItem_ID
    FROM
        WorkItemStatus wis
    -- Get the max modification date for the creation date
    INNER JOIN (
        SELECT
            wis.WorkItem_ID,
            wis.CreationDateTime,
            MAX(IFNULL(wis.ModificationDateTime, CURRENT_DATE)) AS mxModificationDT
        FROM
            WorkItemStatus wis
        -- Get the max creation date
        INNER JOIN (
            SELECT
                wis.WorkItem_ID,
                MAX(wis.CreationDateTime) AS mxCreationDT
            FROM
                WorkItemStatus wis
            WHERE
                DeletionDateTime IS NULL
            GROUP BY
                wis.WorkItem_ID
        ) AS subCreation
        ON wis.WorkItem_ID = subCreation.WorkItem_ID
        AND wis.CreationDateTime = subCreation.mxCreationDT
        WHERE
            DeletionDateTime IS NULL
        GROUP BY
            wis.WorkItem_ID
        ) AS subModification
        ON wis.WorkItem_ID = subModification.WorkItem_ID
        AND wis.CreationDateTime = subModification.CreationDateTime    
        AND ifnull(wis.ModificationDateTime, CURRENT_DATE) = subModification.mxModificationDT
    WHERE
        DeletionDateTime IS NULL
)

SELECT 
    wis.WorkItemStatus_ID,
    Base_CTE.WorkItem_ID,
    wis.Status_ID,
    wis.CompletionAmount,
    wis.CreationDateTime,
    wis.ModificationDateTime,
    Status.StatusLabel,
    CASE
       WHEN wis.ModificationDateTime IS NOT NULL THEN wis.ModificationDateTime
       ELSE wis.CreationDateTime
    END AS StatusDateTime,
    CASE
       WHEN Status.IsConsideredActive = 0 THEN 
       cast ( (julianday('now') - julianday(wis.CreationDateTime)) as int ) + ( (julianday('now') - julianday(wis.CreationDateTime)) > cast ( (julianday('now') - julianday(wis.CreationDateTime)) as int )) 
       ELSE -1
    END AS DaysSinceCompletion,
    Status.IsConsideredActive
FROM
    Base_CTE
LEFT JOIN
    MaxStatusID_CTE ON Base_CTE.WorkItem_ID = MaxStatusID_CTE.WorkItem_ID
INNER JOIN
    WorkItemStatus wis ON MaxStatusID_CTE.WorkItemStatus_ID = wis.WorkItemStatus_ID
LEFT JOIN
    Status ON wis.Status_ID = Status.Status_ID