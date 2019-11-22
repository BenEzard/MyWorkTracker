CREATE VIEW vwNotebookTopic AS
    SELECT NotebookTopic_ID,
           ParentTopic_ID,
           Topic,
		   TreePath
      FROM NotebookTopic
     ORDER BY TreePath,
              Topic ASC;
