using System.Collections;
using System.Collections.Generic;

namespace LeagueRecorder.Shared.Abstractions.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }
    }
}