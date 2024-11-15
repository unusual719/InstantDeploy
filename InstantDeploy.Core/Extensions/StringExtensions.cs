namespace InstantDeploy.Extensions;

/// <summary>
/// String Extensions
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Join
    /// </summary>
    /// <param name="items"></param>
    /// <param name="separate"></param>
    /// <param name="removeEmptyEntry"></param>
    /// <returns></returns>
    public static string Join(this IEnumerable<string> items, string separate = ", ", bool removeEmptyEntry = false) =>
        string.Join(separate, removeEmptyEntry ? items.Where(s => !string.IsNullOrEmpty(s)) : items);

    /// <summary>
    /// Join
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="separate"></param>
    /// <param name="removeEmptyEntry"></param>
    /// <returns></returns>
    public static string Join<T>(this IEnumerable<T> items, string separate = ", ", bool removeEmptyEntry = false) =>
        string.Join(separate, removeEmptyEntry ? items.Where(t => t != null) : items);

    /// <summary>
    /// 字符串转Guid
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Guid ToGuid(this string value)
    {
        Guid.TryParse(value, out Guid guid);
        return guid;
    }

    /// <summary>
    /// 字符串是否为空
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrWhiteSpace(value) || value.Equals("null", StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// 字符掩码
    /// </summary>
    /// <param name="value"></param>
    /// <param name="mask"></param>
    /// <returns></returns>
    public static string Mask(this string value, char mask = '*')
    {
        if (string.IsNullOrWhiteSpace(value?.Trim()))
        {
            return value;
        }

        value = value.Trim();
        string masks = mask.ToString().PadLeft(4, mask);
        return value.Length switch
        {
            >= 11 => Regex.Replace(value, "(.{3}).*(.{4})", $"$1{masks}$2"),
            10 => Regex.Replace(value, "(.{3}).*(.{3})", $"$1{masks}$2"),
            9 => Regex.Replace(value, "(.{2}).*(.{3})", $"$1{masks}$2"),
            8 => Regex.Replace(value, "(.{2}).*(.{2})", $"$1{masks}$2"),
            7 => Regex.Replace(value, "(.{1}).*(.{2})", $"$1{masks}$2"),
            6 => Regex.Replace(value, "(.{1}).*(.{1})", $"$1{masks}$2"),
            _ => Regex.Replace(value, "(.{1}).*", $"$1{masks}")
        };
    }

    /// <summary>
    /// 转换成字节数组
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ToByteArray(this string value) => Encoding.UTF8.GetBytes(value);

    /// <summary>
    /// 将字符串 URL 编码
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string UrlEncode(this string value) =>
        string.IsNullOrEmpty(value) ? String.Empty : System.Web.HttpUtility.UrlEncode(value, Encoding.UTF8);
}