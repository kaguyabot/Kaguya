using System.Collections.Generic;
using System.Linq;
using Qommon.Collections;

namespace Kaguya.Discord.Overrides.Extensions
{
    internal static partial class KaguyaInteractivityExtensions
    {
        public static ReadOnlyCollection<T> AsReadOnlyCollection<T>(this IEnumerable<T> collection)
            => new ReadOnlyCollection<T>(collection.ToArray());

        public static ReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
            => new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
}