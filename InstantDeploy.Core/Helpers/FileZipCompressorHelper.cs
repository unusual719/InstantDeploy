namespace InstantDeploy.Helpers;

/// <summary>
/// 文件压缩帮助类
/// </summary>
public class FileZipCompressorHelper
{
    private static Dictionary<string, string> GetFileEntryMaps(List<string> files)
    {
        var fileList = new List<string>();
        void GetFilesRecurs(string path)
        {
            //遍历目标文件夹的所有文件
            fileList.AddRange(Directory.GetFiles(path));

            //遍历目标文件夹的所有文件夹
            foreach (string directory in Directory.GetDirectories(path))
                GetFilesRecurs(directory);
        }

        files.Where(s => !s.StartsWith("http")).ForEach(s =>
        {
            if (Directory.Exists(s))
                GetFilesRecurs(s);
            else
                fileList.Add(s);
        });

        if (!fileList.Any())
            return new Dictionary<string, string>();

        var dirname = new string(fileList.First().Substring(0, fileList.Min(s => s.Length)).TakeWhile((c, i) => fileList.All(s => s[i] == c)).ToArray());
        if (!Directory.Exists(dirname))
            dirname = Directory.GetParent(dirname).FullName;

        var dic = fileList.ToDictionary(s => s, s => s.Substring(dirname.Length));
        return dic;
    }

    private static IWritableArchive CreateZipArchive(List<string> files, string rootdir, ArchiveType archiveType = ArchiveType.Zip)
    {
        var archive = ArchiveFactory.Create(archiveType);
        var dic = GetFileEntryMaps(files);
        var remoteUrls = files.Distinct().Where(s => s.StartsWith("http")).Select(s =>
        {
            try
            {
                return new Uri(s);
            }
            catch (UriFormatException)
            {
                return null;
            }
        }).Where(u => u != null).ToList();
        foreach (var pair in dic)
        {
            archive.AddEntry(Path.Combine(rootdir, pair.Value), pair.Key);
        }

        if (remoteUrls.Any())
        {
            var streams = new ConcurrentDictionary<string, Stream>();
            using var httpClient = new HttpClient();
            Parallel.ForEach(remoteUrls, url =>
            {
                httpClient.GetAsync(url).ContinueWith(async t =>
                {
                    if (t.IsCompleted)
                    {
                        var res = await t;
                        if (res.IsSuccessStatusCode)
                        {
                            Stream stream = await res.Content.ReadAsStreamAsync();
                            streams[Path.Combine(rootdir, Path.GetFileName(HttpUtility.UrlDecode(url.AbsolutePath)))] = stream;
                        }
                    }
                }).Wait();
            });
            foreach (var kv in streams)
            {
                archive.AddEntry(kv.Key, kv.Value, true);
            }
        }

        return archive;
    }

    /// <summary>
    /// 将多个文件压缩
    /// </summary>
    /// <param name="files"></param>
    /// <param name="directory"></param>
    public static void Zip(List<string> files, string directory = "")
    {
        using var archive = CreateZipArchive(files, directory, ArchiveType.Zip);
        archive.SaveTo(directory, new WriterOptions(CompressionType.LZMA)
        {
            LeaveStreamOpen = true,
            ArchiveEncoding = new ArchiveEncoding()
            {
                Default = Encoding.UTF8
            }
        });
    }

    /// <summary>
    /// 解压文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="directory"></param>
    public static void Decompress(string filePath, string directory)
    {
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, null))
                    {
                        tarArchive.ExtractContents(directory);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}