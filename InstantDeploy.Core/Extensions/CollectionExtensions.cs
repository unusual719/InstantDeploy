namespace InstantDeploy.Extensions;

/// <summary>
/// 集合扩展类
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// 集合分片
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="numberOfThreads"></param>
    /// <returns></returns>
    public static IEnumerable<T[]> ToChunk<T>(this List<T> source, int numberOfThreads)
    {
#if (NET6_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER)
        return source.Chunk((int)Math.Ceiling(source.Count / (numberOfThreads * 1.0)));
#else
        var chunkSize = (int)Math.Ceiling((double)source.Count / numberOfThreads);
        return Enumerable.Range(0, numberOfThreads)
            .Select(i => source.Skip(i * chunkSize)
            .Take(chunkSize).ToArray());
#endif
    }
}