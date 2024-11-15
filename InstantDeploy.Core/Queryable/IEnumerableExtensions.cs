namespace InstantDeploy;

/// <summary> IEnumerable Extensions </summary>
public static class IEnumerableExtensions
{
    /// <summary> 根据条件成立再构建 Where 查询 </summary>
    /// <typeparam name="TSource"> 泛型类型 </typeparam>
    /// <param name="sources"> 集合对象 </param>
    /// <param name="condition"> 布尔条件 </param>
    /// <param name="expression"> 表达式 </param>
    /// <returns> 新的集合对象 </returns>
    public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> sources, bool condition, Func<TSource, bool> expression)
    {
        return condition ? sources.Where(expression) : sources;
    }

    /// <summary> 遍历 IEnumerable </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="sources"> </param>
    /// <param name="action"> </param>
    public static void ForEach<T>(this IEnumerable<T> sources, Action<T> action)
    {
        foreach (var item in sources) action(item);
    }
}