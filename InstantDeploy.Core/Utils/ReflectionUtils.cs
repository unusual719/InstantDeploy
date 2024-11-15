#pragma warning disable CS8602,CS8603

namespace InstantDeploy.Utils;

/// <summary>
/// 反射操作工具类
/// </summary>
public static class ReflectionUtils
{
    private static readonly ConcurrentDictionary<string, Delegate> DelegateCacheDictionary = new();

    /// <summary>
    /// 检查类型是否源于原始泛型（如 List[[]]）
    /// </summary>
    /// <param name="toCheck"> </param>
    /// <param name="generic"> </param>
    /// <returns> </returns>
    public static bool IsSubclassOfRawGeneric(this Type? toCheck, Type generic)
    {
        // objType.IsSubclassOfRawGeneric(typeof(List<>))

        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.GetTypeInfo().IsGenericType
                ? toCheck.GetGenericTypeDefinition()
                : toCheck;

            if (generic == cur) return true;

            toCheck = toCheck.GetTypeInfo().BaseType;
        }

        return false;
    }

    /// <summary>
    /// 根据程序集名称获取运行时程序集
    /// </summary>
    /// <param name="assemblyName"> </param>
    /// <returns> </returns>
    public static Assembly GetAssembly(string assemblyName) =>
        AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));

    /// <summary>
    /// 加载程序集中的所有类型
    /// </summary>
    /// <param name="ass"> Assembly 程序集 </param>
    /// <returns> Type </returns>
    public static IEnumerable<Type> GetTypes(this Assembly ass)
    {
        var types = Array.Empty<Type>();

        try
        {
            types = ass.GetTypes();
        }
        catch
        {
            Console.WriteLine($"Error load `{ass.FullName}` assembly.");
        }

        return types.Where(u => u.IsPublic);
    }

    /// <summary>
    /// 获取应用有效程序集
    /// </summary>
    /// <returns> IEnumerable </returns>
    public static IEnumerable<Type> GetAssemblieTypes(string[]? excludeAssemblyNames = null)
    {
        // 获取入口程序集
        var entryAssembly = Assembly.GetEntryAssembly();

        // 非独立发布/非单文件发布
        if (!string.IsNullOrWhiteSpace(entryAssembly.Location))
        {
            var dependencyContext = DependencyContext.Default;

            // 读取项目程序集手动添加引用的dll
            IEnumerable<Assembly> Assemblies = dependencyContext
                .RuntimeLibraries.Where(u => u.Type == "project")
                .Where(
                    excludeAssemblyNames != null,
                    u => !excludeAssemblyNames!.Any(j => u.Name.EndsWith(j))
                )
                .Select(u =>
                    AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(u.Name))
                );

            // 获取有效的订阅者类型集合
            var effectiveTypes = Assemblies.SelectMany(GetTypes);
            return effectiveTypes;
        }
        return default!;
    }

    /// <summary>
    /// 全局扫描程序集
    /// </summary>
    public static IEnumerable<Type> ScanAllAssemblys(string[]? excludeAssemblyNames = null) =>
        GetAssemblieTypes(excludeAssemblyNames);

    /// <summary>
    /// 查找方法指定特性，如果没找到则继续查找声明类
    /// </summary>
    /// <typeparam name="TAttribute"> </typeparam>
    /// <param name="method"> </param>
    /// <param name="inherit"> </param>
    /// <returns> </returns>
    public static TAttribute GetFoundAttribute<TAttribute>(this MethodInfo method, bool inherit)
        where TAttribute : Attribute
    {
        // 获取方法所在类型
        var declaringType = method.DeclaringType;

        var attributeType = typeof(TAttribute);

        // 判断方法是否定义指定特性，如果没有再查找声明类
        var foundAttribute = method.IsDefined(attributeType, inherit)
            ? method.GetCustomAttribute<TAttribute>(inherit)
            : (
                declaringType.IsDefined(attributeType, inherit)
                    ? declaringType.GetCustomAttribute<TAttribute>(inherit)
                    : default
            );

        return foundAttribute;
    }

    /// <summary>
    /// 根据程序集和类型完全限定名获取运行时类型
    /// </summary>
    /// <param name="assembly"> </param>
    /// <param name="typeFullName"> </param>
    /// <returns> </returns>
    public static Type GetType(this Assembly assembly, string typeFullName) =>
        assembly.GetType(typeFullName);

    /// <summary>
    /// 执行方法
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="obj"> </param>
    /// <param name="methodName"> </param>
    /// <param name="args"> </param>
    /// <returns> </returns>
    public static T InvokeMethod<T>(this object obj, string methodName, object[] args)
    {
        var method = obj.GetType()
            .GetMethod(methodName, args.Select(o => o.GetType()).ToArray());

        ArgumentNullException.ThrowIfNull(method);

        return (T)method.Invoke(obj, args)!;
    }

    /// <summary>
    /// 执行方法
    /// </summary>
    /// <param name="obj"> 反射对象 </param>
    /// <param name="methodName"> 方法名，区分大小写 </param>
    /// <param name="args"> 方法参数 </param>
    /// <returns> T类型 </returns>
    public static void InvokeMethod(this object obj, string methodName, object[] args)
    {
        var type = obj.GetType();
        type.GetMethod(methodName, args.Select(o => o.GetType()).ToArray()).Invoke(obj, args);
    }

    /// <summary>
    /// 设置字段
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="obj"> </param>
    /// <param name="name"> </param>
    /// <param name="value"> </param>
    public static void SetField<T>(this T obj, string name, object value)
        where T : class => SetProperty(obj, name, value);

    /// <summary>
    /// 获取字段
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="obj"> </param>
    /// <param name="name"> </param>
    /// <returns> </returns>
    public static T GetField<T>(this object obj, string name) => GetProperty<T>(obj, name);

    /// <summary>
    /// 设置属性
    /// </summary>
    /// <param name="obj"> 反射对象 </param>
    /// <param name="name"> 属性名 </param>
    /// <param name="value"> 值 </param>
    /// <returns> 旧值 </returns>
    public static string SetProperty<T>(this T obj, string name, object value)
        where T : class
    {
        var type = obj.GetType();
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.PropertyOrField(parameter, name);
        var before = Expression.Lambda(property, parameter).Compile().DynamicInvoke(obj);
        if (value == before)
        {
            return value?.ToString();
        }

        if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (value is IConvertible x && x.TryConvertTo(property.Type.GenericTypeArguments[0], out var v))
            {
                type.GetProperty(name)?.SetValue(obj, v);
            }
            else
            {
                type.GetProperty(name)?.SetValue(obj, value);
            }
        }
        else
        {
            var valueExpression = Expression.Parameter(property.Type, "v");
            var assign = Expression.Assign(property, valueExpression);
            if (value is IConvertible x && x.TryConvertTo(property.Type, out var v))
            {
                Expression
                    .Lambda(assign, parameter, valueExpression)
                    .Compile()
                    .DynamicInvoke(obj, v);
            }
            else
            {
                Expression
                    .Lambda(assign, parameter, valueExpression)
                    .Compile()
                    .DynamicInvoke(obj, value);
            }
        }

        return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
    }

    /// <summary>
    /// 获取属性
    /// </summary>
    /// <param name="obj"> 反射对象 </param>
    /// <param name="name"> 属性名 </param>
    /// <typeparam name="T"> 约束返回的T必须是引用类型 </typeparam>
    /// <returns> T类型 </returns>
    public static T GetProperty<T>(this object obj, string name) => (T)GetProperty(obj, name);

    /// <summary>
    /// 获取属性
    /// </summary>
    /// <param name="obj"> 反射对象 </param>
    /// <param name="name"> 属性名 </param>
    /// <returns> T类型 </returns>
    public static object GetProperty(this object obj, string name)
    {
        var type = obj.GetType();
        if (DelegateCacheDictionary.TryGetValue(type.Name + "." + name, out var func))
        {
            return func.DynamicInvoke(obj);
        }
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.PropertyOrField(parameter, name);
        func = Expression.Lambda(property, parameter).Compile();
        DelegateCacheDictionary.TryAdd(type.Name + "." + name, func);
        return func.DynamicInvoke(obj);
    }

    /// <summary>
    /// 获取所有的属性信息
    /// </summary>
    /// <param name="obj"> 反射对象 </param>
    /// <returns> 属性信息 </returns>
    public static PropertyInfo[] GetProperties(this object obj) =>
        obj.GetType()
            .GetProperties(
                BindingFlags.DeclaredOnly
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.Static
            );
}