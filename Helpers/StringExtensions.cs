using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPQMessenger.Helpers
{
    /// <summary>
    /// Contains static extension methods for the string class.
    /// </summary>
    static class StringExtensions
    {
        /// <summary>
        /// Returns text from within a string that is contained between specified separators.
        /// </summary>
        /// <param name="haystack">The string in which to search.</param>
        /// <param name="openingSeparator">The character that marks the start of the constraint.</param>
        /// <param name="closingSeparator">The chatacter that marks the end of the constraint.</param>
        /// <returns>The text constrained between the separators, or null if none was found.</returns>
        public static string FindContainedText(this string haystack, string openingSeparator, string closingSeparator)
        {
            int startIndex = haystack.IndexOf(openingSeparator) + 1;
            if (startIndex == -1)
            {
                return null;
            }
            int endIndex = haystack.IndexOf(closingSeparator, startIndex);
            return haystack.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Gives a list of all starting indexes of a substring within a string.
        /// </summary>
        /// <param name="str">The string in which to search.</param>
        /// <param name="searchText">The substring to search for.</param>
        /// <returns>An IEnumerable&lt;int&gt; of all the indexes of the specified substring.</returns>
        public static IEnumerable<int> AllIndexesOf(this string str, string searchText)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (searchText == null)
            {
                throw new ArgumentNullException("searchText");
            }

            return AllIndexesOfCore(str, searchText);
        }
        private static IEnumerable<int> AllIndexesOfCore(string str, string searchText)
        {
            for (int index = 0; ; index += searchText.Length)
            {
                index = str.IndexOf(searchText, index);
                if (index == -1)
                    break;
                yield return index;
            }
        }
    }
}
