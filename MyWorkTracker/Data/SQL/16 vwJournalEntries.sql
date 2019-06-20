CREATE VIEW vwJournalEntries AS
WITH FirstCTE AS (
SELECT Journal_ID,
MIN(CreationDateTime) AS CreationDateTime
FROM Journals
),

LastCTE AS (
SELECT Journal_ID,
MAX(CreationDateTime) AS CreationDateTime
FROM Journals
)

SELECT Journals.Journal_ID,
Journals.Header,
Journals.Entry,
Journals.CreationDateTime,
CASE WHEN Journals.CreationDateTime = FirstCTE.CreationDateTime THEN 'Y' ELSE 'N' END AS FirstFlag,
CASE WHEN Journals.CreationDateTime = LastCTE.CreationDateTime THEN 'Y' ELSE 'N' END AS LastFlag
FROM Journals
LEFT JOIN FirstCTE ON Journals.Journal_ID = FirstCTE.Journal_ID
LEFT JOIN LastCTE ON Journals.Journal_ID = FirstCTE.Journal_ID