using System.ComponentModel.DataAnnotations;

namespace InstantDeploy.Extensions;

/// <summary>
/// 枚举扩展类
/// </summary>
public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<int, string>> EnumNameValueDict = new();

    /// <summary>
    /// 获取枚举 Key/Value
    /// </summary>
    /// <param name="enumType"> </param>
    /// <returns> </returns>
    public static Dictionary<int, string> ToDictionary(this Type enumType)
    {
        if (!enumType.IsEnum)
            throw new Exception($"{nameof(enumType)} is not Enum");

        return EnumNameValueDict.GetOrAdd(enumType, _ => GetDictionaryItems(enumType));

        static Dictionary<int, string> GetDictionaryItems(Type enumType)
        {
            var enumItems = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            var names = new Dictionary<int, string>(enumItems.Length);
            foreach (var enumItem in enumItems)
            {
                var descAttribute = enumItem.GetCustomAttribute<DescriptionAttribute>();

                var value = (int)enumItem.GetValue(enumType)!;
                var description = descAttribute?.Description ?? enumItem.Name;

                names[value] = description;
            }

            return names;
        }
    }

    /// <summary>
    /// 根据枚举成员获取Display的属性Name
    /// </summary>
    /// <returns> </returns>
    public static string GetDisplay(this Enum @enum)
    {
        var type = @enum.GetType();
        var memberInfos = type.GetMember(@enum.ToString());
        if (memberInfos.Any() && memberInfos[0].GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] attrs && attrs.Length > 0)
        {
            return attrs[0].Name!;
        }

        return @enum.ToString();
    }

    /// <summary>
    /// 获取枚举 Description
    /// </summary>
    /// <param name="enum"> </param>
    /// <returns> </returns>
    public static string? GetDescription(this System.Enum @enum)
    {
        return @enum.GetType().GetMember(@enum.ToString()).FirstOrDefault()
            ?.GetCustomAttribute<DescriptionAttribute>()
            ?.Description;
    }

    /// <summary>
    /// 获取枚举 Description
    /// </summary>
    /// <param name="value"> </param>
    /// <returns> </returns>
    public static string GetDescription(this Type type, int value)
    {
        if (!type.IsEnum)
            throw new Exception($"{nameof(type)} is not Enum");

        if (EnumNameValueDict.ContainsKey(type))
            _ = ToDictionary(type);

        if (EnumNameValueDict.TryGetValue(type, out var desc))
        {
            if (desc.TryGetValue(value, out string? descValue))
            {
                return descValue;
            }
            else
            {
                descValue = GetDescription(System.Enum.Parse(type.GetType(), value.ToString())?.ToString());
                _ = desc.TryAdd<int, string>(value, descValue);
                return descValue;
            }
        }

        return default!;
    }

    /// <summary>
    /// 获取枚举 Description
    /// </summary>
    /// <param name="value"> </param>
    /// <returns> </returns>
    public static string? GetDescription(this string? value)
    {
        if (value == null) return string.Empty;
        return value.GetType().GetMember(value ?? string.Empty).FirstOrDefault()
            ?.GetCustomAttribute<DescriptionAttribute>()
            ?.Description;
    }

    /// <summary>
    /// GetAttributes，读取相关特性
    /// </summary>
    /// <typeparam name="TAttribute"> </typeparam>
    /// <param name="enum"> </param>
    /// <returns> </returns>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Enum @enum)
        where TAttribute : Attribute
    {
        var type = @enum.GetType();
        if (!Enum.IsDefined(type, @enum))
        {
            return Enum.GetValues(type).OfType<Enum>().Where(@enum.HasFlag)
                .SelectMany(e => type.GetField(e.ToString())
                !.GetCustomAttributes<TAttribute>(false));
        }

        return type.GetField(@enum.ToString())!.GetCustomAttributes<TAttribute>(false);
    }
}