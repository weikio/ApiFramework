using System;
using System.Collections.Concurrent;

namespace Weikio.AspNetCore.StartupTasks
{
    public class StartupTaskQueue : IStartupTaskQueue
    {
        private readonly ConcurrentQueue<IStartupTask> _startupTasks = new ConcurrentQueue<IStartupTask>();

        public void QueueStartupTask(IStartupTask startupTask)
        {
            if (startupTask == null)
            {
                throw new ArgumentNullException(nameof(startupTask));
            }

            _startupTasks.Enqueue(startupTask);
        }

        public IStartupTask DequeueAsync()
        {
            _startupTasks.TryDequeue(out var startupTask);

            return startupTask;
        }

        public int Count
        {
            get
            {
                return _startupTasks.Count;
            }
        }
    }
}
