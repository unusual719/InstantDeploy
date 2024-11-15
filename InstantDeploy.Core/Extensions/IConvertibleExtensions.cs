#pragma warning disable CS8603,CS8600,CS8601

namespace InstantDeploy.Extensions;

/// <summary>
/// Convertible Extensions
/// </summary>
public static class IConvertibleExtensions
{
    #region private methods

    private static T ConvertTo<T>(this IConvertible value) where T : IConvertible
    {
        if (value != null)
        {
            var type = typeof(T);
            if (value.GetType() == type)
            {
                return (T)value;
            }

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, value.ToString(CultureInfo.InvariantCulture));
            }

            if (value == DBNull.Value)
            {
                return default;
            }

            if (type.IsNumeric())
            {
                return (T)value.ToType(type, new NumberFormatInfo());
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return (T)(underlyingType!.IsEnum ? Enum.Parse(underlyingType, value.ToString(CultureInfo.CurrentCulture)) : Convert.ChangeType(value, underlyingType));
            }

            var converter = TypeDescriptor.GetConverter(value);
            if (converter.CanConvertTo(type))
            {
                return (T)converter.ConvertTo(value, type);
            }

            converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(value.GetType()))
            {
                return (T)converter.ConvertFrom(value);
            }

            return (T)Convert.ChangeType(value, type);
        }

        return (T)value;
    }

    private static object ConvertTo(this IConvertible value, Type type)
    {
        if (value == null)
        {
            return default;
        }

        if (value.GetType() == type)
        {
            return value;
        }

        if (value == DBNull.Value)
        {
            return null;
        }

        if (type.IsAssignableFrom(typeof(string)))
        {
            return value.ToString();
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, value.ToString(CultureInfo.InvariantCulture));
        }

        if (type.IsAssignableFrom(typeof(Guid)))
        {
            return Guid.Parse(value.ToString()!);
        }

        if (type.IsAssignableFrom(typeof(DateTime)))
        {
            return DateTime.Parse(value.ToString()!);
        }

        if (type.IsAssignableFrom(typeof(DateTimeOffset)))
        {
            return DateTimeOffset.Parse(value.ToString()!);
        }

        if (type.IsNumeric())
        {
            return value.ToType(type, new NumberFormatInfo());
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType!.IsEnum ? Enum.Parse(underlyingType, value.ToString(CultureInfo.CurrentCulture)) : Convert.ChangeType(value, underlyingType);
        }

        var converter = TypeDescriptor.GetConverter(value);
        if (converter.CanConvertTo(type))
        {
            return converter.ConvertTo(value, type);
        }

        converter = TypeDescriptor.GetConverter(type);
        return converter.CanConvertFrom(value.GetType()) ? converter.ConvertFrom(value) : Convert.ChangeType(value, type);
    }

    #endregion private methods

    /// <summary>
    /// 是否数值类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNumeric(this Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 类型直转
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T TryConvertTo<T>(this IConvertible value, T defaultValue = default!) where T : IConvertible
    {
        try
        {
            return ConvertTo<T>(value);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 类型直转
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryConvertTo(this IConvertible value, Type type, out object result)
    {
        try
        {
            result = ConvertTo(value, type);
            return true;
        }
        catch
        {
            result = default!;
            return false;
        }
    }
}