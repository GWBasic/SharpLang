using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using SharpLang;

namespace SharpLang.Tests
{
    public class FiberTests
    {
        Fiber fiber = new Fiber("Test Fiber");
        List<Exception> exceptions = new List<Exception>();

        public FiberTests()
        {
            this.fiber.ExceptionHandler = exceptions.Add;
        }

        [SetUp]
        public void SetUp()
        {
            this.exceptions.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (this.exceptions.Count > 0)
            {
                throw this.exceptions[0];
            }
        }

        [Test]
        public async Task RunOnQueue()
        {
            var value = 0;

            this.fiber.QueueToRun(() =>
            {
                Assert.AreEqual(0, value);
                value = 1;
            });

            this.fiber.QueueToRun(() =>
            {
                Assert.AreEqual(1, value);
                value = 2;
            });

            this.fiber.QueueToRun(() =>
            {
                Assert.AreEqual(2, value);
                value = 3;
            });

            this.fiber.QueueToRun(() =>
            {
                Assert.AreEqual(3, value);
                value = 4;
            });

            this.fiber.QueueToRun(() =>
            {
                Assert.AreEqual(4, value);
                value = 5;
            });

            await this.fiber.Wait();

            Assert.AreEqual(5, value);
        }

        [Test]
        public async Task Schedule()
        {
            var taskCompletionSource = new TaskCompletionSource<IntPtr>();

            var start = DateTime.UtcNow;
            fiber.ScheduleOnce(TimeSpan.FromSeconds(1), () => taskCompletionSource.SetResult(IntPtr.Zero));

            await taskCompletionSource.Task;

            var completed = DateTime.UtcNow - start;

            Assert.GreaterOrEqual(completed.TotalSeconds, 0.9, "Scheduled task started too soon");
            Assert.LessOrEqual(completed.TotalSeconds, 1.1, "Scheduled task completed too late");
        }

        [Test]
        public async Task Schedule_Cancel()
        {
            var start = DateTime.UtcNow;
            var ran = false;
            fiber.ScheduleOnce(TimeSpan.FromSeconds(1), () => ran = true).Dispose();

            await Task.Delay(TimeSpan.FromSeconds(1.5));

            Assert.False(ran, "Task was not canceled");
        }

        [Test]
        public async Task ScheduleOnInterval()
        {
            var ctr = 0;
            using (fiber.ScheduleOnInterval(TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(0.5), () => ctr++))
            {
                Assert.AreEqual(0, ctr);

                await Task.Delay(TimeSpan.FromSeconds(0.3));
                Assert.AreEqual(1, ctr);

                await Task.Delay(TimeSpan.FromSeconds(0.5));
                Assert.AreEqual(2, ctr);

                await Task.Delay(TimeSpan.FromSeconds(0.5));
                Assert.AreEqual(3, ctr);

                await Task.Delay(TimeSpan.FromSeconds(0.5));
                Assert.AreEqual(4, ctr);
            }

            await Task.Delay(TimeSpan.FromSeconds(0.5));
            Assert.AreEqual(4, ctr);
        }

        [Test]
        public async Task Lock_Return()
        {
            this.fiber.QueueToRun(() => Task.Delay(TimeSpan.FromSeconds(1)));

            var start = DateTime.UtcNow;

            var actual = await this.fiber.Lock(() => 5);

            var completed = DateTime.UtcNow - start;

            Assert.GreaterOrEqual(completed.TotalSeconds, 0.9, "Locked task started too soon");
            Assert.LessOrEqual(completed.TotalSeconds, 1.1, "Locked task completed too late");

            Assert.AreEqual(5, actual, "Wrong value returned from Lock");
        }

        [Test]
        public async Task Lock()
        {
            this.fiber.QueueToRun(() => Task.Delay(TimeSpan.FromSeconds(1)));

            var start = DateTime.UtcNow;

            var called = false;
            await this.fiber.Lock(() => called = true);

            var completed = DateTime.UtcNow - start;

            Assert.GreaterOrEqual(completed.TotalSeconds, 0.9, "Locked task started too soon");
            Assert.LessOrEqual(completed.TotalSeconds, 1.1, "Locked task completed too late");

            Assert.IsTrue(called, "Locked statement not called");
        }


        [Test]
        public void Lock_Exception()
        {
            Assert.Inconclusive("Incomplete");
        }

        [Test]
        public async Task IsOnFiber()
        {
            // Fiber queue is empty
            Assert.IsFalse(this.fiber.IsOnFiber);

            var taskCompletionSource_FiberStarted = new TaskCompletionSource<IntPtr>();
            var taskCompletionSource_EndQueued = new TaskCompletionSource<IntPtr>();

            fiber.QueueToRun(async () =>
            {
                taskCompletionSource_FiberStarted.SetResult(IntPtr.Zero);
                await taskCompletionSource_EndQueued.Task;
            });

            // Fiber is running item on queue
            await taskCompletionSource_FiberStarted.Task;

            try
            {
                Assert.IsFalse(this.fiber.IsOnFiber);
            }
            finally
            {
                taskCompletionSource_EndQueued.SetResult(IntPtr.Zero);
            }

            // Running in the fiber
            await this.fiber.Lock(() => Assert.IsTrue(this.fiber.IsOnFiber));

            // Running in another task
            await Task.Run(() => Assert.IsFalse(this.fiber.IsOnFiber));

            // Running in a task started in the fiber
            await this.fiber.Lock(async () =>
            {
                await Task.Run(() => Assert.IsFalse(this.fiber.IsOnFiber));
            });
        }

        [Test]
        public async Task AssertOnFiber()
        {
            await this.fiber.Lock(this.fiber.AssertOnFiber);

            Assert.Throws<WrongFiberException>(this.fiber.AssertOnFiber);
        }
    }
}