using System;

namespace MyWorkTracker.Code
{
    public class JournalEntry : BaseDBElement
    {
        public int JournalID
        {
            get { return DatabaseID; }
            set { DatabaseID = value; }
        }

        public string Title { get; set; }
        public string Entry { get; set; }

        public JournalEntry() { }

        public JournalEntry(string title, string entry)
        {
            Title = title;
            Entry = entry;
        }

        public JournalEntry(int journalID, string title, string entry, DateTime? creationDateTime, DateTime? modificationDateTime)
        {
            JournalID = journalID;
            Title = title;
            Entry = entry;
            CreationDateTime = creationDateTime;
            ModificationDateTime = modificationDateTime;
        }

        public JournalEntry(string title, string entry, DateTime? creationDateTime, DateTime? modificationDateTime, DateTime? deletionDateTime)
        {
            Title = title;
            Entry = entry;
            if (creationDateTime.HasValue)
                CreationDateTime = creationDateTime.Value;
            if (modificationDateTime.HasValue)
            ModificationDateTime = modificationDateTime.Value;
            if (deletionDateTime.HasValue)
            DeletionDateTime = deletionDateTime.Value;
        }

    }
}
