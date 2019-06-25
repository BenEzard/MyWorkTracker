using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorkTracker.Code
{
    public static class DateMethods
    {
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
