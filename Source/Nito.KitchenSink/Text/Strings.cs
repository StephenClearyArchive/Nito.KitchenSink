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
            return DamerauLevenshteinDistance(str1, str2, new int[str1.Length + 1, str2.Length + 1]);
        }

        /// <summary>
        /// Computes the optimal string alignment of the Damerau-Levenshtein edit distance of two strings. This is a fuzzy string matching algorithm that is both powerful and efficient. The strings are compared ordinally.
        /// </summary>
        /// <param name="str1">The first string to compare.</param>
        /// <param name="str2">The second string to compare.</param>
        /// <param name="workingValues">The array used as scratch values. The first dimension of this array must be at least <c>str1.Length + 1</c>, and the second dimension of this array must be at least <c>str2.Length + 1</c>.</param>
        /// <returns>The estimated Damerau-Levenshtein edit distance of the two strings.</returns>
        public static int DamerauLevenshteinDistance(string str1, string str2, int[,] workingValues)
        {
            var length1 = str1.Length;
            var length2 = str2.Length;
            for (int i = 0; i <= length1; ++i)
            {
                workingValues[i, 0] = i;
            }

            for (int j = 1; j <= length2; ++j)
            {
                workingValues[0, j] = j;
            }

            for (int i = 1; i <= length1; ++i)
            {
                for (int j = 1; j <= length2; ++j)
                {
                    char char1 = str1[i - 1];
                    char char2 = str2[j - 1];
                    int cost = (char1 == char2) ? 0 : 1;

                    var deletion = workingValues[i - 1, j] + 1;
                    var insertion = workingValues[i, j - 1] + 1;
                    var substitution = workingValues[i - 1, j - 1] + cost;
                    workingValues[i, j] = Math.Min(Math.Min(deletion, insertion), substitution);

                    if (i > 1 && j > 1 && char1 == str2[j - 2] && str1[i - 2] == char2)
                    {
                        var transposition = workingValues[i - 2, j - 2] + cost;
                        workingValues[i, j] = Math.Min(workingValues[i, j], transposition);
                    }
                }
            }

            return workingValues[length1, length2];
        }
    }
}
