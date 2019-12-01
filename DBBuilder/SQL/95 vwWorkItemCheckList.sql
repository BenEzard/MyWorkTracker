CREATE VIEW vwWorkItemCheckList AS
    SELECT WorkItemCheckList_ID,
           WorkItem_ID,
           TaskText,
           TaskDetails,
		   Indent,
           DueDateTime,
           CompletionDateTime,
           ItemSortOrder,
           CreationDateTime,
		   ModificationDateTime,
		   DeletionDateTime
      FROM WorkItemCheckList;