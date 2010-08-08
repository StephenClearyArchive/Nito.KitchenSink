using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Text
{
    /// <summary>
    /// String-related algorithms.
    /// </summary>
    public static class Strings
    {
        /// <summary>
        /// Computes the optimal string alignment of the Damerau-Levenshtein edit distance of two strings. This is a fuzzy string matching algorithm that is both powerful and efficient. The strings are compared ordinally.
        /// </summary>
        /// <param name="str1">The first string to compare.</param>
        /// <param name="str2">The second string to compare.</param>
        /// <returns>The estimated Damerau-Levenshtein edit distance of the two strings.</returns>
        public static int DamerauLevenshteinDistance(string str1, string str2)
        {
            var length1 = str1.Length;
            var length2 = str2.Length;
            var ret = new int[length1 + 1, length2 + 1];
            for (int i = 0; i <= length1; ++i)
            {
                ret[i, 0] = i;
            }

            for (int j = 1; j <= length2; ++j)
            {
                ret[0, j] = j;
            }

            for (int i = 1; i <= length1; ++i)
            {
                for (int j = 1; j <= length2; ++j)
                {
                    char char1 = str1[i - 1];
                    char char2 = str2[j - 1];
                    int cost = (char1 == char2) ? 0 : 1;
                    ret[i, j] = new []
                    {
                        ret[i - 1, j] + 1, // deletion
                        ret[i, j - 1] + 1, // insertion
                        ret[i - 1, j - 1] + cost, // substitution
                    }.Min();

                    if (i > 1 && j > 1 && char1 == str2[j - 2] && str1[i - 2] == char2)
                    {
                        // transposition
                        ret[i, j] = Math.Min(ret[i, j], ret[i - 2, j - 2] + cost);
                    }
                }
            }

            return ret[length1, length2];
        }
    }
}
