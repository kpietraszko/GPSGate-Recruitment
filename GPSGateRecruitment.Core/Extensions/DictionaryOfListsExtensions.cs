using System.Collections.Generic;

namespace GPSGateRecruitment.Core.Extensions;

public static class DictionaryOfListsExtensions
{
    public static void AddToList<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key].Add(value);
        }
        else
        {
            dictionary.Add(key, new List<TValue> { value });
        }
    }

    public static void RemoveFromList<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key,
        TValue value)
    {
        dictionary[key].Remove(value);

        if (dictionary[key].Count == 0)
        {
            dictionary.Remove(key);
        }
    }
}