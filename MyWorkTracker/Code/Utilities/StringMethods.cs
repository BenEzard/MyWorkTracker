using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorkTracker.Code
{
    public static class StringMethods
    {
        /// <summary>
        /// Spit a string by the given number of characters.
        /// </summary>
        /// <param name="stringToSplit"></param>
        /// <param name="characterCount"></param>
        /// <returns></returns>
        public static string[] SplitStringByLength(string stringToSplit, int characterCount)
        {
            int arraySize = stringToSplit.Length / 3;
            string[] rValue = new string[arraySize];

            int index = 0;
            while (stringToSplit.Length > 0)
            {
                string snip = stringToSplit.Substring(0, characterCount);
                rValue[index] = snip;
                ++index;
                stringToSplit = stringToSplit.Substring(characterCount, stringToSplit.Length - characterCount);
            }
            
            return rValue;
        }
    }
}
