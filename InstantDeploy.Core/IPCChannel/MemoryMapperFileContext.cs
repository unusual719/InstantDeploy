using System.IO.MemoryMappedFiles;

namespace InstantDeploy.IPCChannel;

/// <summary> 当前机器多进程之间通信上下文 </summary>
public sealed class MemoryMapperFileContext<TMessage, THandler> where THandler : ChannelHandler<TMessage>
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly MemoryMappedFile _mmf;
    private readonly Mutex _mutex;
    private long _capacity = 1024 * 1024;
    private string _memoryMapName;

    /// <summary> 私有构造函数 </summary>
    public MemoryMapperFileContext(string memoryMapName)
    {
        _memoryMapName = $"Global\\{memoryMapName}";
        _mmf = MemoryMappedFile.CreateOrOpen(_memoryMapName, _capacity);
        _mutex = new Mutex(false, $"Global\\{memoryMapName}_Mutex");
    }

    /// <summary> 开始读取共享内存消息 </summary>
    private void StartReader(CancellationToken cancellationToken)
    {
        // 创建长时间线程读取器
        _ = Task.Factory.StartNew(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync();
                try
                {
                    using var memoryMappedFile = MemoryMappedFile.OpenExisting(_memoryMapName);
                    using var accessor = memoryMappedFile.CreateViewAccessor();

                    var buffer = new byte[_capacity];
                    accessor.ReadArray(0, buffer, 0, buffer.Length);

                    var message = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                    if (string.IsNullOrEmpty(message)) continue;

                    await Activator.CreateInstance<THandler>().InvokeAsync((TMessage)Convert.ChangeType(message, typeof(TMessage)));
                }
                catch (FileNotFoundException ex)
                {
                    throw ex;
                }
                finally
                {
                    _semaphore.Release();
                }

                await Task.Delay(500);
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    /// <summary> 写入消息到共享内存 </summary>
    public async Task WriteAsync(TMessage message)
    {
        await _semaphore.WaitAsync();

        var messageData = Encoding.UTF8.GetBytes(message.ToString());
        try
        {
            using var memoryMappedFile = MemoryMappedFile.CreateOrOpen(_memoryMapName, _capacity);
            using var accessor = memoryMappedFile.CreateViewAccessor(0, messageData.Length);
            accessor.WriteArray(0, messageData, 0, messageData.Length);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary> 设置容量大小 </summary>
    /// <param name="capacity"> </param>
    public MemoryMapperFileContext<TMessage, THandler> SetCapacity(long capacity)
    {
        _capacity = capacity;
        return this;
    }

    /// <summary> 设置名称 </summary>
    /// <param name="memoryMapName"> </param>
    public MemoryMapperFileContext<TMessage, THandler> SetMemoryMapName(string memoryMapName)
    {
        _memoryMapName = memoryMapName;
        return this;
    }
}