#pragma warning disable CS8618

namespace InstantDeploy;

/// <summary>
/// 分页集合
/// </summary>
/// <typeparam name="T"></typeparam>
public class PagedList<T>
{
    /// <summary>
    /// 分页数据
    /// </summary>
    /// <param name="items">数据集</param>
    /// <param name="page">当前页</param>
    /// <param name="size">页大小</param>
    /// <param name="count">总条数</param>
    public PagedList(List<T> items, int page, int size, int count)
    {
        TotalCount = count;
        PageSize = size;
        PageIndex = page;
        TotalPages = (int)Math.Ceiling(count * 1.0 / size);
        Data = items;
    }

    public PagedList()
    {
    }

    /// <summary>
    /// 数据集
    /// </summary>
    public List<T> Data { get; }

    /// <summary>
    /// 当前页
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// 总条数
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// 当前页数据条数
    /// </summary>
    public int CurrentCount => Data.Count;

    /// <summary>
    /// 是否有前一页
    /// </summary>
    public bool HasPrev => PageIndex > 1;

    /// <summary>
    /// 是否有后一页
    /// </summary>
    public bool HasNext => PageIndex < TotalPages;
}

/// <summary>
/// IQueryable 分页扩展类
/// </summary>
public static class IQueryablePagedExtensions
{
    /// <summary>
    /// 生成分页集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex">当前页</param>
    /// <param name="pageSize">页大小</param>
    /// <returns></returns>
    public static PagedList<T> ToPagedList<T>(this IQueryable<T> query, int pageIndex = 1, int pageSize = 100)
    {
        if (pageIndex <= 0) throw new InvalidOperationException($"{nameof(pageIndex)} 必须是大于0的正整数。");

        var totalCount = query.Count();
        if (1L * pageIndex * pageSize > totalCount)
        {
            pageIndex = (int)Math.Ceiling(totalCount / (pageSize * 1.0));
        }

        pageIndex = pageIndex <= 0 ? 1 : pageIndex;

        var list = query.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        return new PagedList<T>(list, pageIndex, pageSize, totalCount);
    }

    /// <summary>
    /// 生成分页集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex">当前页</param>
    /// <param name="pageSize">页大小</param>
    /// <returns></returns>
	public static PagedList<T> ToPagedList<T>(this IEnumerable<T> query, int pageIndex = 1, int pageSize = 100)
    {
        if (pageIndex <= 0) throw new InvalidOperationException($"{nameof(pageIndex)} 必须是大于0的正整数。");

        var totalCount = query.Count();
        if (1L * pageIndex * pageSize > totalCount)
        {
            pageIndex = (int)Math.Ceiling(totalCount / (pageSize * 1.0));
        }

        pageIndex = pageIndex <= 0 ? 1 : pageIndex;

        var list = query.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        return new PagedList<T>(list, pageIndex, pageSize, totalCount);
    }
}

/// <summary>
/// IQueryable 扩展类
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// 根据条件成立再构建 Where 查询
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="expression">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> sources, bool condition
        , Expression<Func<TSource, bool>> expression)
    {
        return condition ? Queryable.Where(sources, expression) : sources;
    }

    /// <summary>
    /// 根据条件成立再构建 Where 查询，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="expression">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> sources, bool condition
        , Expression<Func<TSource, int, bool>> expression)
    {
        return condition ? Queryable.Where(sources, expression) : sources;
    }
}