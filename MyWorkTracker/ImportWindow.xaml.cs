using Microsoft.Win32;
using MyWorkTracker.Code;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace MyWorkTracker
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        private string _defaultSaveLocation;
        public ObservableCollection<WorkItemImportListEntry> _importList = new ObservableCollection<WorkItemImportListEntry>();

        /// <summary>
        /// This is the list of ALL WorkItemStatuses that are contained within the to-be-imported XML file.
        /// It is NOT the list of Statuses that will be imported; see _importStatuses
        /// </summary>
        private List<WorkItemStatus> _statuses = new List<WorkItemStatus>();

        /// <summary>
        /// This is the list of WorkItemStatuses that MAY need to be imported.
        /// It is a DISTINCT list of all Statuses that relate to the WorkItems that are selected when the user selects the import button.
        /// This list is populated by PreparePotentialStatusImports();
        /// </summary>
        private List<WorkItemStatus> _importStatuses = null;
        public List<WorkItemStatus> GetImportStatuses
        {
            get { return _importStatuses; }
        }

        private XDocument _xmlDoc = null;
        public XDocument LoadedXMLDocument { get { return _xmlDoc; } }

        private bool _isSubmitted = false;
        public bool WasSubmitted
        {
            get
            {
                return _isSubmitted;
            }
        }

        /// <summary>
        /// Should the Preferences be imported?
        /// </summary>
        public bool ImportPreferencesSelected
        {
            get
            {
                if (LoadPreferencesCheckBox.IsChecked.HasValue && LoadPreferencesCheckBox.IsChecked == true)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Count the number of Work Items selected for import.
        /// </summary>
        public int ImportSelectionCount
        {
            get { return LoadList.SelectedItems.Count; }
        }

        public Dictionary<PreferenceName, string> LoadedPreferences = new Dictionary<PreferenceName, string>();

        public List<int> GetSelections()
        {
            List<int> rValue = new List<int>();

            foreach(var item in LoadList.SelectedItems)
            {

                rValue.Add(((WorkItemImportListEntry)item).WorkItemID);
            }

            return rValue;
        }

        public ImportWindow(string applicationVersion, string defaultSaveLocation)
        {
            _defaultSaveLocation = defaultSaveLocation;
            InitializeComponent();
            IntoVersionTextBox.Text = applicationVersion;

            LoadList.ItemsSource = _importList;
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "xml|*.xml";
            openFileDialog.InitialDirectory = _defaultSaveLocation;
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                LoadFile(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Given a StatusID, returns the Status Label.
        /// </summary>
        /// <param name="statusID"></param>
        /// <returns></returns>
        private string GetStatusLabel(int statusID)
        {
            string rValue = "";

            foreach (WorkItemStatus wis in _statuses)
            {
                if (wis.WorkItemStatusID == statusID)
                {
                    rValue = wis.Status;
                    break;
                }
            }

            return rValue;
        }

        /// <summary>
        /// Given a specified label, return the WorkItemStatus.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private WorkItemStatus GetStatusByLabel(string status)
        {
            WorkItemStatus rValue = null;

            foreach (WorkItemStatus wis in _statuses)
            {
                if (wis.Status == status)
                {
                    rValue = wis;
                    break;
                }
            }

            return rValue;
        }

        /// <summary>
        /// Check to see if a Status is considered to be active (based on the XML load).
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool IsConsideredActive(string status)
        {
            bool rValue = false;

            foreach (WorkItemStatus wis in _statuses)
            {
                if (wis.Status == status)
                {
                    rValue = wis.IsConsideredActive;
                    break;
                }
            }

            return rValue;
        }

        /// <summary>
        /// Return the version number of the data attempting to be loaded.
        /// </summary>
        /// <returns></returns>
        public string GetImportVersion
        {
            get
            {
                return LoadVersionTextBox.Text;
            }
        }

        /// <summary>
        /// Return the file location where the import occured from.
        /// Returns NULL if file was not imported.
        /// </summary>
        public string GetImportFileLocation
        {
            get
            {
                if (WasSubmitted == true)
                    return ImportFileTextBox.Text;
                else
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        private void LoadFile(string filename)
        {
            ImportFileTextBox.Text = filename;
            _importList.Clear();
            _statuses.Clear();

            // Use XMLReader if you don't want to load it into memory
            _xmlDoc = XDocument.Load(filename);
            var query = from element in _xmlDoc.Elements("MyWorkTracker")
                        select element;

            string version = "";
            string extractVersion = "";
            string extractDate = "";
            foreach (var el2 in query)
            {
                version = el2.Attribute("ApplicationVersion").Value;
                extractVersion = el2.Attribute("ExtractVersion").Value;
                extractDate = el2.Attribute("ExtractDate").Value;
            }
            FileDataTextBox.Text = extractDate;
            LoadVersionTextBox.Text = extractVersion;

            var preferenceQuery = from element in _xmlDoc.Descendants("Preference")
                         select element;
            foreach (var pq in preferenceQuery)
            {
                string prefName = pq.Element("Name").Value;
                string prefValue = pq.Element("Value").Value;
                Enum.TryParse(prefName, out PreferenceName pName);
                LoadedPreferences.Add(pName, prefValue);
            }

            var query2 = from element in _xmlDoc.Descendants("Status")
                         select element;

            foreach (var el2 in query2)
            {
                int statusID = Int32.Parse(el2.Attribute("Status_ID").Value);
                string statusLabel = el2.Element("StatusLabel").Value;
                bool isConsideredActive = Boolean.Parse(el2.Element("IsConsideredActive").Value);
                bool isDefault = Boolean.Parse(el2.Element("IsDefault").Value);
                string deletionDateStr = el2.Element("DeletionDateTime").Value;

                if (deletionDateStr.Equals("") == false)
                {
                    _statuses.Add(new WorkItemStatus(statusID, statusLabel, isConsideredActive, isDefault, DateTime.Parse(deletionDateStr)));
                }
                else
                    _statuses.Add(new WorkItemStatus(statusID, statusLabel, isConsideredActive, isDefault));
            }

            var query3 = from element in _xmlDoc.Descendants("WorkItem")
                        select element;

            foreach (var el3 in query3)
            {
                var workItemID = Int32.Parse(el3.Attribute("WorkItem_ID").Value);
                var statusID = Int32.Parse(el3.Attribute("LastWorkItemStatus_ID").Value);
                var title = el3.Element("Title").Value;
                var creationDate = DateTime.Parse(el3.Element("CreationDateTime").Value);

                _importList.Add(new WorkItemImportListEntry(workItemID, title, creationDate, GetStatusLabel(statusID)));
            }
        }

        /// <summary>
        /// Respond to radio button selection (list filters), displaying the appropriate records.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectListItems(object sender, RoutedEventArgs e)
        {
            if (AllRadioButton.IsChecked.HasValue && AllRadioButton.IsChecked.Value)
            {
                LoadList.SelectAll();
            }
            else if (ActiveRadioButton.IsChecked.HasValue && ActiveRadioButton.IsChecked.Value)
            {
                LoadList.SelectedItems.Clear();
                foreach (WorkItemImportListEntry item in LoadList.Items)
                {
                    if (IsConsideredActive(item.Status))
                        LoadList.SelectedItems.Add(item);
                }
            }
            else if (ClosedRadioButton.IsChecked.HasValue && ClosedRadioButton.IsChecked.Value)
            {
                LoadList.SelectedItems.Clear();
                foreach (WorkItemImportListEntry item in LoadList.Items)
                {
                    if (IsConsideredActive(item.Status) == false)
                        LoadList.SelectedItems.Add(item);
                }
            }
            else if (SelectedOnlyRadioButton.IsChecked.HasValue && SelectedOnlyRadioButton.IsChecked.Value)
            {
                // Do nothing here
            }

            // TODO: This should be in XAML
            if (LoadList.SelectedItems.Count == 0)
                ImportButton.IsEnabled = false;
            else
                ImportButton.IsEnabled = true;
        }

        /// <summary>
        /// Update the label which shows how many items are selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateSelectionLabel(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectionCountTextBox.Text = "0 Work Items selected".Replace("0", LoadList.SelectedItems.Count.ToString());
        }

        /// <summary>
        /// Prepares a list of distinct WorkItemStatuses of the selected items.
        /// </summary>
        private void PreparePotentialStatusImports()
        {
            _importStatuses = new List<WorkItemStatus>();
            if (LoadList.SelectedItems.Count > 0)
            {
                foreach (WorkItemImportListEntry item in LoadList.SelectedItems)
                {
                    WorkItemStatus status = GetStatusByLabel(item.Status);
                    if (_importStatuses.Contains(status) == false) {
                        _importStatuses.Add(status);
                    }
                }
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            PreparePotentialStatusImports();
            _isSubmitted = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isSubmitted = false;
            this.Close();
        }
    }
}
