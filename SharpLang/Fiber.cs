using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpLang
{
    /// <summary>
    /// Runs tasks on the background threadpool
    /// </summary>
    public class Fiber : IFiber
    {
        public Fiber(string name)
        {
            this.Name = name;
        }

        private object sync = new object();
        private Queue<Func<Task>> taskQueue = null;
        private int? currentTaskId = null;

        public string Name { get; }

        /// <summary>
        /// Queues the task to run on the fiber
        /// </summary>
        /// <param name="task"></param>
        public void QueueToRun(Func<Task> task)
        {
            lock (this.sync)
            {
                if (this.taskQueue == null)
                {
                    this.taskQueue = new Queue<Func<Task>>();
                    this.taskQueue.Enqueue(task);
                    Task.Run(this.RunTaskQueue);
                }
                else
                {
                    this.taskQueue.Enqueue(task);
                }
            }
        }

        private async Task RunTaskQueue()
        {
            this.currentTaskId = Task.CurrentId;

            try
            {
                while (true)
                {
                    Func<Task> task;

                    lock (this.sync)
                    {
                        if (this.taskQueue.Count == 0)
                        {
                            this.taskQueue = null;
                            return;
                        }

                        task = this.taskQueue.Dequeue();
                    }

                    try
                    {
                        await task();
                    }
                    catch (Exception exception)
                    {
                        (this.ExceptionHandler ?? Fiber.DefaultExceptionHandler)(exception);
                    }
                }
            }
            finally
            {
                this.currentTaskId = null;
            }
        }

        /// <summary>
        /// The exception handler for the fiber, called when a task has an unhandled exception
        /// </summary>
        public ExceptionHandler ExceptionHandler { get; set; }

        /// <summary>
        /// The default exception handler for all fibers, used when ExceptionHandler is unset.
        /// </summary>
        public static ExceptionHandler DefaultExceptionHandler
        {
            get { return Fiber.defaultExceptionHandler; }
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException($"{nameof(DefaultExceptionHandler)} can not be null");
                }

                Fiber.defaultExceptionHandler = value;
            }
        }

        public bool IsOnFiber
        {
            get
            {
                var currentTaskId = this.currentTaskId;

                if (currentTaskId == null)
                {
                    return false;
                }

                return currentTaskId == Task.CurrentId;
            }
        }

        private static ExceptionHandler defaultExceptionHandler = Fiber.DefaultHandleException;

        private static void DefaultHandleException(Exception exception)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }

            System.Diagnostics.Debug.Assert(false, exception.ToString());
        }
    }
}
