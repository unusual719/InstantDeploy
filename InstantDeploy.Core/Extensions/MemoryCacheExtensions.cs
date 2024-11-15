#pragma warning disable CS8600,CS8602,CS8603,CS8604

namespace InstantDeploy.Extensions;

public static class MemoryCacheExtensions
{
    #region Microsoft.Extensions.Caching.Memory_6_OR_OLDER

    private static readonly Lazy<Func<MemoryCache, object>> GetEntries6 =
        new Lazy<Func<MemoryCache, object>>(
            () => (Func<MemoryCache, object>)Delegate.CreateDelegate(typeof(Func<MemoryCache, object>)
                , typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true)
                , throwOnBindFailure: true));

    #endregion Microsoft.Extensions.Caching.Memory_6_OR_OLDER

    #region Microsoft.Extensions.Caching.Memory_7_OR_NEWER

    private static readonly Lazy<Func<MemoryCache, object>> GetCoherentState = new Lazy<Func<MemoryCache, object>>(
        () => CreateGetter<MemoryCache, object>(typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance)));

    private static readonly Lazy<Func<object, IDictionary>> GetEntries7 = new Lazy<Func<object, IDictionary>>(
        () => CreateGetter<object, IDictionary>(typeof(MemoryCache)
            .GetNestedType("CoherentState", BindingFlags.NonPublic)
            .GetField("_stringEntries", BindingFlags.NonPublic | BindingFlags.Instance)));

    private static Func<TParam, TReturn> CreateGetter<TParam, TReturn>(FieldInfo field)
    {
        var methodName = $"{field.ReflectedType.FullName}.get_{field.Name}";
        var method = new DynamicMethod(methodName, typeof(TReturn), new[] { typeof(TParam) }, typeof(TParam), true);
        var ilGen = method.GetILGenerator();
        ilGen.Emit(OpCodes.Ldarg_0);
        ilGen.Emit(OpCodes.Ldfld, field);
        ilGen.Emit(OpCodes.Ret);
        return (Func<TParam, TReturn>)method.CreateDelegate(typeof(Func<TParam, TReturn>));
    }

    #endregion Microsoft.Extensions.Caching.Memory_7_OR_NEWER

    private static readonly Func<MemoryCache, IDictionary> GetEntries =
        Assembly.GetAssembly(typeof(MemoryCache)).GetName().Version.Major < 7
        ? (Func<MemoryCache, IDictionary>)(cache => cache != null ? (IDictionary)GetEntries6.Value(cache) : new Dictionary<MemoryCache, IDictionary>())
        : cache => cache != null ? GetEntries7.Value(GetCoherentState.Value(cache)) : new Dictionary<MemoryCache, IDictionary>();

    public static ICollection GetKeys(this IMemoryCache memoryCache) => GetEntries((MemoryCache)memoryCache).Keys;

    public static IEnumerable<T> GetKeys<T>(this IMemoryCache memoryCache) => memoryCache.GetKeys().OfType<T>();
}