namespace InstantDeploy.Extensions;

/// <summary>
/// Dictionary 字典扩展类
/// </summary>
public static class IDictionaryExtensions
{
    /// <summary>
    /// Dictionary TryAdd
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (dictionary.IsReadOnly || dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }
}