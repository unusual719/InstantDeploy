using System.Security.Cryptography;

namespace InstantDeploy.Core.Utilities;

/// <summary>
/// 文件相关操作工具类
/// </summary>
public class FileUtility
{
    /// <summary>
    /// 计算文件的哈希值
    /// </summary>
    /// <param name="stream"> 被操作的源数据流 </param>
    /// <param name="algo"> 加密算法 </param>
    /// <returns> 哈希值16进制字符串 </returns>
    public static string HashFile(Stream stream, string algo = nameof(MD5))
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
    /// <param name="fileStream"> 源文件流 </param>
    /// <returns> MD5 值16进制字符串 </returns>
    public static string GetFileMD5(FileStream fileStream) => HashFile(fileStream);

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="path"> 文件完整路径 </param>
    /// <returns> <see cref="string" /> </returns>
    public static string GetFileExtensionsName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return path.Substring(path.LastIndexOf('.')).Trim();
    }

    /// <summary>
    /// 复制文件到目标文件夹
    /// </summary>
    /// <param name="original"> 原文件路径 </param>
    /// <param name="target"> 目标文件路径 </param>
    /// <returns> </returns>
    public static bool CopyFileToTargetDirectory(string original, string target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(original);
        ArgumentException.ThrowIfNullOrWhiteSpace(target);

        try
        {
            // 如果目标路径不存在,则创建目标路径
            target.TryCreateDirectory();

            // 原文件根目录下的所有文件
            foreach (var file in Directory.GetFiles(original))
            {
                // 复制文件
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(target, fileName), true);
            }

            // 原文件根目录下的所有文件夹
            foreach (var directory in Directory.GetDirectories(original))
            {
                // 构建目标路径,递归复制文件
                var fileName = Path.GetFileName(directory);
                CopyFileToTargetDirectory(directory, Path.Combine(target, fileName));
            }

            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}