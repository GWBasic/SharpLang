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
        /// The task is queued to run and runs in the background in isolation.
        /// </summary>
        /// <param name="task"></param>
        /// <remarks>The task must be inserted into the queue synchronously.</remarks>
        void QueueToRun(Func<Task> task);

        /// <summary>
        /// The name of the fiber
        /// </summary>
        string Name { get; }

        /// <summary>
        /// True if the code is currently running on the fiber
        /// </summary>
        bool IsOnFiber { get; }
    }

    public static class FiberEx
    {
        /// <summary>
        /// The task is queued to run and runs in the background in isolation
        /// </summary>
        /// <param name="task"></param>
        public static void QueueToRun(this IFiber fiber, Action task)
        {
            fiber.QueueToRun(() =>
            {
                task();
                return Task.FromResult(IntPtr.Zero);
            });
        }

        /// <summary>
        /// Schedules the task to run at a time in the future
        /// </summary>
        /// <param name="delay">How long to wait until running the task</param>
        /// <param name="task">The task to run</param>
        /// <returns></returns>
        public static IDisposable ScheduleOnce(this IFiber fiber, TimeSpan delay, Func<Task> task)
        {
            var keepRunning = true;

            Task.Run(async () =>
            {
                await Task.Delay(delay);

                if (keepRunning)
                {
                    fiber.QueueToRun(task);
                }
            });

            return new Disposable(() => keepRunning = false);
        }

        /// <summary>
        /// Schedules the task to run at a time in the future
        /// </summary>
        /// <param name="delay">How long to wait until running the task</param>
        /// <param name="task">The task to run</param>
        /// <returns></returns>
        public static IDisposable ScheduleOnce(this IFiber fiber, TimeSpan delay, Action task)
        {
            return fiber.ScheduleOnce(delay, () =>
            {
                task();
                return Task.FromResult<IntPtr>(IntPtr.Zero);
            });
        }

        /// <summary>
        /// Schedules the task to run, after a delay, on an interval
        /// </summary>
        /// <param name="delay">The delay for the first instance of running the task. If this is null, the task will be queued immediately</param>
        /// <param name="interval">How often to run the task</param>
        /// <param name="task">The task to run</param>
        /// <returns></returns>
        public static IDisposable ScheduleOnInterval(this IFiber fiber, TimeSpan? delay, TimeSpan interval, Func<Task> task)
        {
            var keepRunning = true;

            Task.Run(async () =>
            {
                await Task.Delay(delay ?? interval);

                while (keepRunning)
                {
                    fiber.QueueToRun(task);
                    await Task.Delay(interval);
                }
            });

            return new Disposable(() => keepRunning = false);
        }


        /// <summary>
        /// Schedules the task to run, after a delay, on an interval
        /// </summary>
        /// <param name="delay">The delay for the first instance of running the task. If this is null, the task will be queued immediately</param>
        /// <param name="interval">How often to run the task</param>
        /// <param name="task">The task to run</param>
        /// <returns></returns>
        public static IDisposable ScheduleOnInterval(this IFiber fiber, TimeSpan? delay, TimeSpan interval, Action task)
        {
            return fiber.ScheduleOnInterval(delay, interval, () =>
            {
                task();
                return Task.FromResult(IntPtr.Zero);
            });
        }

        /// <summary>
        /// Does not return until the task completes on the fiber, essentially behaving as a lock
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Task<T> Lock<T>(this IFiber fiber, Func<Task<T>> task)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            fiber.QueueToRun(async () =>
            {
                try
                {
                    taskCompletionSource.SetResult(await task());
                }
                catch (Exception exception)
                {
                    taskCompletionSource.SetException(exception);
                }
            });

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Does not return until the task completes on the fiber, essentially behaving as a lock
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Task<T> Lock<T>(this IFiber fiber, Func<T> task)
        {
            return fiber.Lock<T>(() => Task.FromResult(task()));
        }

        /// <summary>
        /// Does not return until the task completes on the fiber, essentially behaving as a lock
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Task Lock(this IFiber fiber, Func<Task> task)
        {
            return fiber.Lock<IntPtr>(async () =>
            {
                await task();
                return IntPtr.Zero;
            });
        }

        /// <summary>
        /// Does not return until the task completes on the fiber, essentially behaving as a lock
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Task Lock(this IFiber fiber, Action task)
        {
            return fiber.Lock(() => Task.FromResult(IntPtr.Zero) as Task);
        }

        /// <summary>
        /// Waits for all tasks in the queue to complete
        /// </summary>
        /// <returns></returns>
        public static Task Wait(this IFiber fiber)
        {
            var taskCompletionSource = new TaskCompletionSource<IntPtr>();

            fiber.QueueToRun(() =>
            {
                try
                {
                    taskCompletionSource.SetResult(IntPtr.Zero);
                }
                catch (Exception exception)
                {
                    taskCompletionSource.SetException(exception);
                }
            });

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Asserts that the code is being called on the fiber
        /// </summary>
        /// <param name="fiber"></param>
        public static void AssertOnFiber(this IFiber fiber)
        {
            if (!fiber.IsOnFiber)
            {
                throw new WrongFiberException(fiber);
            }
        }
    }
}
