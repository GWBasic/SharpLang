using System;
using System.Threading.Tasks;

namespace SharpLang
{
    /// <summary>
    /// A fiber is a unit of isolation, similar to a queue. Code running on a fiber runs in isolation of all other code queued onto the fiber
    /// </summary>
    public interface IFiber
    {
        /// <summary>
        /// The task is queued to run and runs in the background in isolation
        /// </summary>
        /// <param name="task"></param>
        void QueueToRun(Func<Task> task);

        /// <summary>
        /// Schedules the task to run at an indetermine time in the future
        /// </summary>
        /// <param name="delay">How long to wait until running the task</param>
        /// <param name="task">The task to run</param>
        /// <returns></returns>
        IDisposable ScheduleOnce(TimeSpan delay, Func<Task> task);

        /// <summary>
        /// Schedules the task to run, after a delay, on an interval
        /// </summary>
        /// <param name="delay">The delay for the first instance of running the task. If this is null, the task will be queued immediately</param>
        /// <param name="interval">How often to run the task</param>
        /// <param name="task">The task to run</param>
        /// <returns></returns>
        IDisposable ScheduleOnInterval(TimeSpan? delay, TimeSpan interval, Func<Task> task);

        /// <summary>
        /// Does not return until the task completes on the fiber, essentially behaving as a lock
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task Lock(Func<Task> task);

        /// <summary>
        /// Does not return until the task completes on the fiber, essentially behaving as a lock
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task<T> Lock<T>(Func<Task<T>> task);
    }
}
