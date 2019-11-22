CREATE TABLE NotebookTopic (
    NotebookTopic_ID     INTEGER      PRIMARY KEY AUTOINCREMENT,
    ParentTopic_ID       INTEGER	  REFERENCES NotebookTopic (NotebookTopic_ID),
    Topic                VARCHAR (50) NOT NULL,
	TreePath			VARCHAR (100) NULL,
    CreationDateTime     DATETIME     NOT NULL DEFAULT(CURRENT_TIMESTAMP),
    ModificationDateTime DATETIME,
    DeletionDateTime     DATETIME
);

CREATE TRIGGER NotebookTopicTree_Insert
         AFTER INSERT ON NotebookTopic FOR EACH ROW
BEGIN
    UPDATE NotebookTopic
	SET 
		TreePath = CASE 
			WHEN ParentTopic_ID IS NULL THEN '' 
			ELSE (
				SELECT TreePath
				FROM NotebookTopic
				WHERE NotebookTopic_ID = NEW.ParentTopic_ID
			)
		|| CASE 
				WHEN ParentTopic_ID < 10 THEN '00' || ParentTopic_ID 
				WHEN ParentTopic_ID < 100 THEN '0' || ParentTopic_ID 
				ELSE ParentTopic_ID 
			END
		END
     WHERE NotebookTopic_ID = NEW.NotebookTopic_ID;
END;
