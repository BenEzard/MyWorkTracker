using System;

namespace MyWorkTracker.Code
{
    public class JournalEntry
    {
        public int JournalID { get; set; } = -1;
        public string Title { get; set; }
        public string Entry { get; set; }
        public DateTime CreationDateTime { get; set; } = DateTime.Now;
        public DateTime? ModificationDateTime { get; set; }

        public DateTime? DeletionDateTime { get; set; }

        public JournalEntry() { }

        public JournalEntry(string title, string entry)
        {
            Title = title;
            Entry = entry;
        }

        public JournalEntry(int journalID, string title, string entry, DateTime creationDateTime, DateTime? modificationDateTime)
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
