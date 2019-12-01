using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyWorkTracker.Code
{
    public class NotebookTopic : BaseDBElement
    {
        private int _parentDBID = -1;
        public int ParentDatabaseID
        {
            get {
                return _parentDBID;
            }

            set
            {
                _parentDBID = value;
                OnPropertyChanged();
            }
        }

        private string _topic;
        public string Topic
        {
            get
            {
                return _topic;
            }

            set
            {
                _topic = value;
                OnPropertyChanged();
            }
        }

        private string _treePath;
        public string TreePath
        {
            get
            {
                return _treePath;
            }

            set
            {
                _treePath = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<NotebookTopic> Topics = new ObservableCollection<NotebookTopic>();

        public NotebookTopic(int id, int parentID, string topic, string treePath)
        {
            DatabaseID = id;
            _parentDBID = parentID;
            _topic = topic;
            _treePath = treePath;
        }

        public void Add(NotebookTopic topic)
        {
            Topics.Add(topic);
        }

        public NotebookTopic GetNotebookTopic(int id)
        {
            NotebookTopic rValue = null;

            for (int i = 0; i < Topics.Count; i++)
            {
                if (Topics[i].DatabaseID == id)
                {
                    rValue = Topics[i];
                    break;
                }
            }

            return rValue;
        }
    }
}
