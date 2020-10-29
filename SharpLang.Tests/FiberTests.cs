using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using SharpLang;

namespace SharpLang.Tests
{
    public class FiberTests
    {
        Fiber fiber = new Fiber();
        List<Exception> exceptions;

        [SetUp]
        public void SetUp()
        {
            this.exceptions = new List<Exception>();
            this.fiber.ExceptionHandler = exceptions.Add;
        }

        [TearDown]
        public void TearDown()
        {
            Assert.AreEqual(0, this.exceptions.Count, "Unhandled exceptions in the fiber");
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

            Assert.Pass();
        }

        [Test]
        public async Task TestSchedule()
        {
            var taskCompletionSource = new TaskCompletionSource<IntPtr>();

            var start = DateTime.UtcNow;
            fiber.ScheduleOnce(TimeSpan.FromSeconds(1), () => taskCompletionSource.SetResult(IntPtr.Zero));

            await taskCompletionSource.Task;

            var completed = DateTime.UtcNow - start;

            Assert.GreaterOrEqual(completed.TotalSeconds, 0.9, "Scheduled task started too soon");
            Assert.LessOrEqual(completed.TotalSeconds, 1.1, "Scheduled task completed too late");

            Assert.Pass();
        }

        [Test]
        public async Task TestSchedule_Cancel()
        {
            var start = DateTime.UtcNow;
            var ran = false;
            fiber.ScheduleOnce(TimeSpan.FromSeconds(1), () => ran = true).Dispose();

            await Task.Delay(TimeSpan.FromSeconds(1.5));

            Assert.False(ran, "Task was not canceled");

            Assert.Pass();
        }



        // ScheduleOnInterval

        // Lock
    }
}