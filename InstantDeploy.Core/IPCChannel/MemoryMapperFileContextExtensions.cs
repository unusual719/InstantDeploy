namespace InstantDeploy.IPCChannel;

/// <summary> MemoryMapperFileContext Extensions </summary>
public static class MemoryMapperFileContextExtensions
{
    public static MemoryMapperFileContext<TMessage, THandler> CreateMemoryMapperFileContextInstance<TMessage, THandler>(this string memoryMapName)
        where THandler : ChannelHandler<TMessage>
        => new MemoryMapperFileContext<TMessage, THandler>(memoryMapName);
}