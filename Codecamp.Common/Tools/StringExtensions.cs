using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Codecamp.Common.Tools
{
    public static class StringExtensions
    {

        public static List<string> GetWords(this string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !String.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value);

            return words.ToList();
        }

        public static string GetValidString(this string text)
        {
            if (text.Length >= 100)
                return text.Substring(0, 100);
            return text;
        }

        private static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
    }
}
