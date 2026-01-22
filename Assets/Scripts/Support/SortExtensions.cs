using System;
using System.Collections.Generic;

namespace Support
{
    public static class SortExtensions
    {
        public static IEnumerable<T> InsertionSort<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            List<T> sortedList = new List<T>(collection);
            for (int i = 1; i < sortedList.Count; i++)
            {
                T key = sortedList[i];
                int j = i - 1;

                while (j >= 0 && sortedList[j].CompareTo(key) > 0)
                {
                    sortedList[j + 1] = sortedList[j];
                    j--;
                }

                sortedList[j + 1] = key;
            }

            return sortedList;
        }

        public static IEnumerable<T> SelectionSort<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            List<T> sortedList = new List<T>(collection);
            for (int i = 0; i < sortedList.Count - 1; i++)
            {
                int minIndex = i;

                for (int j = i + 1; j < sortedList.Count; j++)
                {
                    if (sortedList[j].CompareTo(sortedList[minIndex]) < 0)
                    {
                        minIndex = j;
                    }
                }

                Swap(sortedList, minIndex, i);
            }

            return sortedList;
        }

        public static IEnumerable<T> QuickSort<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            List<T> sortedList = new List<T>(collection);
            QuickSortRecursive(sortedList, 0, sortedList.Count - 1);
            return sortedList;
        }

        private static void QuickSortRecursive<T>(List<T> collection, int left, int right) where T : IComparable<T>
        {
            if (left < right)
            {
                int partitionIndex = Partition(collection, left, right);
                QuickSortRecursive(collection, left, partitionIndex - 1);
                QuickSortRecursive(collection, partitionIndex + 1, right);
            }
        }

        public static IEnumerable<T> BubbleSort<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            List<T> sortedList = new List<T>(collection);
            bool swapped;
            int n = sortedList.Count;

            do
            {
                swapped = false;
                for (int i = 0; i < n - 1; i++)
                {
                    if (sortedList[i].CompareTo(sortedList[i + 1]) > 0)
                    {
                        Swap(sortedList, i, i + 1);
                        swapped = true;
                    }
                }

                n--;
            } while (swapped);

            return sortedList;
        }

        private static int Partition<T>(List<T> collection, int left, int right) where T : IComparable<T>
        {
            T pivot = collection[right];
            int i = left;

            for (int j = left; j < right; j++)
            {
                if (collection[j].CompareTo(pivot) <= 0)
                {
                    Swap(collection, i, j);
                    i++;
                }
            }

            Swap(collection, i, right);

            return i;
        }

        private static void Swap<T>(List<T> collection, int i, int j)
        {
            (collection[i], collection[j]) = (collection[j], collection[i]);
        }
    }
}