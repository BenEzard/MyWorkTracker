using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyWorkTracker.Code
{
    public class BaseDBElement : INotifyPropertyChanged
    {
        public int DatabaseID { get; set; } = -1;

        public DateTime? CreationDateTime { get; set; } = DateTime.Now;

        public DateTime? ModificationDateTime { get; set; }
        public DateTime? DeletionDateTime { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
