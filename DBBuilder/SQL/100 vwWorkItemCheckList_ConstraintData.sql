CREATE VIEW vwWorkItemCheckList_ConstraintData AS
    SELECT WorkItemCheckList_ID,
           WorkItem_ID,
		   Indent,
           ItemSortOrder,
           CreationDateTime
      FROM WorkItemCheckList;