namespace InstantDeploy.Utilities;

/// <summary>
/// 网络静态工具类
/// </summary>
public sealed class NetworkUtility
{
    /// <summary>
    /// 检查 URL 是否是一个互联网地址
    /// </summary>
    /// <param name="url"> URL 地址 </param>
    /// <returns> <see cref="bool" /> </returns>
    public static bool IsWebUrl(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
            (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}