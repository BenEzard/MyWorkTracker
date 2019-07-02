using System;

namespace MyWorkTracker.Code
{
    public class JournalEntry
    {
        public int JournalID { get; set; } = -1;
        public string Title { get; set; }
        public string Entry { get; set; }
        public DateTime CreationDateTime { get; set; }

        public JournalEntry() { }

        public JournalEntry(string title, string entry)
        {
            Title = title;
            Entry = entry;
        }

    }
}
