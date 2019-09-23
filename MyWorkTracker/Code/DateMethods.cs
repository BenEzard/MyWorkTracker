using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorkTracker.Code
{
    public static class DateMethods
    {

        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        /// <summary>
        /// Outputs a date time in SQL format.
        /// TODO: this should be replaced by something else pre-existing 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string FormatSQLDateTime(DateTime dt)
        {
            string rValue = $"{dt.Year}-";
            if (dt.Month < 10)
                rValue += "0";
            rValue += $"{dt.Month}-";
            if (dt.Day < 10)
                rValue += "0";
            rValue += $"{dt.Day} ";
            if (dt.Hour < 10)
                rValue += "0";
            rValue += $"{dt.Hour}:";
            if (dt.Minute < 10)
                rValue += "0";
            rValue += $"{dt.Minute}:";
            if (dt.Second < 10)
                rValue += "0";
            rValue += $"{dt.Second}.000";

            return rValue;
        }

        public static string FormatSQLDate(DateTime dt)
        {
            string rValue = $"{dt.Year}-";
            if (dt.Month < 10)
                rValue += "0";
            rValue += $"{dt.Month}-";
            if (dt.Day < 10)
                rValue += "0";
            rValue += $"{dt.Day}";

            return rValue;
        }

        /// <summary>
        /// Outputs a date in a format similar to yy-mm-dd.
        /// </summary>
        /// <param name="dt">The date to be output</param>
        /// <param name="separator">The string to go between year, month and day</param>
        /// <param name="yearSize">The length of the year (2 or 4)</param>
        /// <param name="monthAndDaySize">The length of the month and day (1 or 2). If the value is greater than the size, size will be ignored.</param>
        /// <returns></returns>
        public static string GetDateString(DateTime dt, string separator="-", int yearSize=4, int monthAndDaySize=2)
        {
            if ((yearSize != 2) && (yearSize != 4)) throw new ArgumentException("yearSize parameter must be either 2 or 4.");
            if ((monthAndDaySize != 1) && (monthAndDaySize != 2)) throw new ArgumentException("monthAndDaySize parameter must be either 1 or 2.");

            string rValue = "";

            if (yearSize == 4)
                rValue += dt.Year;
            else if (yearSize == 2)
                rValue += Convert.ToString(dt.Year).Substring(3,2);

            rValue += separator;

            if ((monthAndDaySize == 1) || (dt.Month > 9))
                rValue += dt.Month;
            else if ((monthAndDaySize == 2) && (dt.Month < 10))
                rValue += "0" + dt.Month;

            rValue += separator;

            if ((monthAndDaySize == 1) || (dt.Day > 9))
                rValue += dt.Day;
            else if ((monthAndDaySize == 2) && (dt.Day < 10))
                rValue += "0" + dt.Day;

            return rValue;
        }

        /// <summary>
        /// Calculates a description for the difference between 2 dates.
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <param name="shortVersion"></param>
        /// <returns></returns>
        public static string GenerateDateDifferenceLabel(DateTime date1, DateTime date2, bool shortVersion)
        {
            string rValue = "";
            bool needComma = false;
            bool valueOutputted = false;    // This is set to true if shortVersion==true and some time-related value has been added to
                                            // the string

            TimeSpan? diffBetweenNowAndDue = date2 - date1;

            if (diffBetweenNowAndDue.HasValue)
            {
                // Check to see if it's future-dated or overdue
                if (diffBetweenNowAndDue.Value.TotalSeconds == 0)
                {
                    rValue = "(Due now)";
                    return rValue;
                }
                else if (diffBetweenNowAndDue.Value.TotalSeconds > 0)
                    rValue = "(Due in ";
                else
                {
                    rValue = "(Overdue by ";
                    diffBetweenNowAndDue = diffBetweenNowAndDue.Value.Negate();
                }

                if (diffBetweenNowAndDue.Value.Days != 0)
                {
                    if (shortVersion)
                        valueOutputted = true;

                    rValue += $"{diffBetweenNowAndDue.Value.Days} day";
                    needComma = true;
                    if (diffBetweenNowAndDue.Value.Days > 1)
                        rValue += "s";
                }

                if ((diffBetweenNowAndDue.Value.Hours != 0) && (valueOutputted == false))
                {
                    if (shortVersion)
                        valueOutputted = true;

                    if (needComma)
                        rValue += ", ";
                    rValue += $"{diffBetweenNowAndDue.Value.Hours} hour";
                    if (diffBetweenNowAndDue.Value.Hours > 1)
                        rValue += "s";
                }

                if ((diffBetweenNowAndDue.Value.Minutes != 0) && (valueOutputted == false))
                {
                    if (shortVersion)
                        valueOutputted = true;

                    if (needComma)
                        rValue += ", ";
                    rValue += $"{diffBetweenNowAndDue.Value.Minutes} minute";
                    if (diffBetweenNowAndDue.Value.Minutes > 1)
                        rValue += "s";
                }
            }
            rValue += ")";

            return rValue;
        }
    }
}
