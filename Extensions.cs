using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JMR.Common
{
    public static class Extensions
    {
        public static int Count(this IList collection)
        {
            return collection == null ? 0 : collection.Count;
        }

        public static IEnumerable<T> Cast<T>(this IList collection)
        {
            if (collection == null)
            {
                return Enumerable.Empty<T>();
            }

            return collection.OfType<T>();
        }

        public static IEnumerable<TTarget> CreateRange<TSource, TTarget>(this IEnumerable<TSource> sources, Func<TSource, TTarget> creator)
        {
            foreach (var source in sources)
            {
                yield return creator(source);
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static void InsertRange<T>(this ObservableCollection<T> collection, int index, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Insert(index++, item);
            }
        }

        public static void RemoveRange<T>(this ObservableCollection<T> collection, int start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                collection.RemoveAt(start);
            }
        }

        public static void MoveRange<T>(this ObservableCollection<T> collection, int oldIndex, int oldCount, int newIndex, int newCount)
        {
            if (oldCount == 1)
            {
                collection.Move(oldIndex, newIndex);
            }
            else
            {
                var items = collection
                    .Skip(oldIndex)
                    .Take(oldCount)
                    .ToList();

                collection.RemoveRange(oldIndex, oldCount);
                collection.InsertRange(newIndex, items);
            }
        }
    }
}
