using System.Security.Cryptography;

namespace InstantDeploy.Utils;

/// <summary>
/// File 工具类
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// 计算文件的哈希值
    /// </summary>
    /// <param name="stream"> 被操作的源数据流 </param>
    /// <param name="algo"> 加密算法 </param>
    /// <returns> 哈希值16进制字符串 </returns>
    private static string HashFile(Stream stream, string algo = nameof(MD5))
    {
        stream.Seek(0, SeekOrigin.Begin);
        using HashAlgorithm crypto = algo switch
        {
            nameof(SHA1) => SHA1.Create(),
            nameof(SHA256) => SHA256.Create(),
            nameof(SHA512) => SHA512.Create(),
            _ => MD5.Create(),
        };
        var bufferedStream = new BufferedStream(stream, 1048576);
        byte[] hash = crypto.ComputeHash(bufferedStream);
        var sb = new StringBuilder();
        foreach (var t in hash)
        {
            sb.Append(t.ToString("x2"));
        }

        stream.Seek(0, SeekOrigin.Begin);
        return sb.ToString();
    }

    /// <summary>
    /// 计算文件的 MD5 值
    /// </summary>
    /// <param name="fs"> 源文件流 </param>
    /// <returns> MD5 值16进制字符串 </returns>
    public static string GetFileMD5(this FileStream fs) => HashFile(fs);

    /// <summary>
    /// 计算文件的 sha1 值
    /// </summary>
    /// <param name="fs"> 源文件流 </param>
    /// <returns> sha1 值16进制字符串 </returns>
    public static string GetFileSha1(this Stream fs) => HashFile(fs, nameof(SHA1));

    /// <summary>
    /// 计算文件的 sha256 值
    /// </summary>
    /// <param name="fs"> 源文件流 </param>
    /// <returns> sha256 值16进制字符串 </returns>
    public static string GetFileSha256(this Stream fs) => HashFile(fs, nameof(SHA256));

    /// <summary>
    /// 计算文件的 sha512 值
    /// </summary>
    /// <param name="fs"> 源文件流 </param>
    /// <returns> sha512 值16进制字符串 </returns>

    public static string GetFileSha512(this Stream fs) => HashFile(fs, nameof(SHA512));

    /// <summary>
    /// 尝试创建目录
    /// </summary>
    /// <param name="directory"> </param>
    public static void TryCreateDirectory(this string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="path"> </param>
    /// <returns> </returns>
    public static string GetFileExtensionsName(this string path)
        => string.IsNullOrWhiteSpace(path) ? string.Empty
        : path.Substring(path.LastIndexOf('.')).ToLower().Trim();

    /// <summary>
    /// 是否为目录
    /// </summary>
    /// <param name="path"> </param>
    /// <returns> </returns>
    public static bool IsDirectory(this string path) => string.IsNullOrWhiteSpace(new FileInfo(path).Extension);

    /// <summary>
    /// 复制文件夹及文件
    /// </summary>
    /// <param name="sourceDirectory"> 原文件路径 </param>
    /// <param name="destDirectory"> 目标文件路径 </param>
    /// <returns> </returns>
    public static bool CopyDirectoryAndFile(string sourceDirectory, string destDirectory)
    {
        try
        {
            // 如果目标路径不存在,则创建目标路径
            if (!System.IO.Directory.Exists(destDirectory))
                System.IO.Directory.CreateDirectory(destDirectory);

            // 原文件根目录下的所有文件
            var files = System.IO.Directory.GetFiles(sourceDirectory);
            foreach (var file in files)
            {
                var name = System.IO.Path.GetFileName(file);
                var dest = System.IO.Path.Combine(destDirectory, name);

                // 复制文件
                File.Copy(file, dest, true);
            }

            // 原文件根目录下的所有文件夹
            var folders = System.IO.Directory.GetDirectories(sourceDirectory);
            foreach (var folder in folders)
            {
                var name = System.IO.Path.GetFileName(folder);
                var dest = System.IO.Path.Combine(destDirectory, name);

                // 构建目标路径,递归复制文件
                CopyDirectoryAndFile(folder, dest);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file"> 文件 </param>
    /// <param name="catalogName"> 目录名称 </param>
    /// <returns> </returns>
    public static (bool Success, string FullFilePath, string FileName, string OriginalFileName) FormFileSaveFile(this IFormFile file)
    {
        try
        {
            // 文件名称
            string flieName = file.FileName.Substring(0, file.FileName.LastIndexOf('.'));
            flieName = Regex.Replace(flieName, @"\s", string.Empty);

            string exf = Path.GetExtension(file.FileName);
            var extension = "xlsx,doc,ppt,pptx,rar,zip,jpeg,jpg,png,bpm,pdf,txt,rp,eml,jpeg,jpg,png,bpm,pdf";
            if (!extension.Contains(exf.Replace(".", string.Empty)))
                throw new Exception("您上传的文件格式不支持!");

            // 随机时间
            string timeValue = DateTime.Now.ToString("yyMMddHHmmssffff") + new Random().Next(10000000, 99999999);
            string randomValue = new Random().Next(1000, 10000).ToString();
            string filePathName = string.Format("{0}_{1}", flieName, timeValue + randomValue + exf);

            string localPath = AppContext.BaseDirectory + "/../../../wwwroot/UploadTemporary/";
            if (!Directory.Exists(localPath))
                Directory.CreateDirectory(localPath);

            // 将文件以随机生成的文件名保存到路径中
            string targetFilePath = Path.Combine(localPath, filePathName);
            using (FileStream fileStream = File.Create(targetFilePath))
            {
                file.CopyTo(fileStream);
                fileStream.Flush();
            }

            return (true, targetFilePath, filePathName, file.FileName);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// byte 异步保存文件
    /// </summary>
    /// <param name="bytes"> </param>
    /// <param name="file"> </param>
    /// <returns> </returns>
    public static async Task TrySaveFileAsync(this byte[] bytes, string file)
    {
        if (!File.Exists(file))
        {
            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write);
            await fileStream.WriteAsync(bytes, 0, bytes.Length);
        }
    }

    /// <summary>
    /// byte 同步保存文件
    /// </summary>
    /// <param name="bytes"> </param>
    /// <param name="file"> </param>
    public static void TrySaveFile(this byte[] bytes, string file)
    {
        if (!File.Exists(file))
        {
            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write);
            fileStream.Write(bytes, 0, bytes.Length);
        }
    }

    /// <summary>
    /// 以文件流的形式复制大文件
    /// </summary>
    /// <param name="fileStream"> 源 </param>
    /// <param name="dest"> 目标地址 </param>
    /// <param name="bufferSize"> 缓冲区大小，默认8MB </param>
    public static void CopyToFile(this Stream fileStream, string dest, int bufferSize = 1024 * 8 * 1024)
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        using var fsWrite = new FileStream(dest, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        var stream = new BufferedStream(fileStream, bufferSize);
        stream.CopyTo(fsWrite);
        fileStream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// 以文件流的形式复制大文件(异步方式)
    /// </summary>
    /// <param name="fileStream"> 源 </param>
    /// <param name="dest"> 目标地址 </param>
    /// <param name="bufferSize"> 缓冲区大小，默认8MB </param>
    public static Task CopyToFileAsync(this Stream fileStream, string dest, int bufferSize = 1024 * 1024 * 8)
    {
        using var fsWrite = new FileStream(dest, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        var stream = new BufferedStream(fileStream, bufferSize);
        return stream.CopyToAsync(fsWrite);
    }
}