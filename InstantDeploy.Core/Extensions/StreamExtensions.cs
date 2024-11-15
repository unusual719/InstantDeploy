namespace InstantDeploy.Extensions;

/// <summary>
/// Stream Extensions
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// 共享读写打开文件
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static FileStream ShareReadWrite(this FileInfo file)
        => file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

    /// <summary>
    /// 共享读写打开文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static FileStream ShareReadWrite(this string filePath)
    {
        FileInfo file = new FileInfo(filePath);
        return file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
    }

    /// <summary>
    /// 读取所有行
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <param name="closeAfter">读取完毕后关闭流</param>
    /// <returns></returns>
    public static async Task<List<string>> ReadAllLinesAsync(this FileStream stream, Encoding? encoding = null,
        bool closeAfter = true)
    {
        encoding ??= Encoding.UTF8;
        var targetSource = new List<string>();
        var streamReader = new StreamReader(stream, encoding);
        while (await streamReader.ReadLineAsync().ConfigureAwait(false) is { } str)
        {
            targetSource.Add(str);
        }

        if (closeAfter)
        {
            streamReader.Close();
            streamReader.Dispose();
            stream.Close();
#if NET5_0_OR_GREATER
            await stream.DisposeAsync().ConfigureAwait(false);
#else
            stream.Dispose();
#endif
        }

        return targetSource;
    }

    /// <summary>
    /// 读取所有行
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<List<string>> ReadAllLinesAsync(this string filePath)
    {
        using var stream = ShareReadWrite(filePath);
        return await ReadAllLinesAsync(stream, Encoding.UTF8, true);
    }

    /// <summary>
    /// 写入所有文本
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="content"></param>
    /// <param name="encoding"></param>
    /// <param name="closeAfter"></param>
    /// <returns></returns>
    public static async Task WriteAllTextAsync(this FileStream stream, string content, Encoding? encoding = null,
        bool closeAfter = true)
    {
        encoding ??= Encoding.UTF8;
        var streamWriter = new StreamWriter(stream, encoding);
        stream.SetLength(0);
        await streamWriter.WriteAsync(content).ConfigureAwait(false);
        await streamWriter.FlushAsync().ConfigureAwait(false);
        if (closeAfter)
        {
            streamWriter.Close();
            stream.Close();
#if NET5_0_OR_GREATER
            await streamWriter.DisposeAsync().ConfigureAwait(false);
            await stream.DisposeAsync().ConfigureAwait(false);
#else
            sw.Dispose();
            stream.Dispose();
#endif
        }
    }

    /// <summary>
    /// 写入所有文本
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task WriteAllTextAsync(this string filePath, string content)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await WriteAllTextAsync(fileStream, content, Encoding.UTF8, true);
    }

    /// <summary>
    /// 文件 To MemoryStream
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static MemoryStream ToMemoryStream(this string file)
    {
        var fileStream = ShareReadWrite(file);
        byte[] bytes = new byte[fileStream.Length];
        fileStream.Read(bytes, 0, bytes.Length);
        fileStream.Close();

        return new MemoryStream(bytes);
    }

    /// <summary>
    /// Stream To File
    /// </summary>
    /// <param name="stream">In MemoryStream</param>
    /// <param name="file">保存文件路径</param>
    public static void TrySaveFile(this Stream stream, string file)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using var fileStream = new FileStream(file, FileMode.Create, FileAccess.Write);
        var bufferedStream = new BufferedStream(stream, 1048576);
        bufferedStream.CopyTo(fileStream);
        stream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Stream To File
    /// </summary>
    /// <param name="stream">In Stream</param>
    /// <param name="file">保存文件路径</param>
    public static async Task TrySaveFileAsync(this Stream stream, string file)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using var fileStream = new FileStream(file, FileMode.Create, FileAccess.Write);
        var bufferedStream = new BufferedStream(stream, 1048576);
        await bufferedStream.CopyToAsync(fileStream);
        stream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// To Base64
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string ToBase64(this Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
        return Convert.ToBase64String(bytes);
    }
}