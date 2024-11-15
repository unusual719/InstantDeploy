using System.Threading.Channels;

namespace InstantDeploy.IPCChannel;

/// <summary> 进程管道内通信上下文 </summary>
/// <remarks> 当前机器当前进程内通信，当前机器多进程通信采用 MemoryMapperFile 。 </remarks>
/// <typeparam name="TMessage"> </typeparam>
/// <typeparam name="THandler"> </typeparam>
public sealed class ChannelContext<TMessage, THandler> where THandler : ChannelHandler<TMessage>
{
    /// <summary> 创建无限容量通道 </summary>
    private static readonly Lazy<Channel<TMessage>> _unBoundedChannel = new(() =>
    {
        var channel = Channel.CreateUnbounded<TMessage>(new UnboundedChannelOptions
        {
            SingleReader = false,   // 允许多个管道读写，提供管道吞吐量（无序操作）
            SingleWriter = false
        });

        StartReader(channel);
        return channel;
    });

    /// <summary> 创建有限容量通道 </summary>
    /// <remarks> 默认容量为 1000 </remarks>
    private static readonly Lazy<Channel<TMessage>> _boundedChannel = new(() =>
    {
        var channel = Channel.CreateBounded<TMessage>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,   // 允许多个管道读写，提供管道吞吐量（无序操作）
            SingleWriter = false
        });

        StartReader(channel);
        return channel;
    });

    /// <summary> 私有构造函数 </summary>
    private ChannelContext()
    {
    }

    private static void StartReader(Channel<TMessage> channel)
    {
        var reader = channel.Reader;

        // 创建长时间线程管道读取器
        _ = Task.Factory.StartNew(async () =>
        {
            while (await reader.WaitToReadAsync())
            {
                if (!reader.TryRead(out var message)) continue;

                // 并行执行（非等待）
                await Activator.CreateInstance<THandler>().InvokeAsync(message);
            }
        }, TaskCreationOptions.LongRunning);
    }

    /// <summary> 信息写入管道内通信上下文 </summary>
    /// <param name="message"> </param>
    /// <param name="isBoundedChannel"> 是否使用有限容量通道 </param>
    public static async Task WriteAsync(TMessage message, bool isBoundedChannel = false)
    {
        Channel<TMessage> channel = isBoundedChannel ? _boundedChannel.Value : _unBoundedChannel.Value;
        await channel.Writer.WriteAsync(message);
    }
}