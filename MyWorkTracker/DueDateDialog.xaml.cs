﻿using MyWorkTracker.Code;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public partial class DueDateDialog : Window
    {
        private DateTime _originalDateTime;

        private DateTime _newDateTime;
        public DateTime NewDateTime
        {
            get
            {
                CalculateNewDateTime();
                return _newDateTime;
            }
        }

        public bool WasDialogSubmitted { get; set; } = false;

        /// <summary>
        /// Return the selected 'Change Reason'
        /// </summary>
        /// <returns></returns>
        public string ChangeReason {
            get { return ChangeReasonTextBox.Text; }
        }

        /// <summary>
        /// This is here (as opposed to in the model) because it's only related to the UI, not the application.
        /// </summary>
        Border _originalBorder = new Border();

        public DueDateDialog(DateTime currentSelectedDateTime)
        {
            InitializeComponent();
            _originalDateTime = currentSelectedDateTime;

            DateTime StartOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime MostRecentSaturday = StartOfWeek.AddDays(-1);

            Collection<CalendarDateRange> dates = new Collection<CalendarDateRange>();
            // Add the first Sunday
            CalendarSelection.BlackoutDates.Add(new CalendarDateRange(MostRecentSaturday.AddDays(1)));
            for (int i = 0; i < 52; i++) // TODO make this 52 a variable setting
            {
                DateTime nextSaturday = MostRecentSaturday.AddDays(7);
                CalendarSelection.BlackoutDates.Add(new CalendarDateRange(nextSaturday, nextSaturday.AddDays(1)));
                MostRecentSaturday = nextSaturday;
            }

            CustomiseDisplay();
        }

        /// <summary>
        /// Put the final touches on the display
        /// </summary>
        private void CustomiseDisplay()
        {
            CalendarSelection.DisplayDateStart = DateTime.Now.Date;
            CalendarSelection.SelectedDate = _originalDateTime.Date;


            string ddLabel = Convert.ToString(_originalDateTime);
            CurrentDueDateLabel.Text = String.Format("{0:ddd dd/MM HH:mm}", _originalDateTime.ToString());
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

            int hour = _originalDateTime.Hour;
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

            int min = _originalDateTime.Minute;
            if (min == 0)
                rValue = "00";
            else
                rValue = min.ToString();

            return rValue;
        }

        /// <summary>
        /// Select today's date.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectToday(object sender, RoutedEventArgs e)
        {
            IncrementCalendarSelection(0, null);
        }

        private void Select1Week(object sender, RoutedEventArgs e)
        {
            IncrementCalendarSelection(7, null);
        }

        private void Select2Weeks(object sender, RoutedEventArgs e)
        {
            IncrementCalendarSelection(14, null);
        }

        private void Select3Weeks(object sender, RoutedEventArgs e)
        {
            IncrementCalendarSelection(21, null);
        }

        private void Select1Month(object sender, RoutedEventArgs e)
        {
            IncrementCalendarSelection(null, 1);
        }

        private void Select1Year(object sender, RoutedEventArgs e)
        {
            IncrementCalendarSelection(null, 12);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfDays"></param>
        private void IncrementCalendarSelection(int? numberOfDays=null, int? numberOfMonths=null)
        {
            DateTime newDT = DateTime.Now;
            if ((numberOfDays.HasValue) && (numberOfMonths.HasValue == false))
                newDT = newDT.AddDays(numberOfDays.Value);
            else if ((numberOfDays.HasValue == false) && (numberOfMonths.HasValue))
                newDT = newDT.AddMonths(numberOfMonths.Value);

            CalendarSelection.SelectedDate = newDT;
            CalendarSelection.DisplayDate = newDT;
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

        /// <summary>
        /// Calculate the Date and Time component by combining the data from the Calendar and the Hour and Minute combos.
        /// </summary>
        private void CalculateNewDateTime()
        {
            DateTime originalDate = CalendarSelection.SelectedDate.Value.Date;
            var cbItem = (ComboBoxItem)(HourCombo.SelectedValue);
            originalDate = originalDate.AddHours(Double.Parse((string)cbItem.Content));
            cbItem = (ComboBoxItem)(MinuteCombo.SelectedValue);
            originalDate = originalDate.AddMinutes(Double.Parse((string)cbItem.Content));
            _newDateTime = originalDate;
        }

        /// <summary>
        /// To highlight the selected control, add a left border.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlGainsFocus(object sender, RoutedEventArgs e)
        {
            Control c = e.Source as Control;
            _originalBorder.BorderThickness = c.BorderThickness;
            _originalBorder.BorderBrush = c.BorderBrush;

            c.BorderBrush = Brushes.DodgerBlue;
            c.BorderThickness = new Thickness(4, 0, 0, 0);
        }

        /// <summary>
        /// Reset the formerly-highlighted control by restoring it's borders.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlLosesFocus(object sender, RoutedEventArgs e)
        {
            Control c = e.Source as Control;
            c.BorderBrush = _originalBorder.BorderBrush;
            c.BorderThickness = _originalBorder.BorderThickness;
        }

        private void ApplyDueDateChangeButton_Click(object sender, RoutedEventArgs e)
        {
            WasDialogSubmitted = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WasDialogSubmitted = false;
            this.Close();
        }

        private void CalendarSelection_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            var calendar = sender as Calendar;
            if (calendar.SelectedDate.HasValue)
            {
                SelectedLabel.Text = $"{calendar.SelectedDate:dddd dd/MM}";
            }
        }

        private void CalendarSelection_GotMouseCapture(object sender, MouseEventArgs e)
        {
            UIElement originalElement = e.OriginalSource as UIElement;
            if (originalElement is CalendarDayButton || originalElement is CalendarItem)
            {
                originalElement.ReleaseMouseCapture();
            }
        }
    }
}
