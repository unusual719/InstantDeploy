namespace InstantDeploy.Extensions;

/// <summary>
/// Task 扩展类
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// 等待异步任务执行完成
    /// </summary>
    /// <remarks>捕获错误信息，防止丢失</remarks>
    /// <param name="task"></param>
    /// <param name="onCompleted"></param>
    /// <param name="onError"></param>
    public static async void Await(this Task task, Action? onCompleted = null, Action<Exception>? onError = null)
    {
        try
        {
            await task;
            onCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
        }
    }

    /// <summary>
    /// 等待异步任务执行完成，获取返回值
    /// </summary>
    /// <remarks>同步方法中调用异步任务</remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <returns></returns>
    public static T GetResult<T>(this Task<T> task) => task.GetAwaiter().GetResult();

    /// <summary>
    /// 等待异步任务执行完成
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static void Await(this Task task) => task.GetAwaiter();

    /// <summary>
    /// 异步任务等待超时
    /// </summary>
    /// <param name="task"></param>
    /// <param name="timeout"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
        if (completedTask != task)
        {
            cts.Cancel();
            throw new TimeoutException("Task is timeout.");
        }

        return await task;
    }

    /// <summary>
    /// 异步任务等待超时
    /// </summary>
    /// <param name="task"></param>
    /// <param name="timeout"></param>
    /// <exception cref="TimeoutException"></exception>
    public static async Task TimeoutAfter(this Task task, TimeSpan timeout) => await task.WaitAsync(timeout);
}