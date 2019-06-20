using MyWorkTracker.Code;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyWorkTracker
{
    /// <summary>
    /// Interaction logic for DueDateWindow.xaml
    /// </summary>
    public partial class DueDateWindow : Window
    {
        private MWTController _controller = null;
        private WorkItem _workItem = null;

        public DueDateWindow(MWTController controller)
        {
            InitializeComponent();
            _controller = controller;
            _workItem = _controller.GetMWTModel().GetSelectedWorkItem();

            CustomiseDisplay();
        }

        private void CustomiseDisplay()
        {
            CalendarSelection.DisplayDateStart = DateTime.Now.Date;
            CalendarSelection.SelectedDate = DateTime.Now.Date;
            string ddLabel = Convert.ToString(_workItem.DueDate);
            CurrentDueDateLabel.Text = String.Format("{0:ddd dd/MM HH:mm}", _workItem.DueDate.ToString());
            SelectComboItem(HourCombo, GetCurrentDueDateHour());
            SelectComboItem(MinuteCombo, GetCurrentDueDateMinute());
            GetCurrentDueDateHour();
        }

        /// <summary>
        /// Using the current Due Date (i.e. unchanged), return a string representing the hour component of the Due Date.
        /// </summary>
        /// <returns></returns>
        private string GetCurrentDueDateHour()
        {
            string rValue;

            int hour = _workItem.DueDate.Hour;
            if (hour < 10)
                rValue = "0" + hour;
            else
                rValue = hour.ToString();

            return rValue;
        }

        /// <summary>
        /// Using the current Due Date (i.e. unchanged), return a string representing the minute component of the Due Date.
        /// </summary>
        /// <returns></returns>
        private string GetCurrentDueDateMinute()
        {
            string rValue;

            int min = _workItem.DueDate.Minute;
            if (min == 0)
                rValue = "00";
            else
                rValue = min.ToString();

            return rValue;
        }

        private void SelectToday(object sender, RoutedEventArgs e)
        {
            // TODO Change to display current month
            CalendarSelection.SelectedDate = DateTime.Now;
        }

        /// <summary>
        /// Select the specified value in the combo.
        /// </summary>
        /// <param name="combo"></param>
        /// <param name="valueToSelect"></param>
        private void SelectComboItem(ComboBox combo, string valueToSelect)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                string value = Convert.ToString(((ComboBoxItem)combo.Items[i]).Content.ToString());
                if (value.Equals(valueToSelect))
                {
                    combo.SelectedIndex = i;
                    break;
                }
            }
        }

        private void ApplyDueDateChangeButton_Click(object sender, RoutedEventArgs e)
        {
            string changeReason = ChangeReasonTextBox.Text;

            if (changeReason == null)
                changeReason = "";

            if (CalendarSelection.SelectedDate.HasValue)
            {
                DateTime originalDate = _workItem.DueDate;
                DateTime selectedDate = CalendarSelection.SelectedDate.Value;
                var cbItem = (ComboBoxItem)(HourCombo.SelectedValue);
                selectedDate = selectedDate.AddHours(Double.Parse((string)cbItem.Content));
                cbItem = (ComboBoxItem)(MinuteCombo.SelectedValue);
                selectedDate = selectedDate.AddMinutes(Double.Parse((string)cbItem.Content));
                Console.WriteLine($"Original date is {originalDate}; new date is {selectedDate}");
                // TODO Add hour and time to field
                if (selectedDate.Equals(originalDate))
                {
                    Console.WriteLine($"Date and time have not changed {selectedDate}");
                }
                else
                {
                    Console.WriteLine($"Date and CHANGED {selectedDate}");

                    // If the DueDate (from database) has been set within x mins of now, UPDATE the record instead of INSERTING it.
                    int minutesSinceLastSet = DateTime.Now.Subtract(_workItem.Meta.DueDateUpdateDateTime).Minutes;
                    if (minutesSinceLastSet < Convert.ToInt32(_controller.GetMWTModel().GetAppSetting(SettingName.DUE_DATE_SET_WINDOW_MINUTES)))
                    {
                        // Update
                        Console.WriteLine($"...within window = update");
                        _controller.UpdateDBDueDate(_workItem, selectedDate, changeReason);
                    } else
                    {
                        // Insert
                        Console.WriteLine($"...outside window = insert");
                        _controller.InsertDBDueDate(_workItem, selectedDate, changeReason);
                    }
                }
            }

            
        }
    }
}
