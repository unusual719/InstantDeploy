namespace InstantDeploy.Extensions;

/// <summary>
/// ValueTypeConvert 值类型扩展类
/// </summary>
public static class ValueTypeConvertExtensions
{
    /// <summary>
    /// 保留小数
    /// </summary>
    /// <param name="num"></param>
    /// <param name="decimals"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static decimal Round(this decimal num, int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        num = Math.Round(num, decimals, mode);
        return num;
    }

    /// <summary>
    /// 保留小数
    /// </summary>
    /// <param name="num"></param>
    /// <param name="decimals"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static double Round(this double num, int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        num = Math.Round(num, decimals, mode);
        return num;
    }

    /// <summary>
    /// 保留小数
    /// </summary>
    /// <param name="num"></param>
    /// <param name="decimals"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static decimal? Round(this decimal? num, int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        if (num.HasValue)
        {
            num = Math.Round(num.Value, decimals, mode);
        }
        return num;
    }

    /// <summary>
    /// 保留小数
    /// </summary>
    /// <param name="num"></param>
    /// <param name="decimals"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static double? Round(this double? num, int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        if (num.HasValue)
        {
            num = Math.Round(num.Value, decimals, mode);
        }
        return num;
    }
}