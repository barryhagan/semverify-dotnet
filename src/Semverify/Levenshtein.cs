using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify
{
    //Originally from: https://github.com/DanHarltey/Fastenshtein

    /// <summary>
    /// Measures the difference between two strings.
    /// Uses the Levenshtein string difference algorithm.
    /// </summary>
    public partial class Levenshtein
    {
        /*
         * WARRING this class is performance critical (Speed).
         */

        private readonly string storedValue;
        private readonly int[] costs;

        /// <summary>
        /// Creates a new instance with a value to test other values against
        /// </summary>
        /// <param Name="value">Value to compare other values to.</param>
        public Levenshtein(string value)
        {
            storedValue = value;
            // Create matrix row
            costs = new int[storedValue.Length];
        }

        /// <summary>
        /// gets the length of the stored value that is tested against
        /// </summary>
        public int StoredLength
        {
            get
            {
                return storedValue.Length;
            }
        }

        /// <summary>
        /// Compares a value to the stored value. 
        /// Not thread safe.
        /// </summary>
        /// <returns>Difference. 0 complete match.</returns>
        public int DistanceFrom(string value)
        {
            if (costs.Length == 0)
            {
                return value.Length;
            }

            // Add indexing for insertion to first row
            for (int i = 0; i < costs.Length;)
            {
                costs[i] = ++i;
            }

            for (int i = 0; i < value.Length; i++)
            {
                // cost of the first index
                int cost = i;
                int additionCost = i;

                // cache value for inner loop to avoid index lookup and bonds checking, profiled this is quicker
                char value1Char = value[i];

                for (int j = 0; j < storedValue.Length; j++)
                {
                    int insertionCost = cost;

                    cost = additionCost;

                    // assigning this here reduces the array reads we do, improvment of the old version
                    additionCost = costs[j];

                    if (value1Char != storedValue[j])
                    {
                        if (insertionCost < cost)
                        {
                            cost = insertionCost;
                        }

                        if (additionCost < cost)
                        {
                            cost = additionCost;
                        }

                        ++cost;
                    }

                    costs[j] = cost;
                }
            }

            return costs[^1];
        }
    }
}
