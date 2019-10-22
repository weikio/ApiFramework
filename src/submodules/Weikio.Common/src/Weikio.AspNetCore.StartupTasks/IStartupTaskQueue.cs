namespace Weikio.AspNetCore.StartupTasks
{
    public interface IStartupTaskQueue
    {
        void QueueStartupTask(IStartupTask startupTask);

        IStartupTask DequeueAsync();

        int Count { get; }
    }
}
