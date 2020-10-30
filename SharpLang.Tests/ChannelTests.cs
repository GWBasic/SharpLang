﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using SharpLang;

namespace SharpLang.Tests
{
    [TestFixture]
    public class ChannelTests
    {
        private Fiber fiber;
        private Channel<int> channel;

        public ChannelTests()
        {
        }

        [SetUp]
        public void SetUp()
        {
            fiber = new Fiber("test fiber");
            channel = new Channel<int>("test channel");
        }

        [Test]
        public async Task BasicSubscription()
        {
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
            MessageHandler<int> handler = (_, i) => taskCompletionSource.SetResult(i);

            this.channel.With(this.fiber).Published += handler;

            taskCompletionSource = new TaskCompletionSource<int>();
            await channel.Publish(2);
            Assert.AreEqual(2, await taskCompletionSource.Task);

            taskCompletionSource = new TaskCompletionSource<int>();
            await channel.Publish(3);
            Assert.AreEqual(3, await taskCompletionSource.Task);

            taskCompletionSource = new TaskCompletionSource<int>();
            await channel.Publish(4);
            Assert.AreEqual(4, await taskCompletionSource.Task);

            this.channel.With(this.fiber).Published -= handler;

            taskCompletionSource = new TaskCompletionSource<int>();
            await channel.Publish(5);
            await this.fiber.Wait();
            Assert.IsFalse(taskCompletionSource.Task.IsCompleted);
        }

        [Test]
        public async Task BasicSubscription_Async()
        {
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
            AsyncMessageHandler<int> handler = (_, i) =>
            {
                taskCompletionSource.SetResult(i);
                return Task.FromResult(IntPtr.Zero);
            };

            this.channel.With(this.fiber).PublishedAsync += handler;

            taskCompletionSource = new TaskCompletionSource<int>();
            await channel.Publish(2);
            Assert.AreEqual(2, await taskCompletionSource.Task);

            taskCompletionSource = new TaskCompletionSource<int>();
            await channel.Publish(3);
            Assert.AreEqual(3, await taskCompletionSource.Task);

            taskCompletionSource = new TaskCompletionSource<int>();
            await channel.Publish(4);
            Assert.AreEqual(4, await taskCompletionSource.Task);

            this.channel.With(this.fiber).PublishedAsync -= handler;

            taskCompletionSource = new TaskCompletionSource<int>();
            await channel.Publish(5);
            await this.fiber.Wait();
            Assert.IsFalse(taskCompletionSource.Task.IsCompleted);
        }

        [Test]
        public async Task Once()
        {
            int ctr = 0;

            this.channel.With(this.fiber).Once((_, __) => ctr++);

            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);

            await this.fiber.Wait();

            Assert.AreEqual(1, ctr);
        }

        [Test]
        public async Task Once_Disposed()
        {
            int ctr = 0;

            this.channel.With(this.fiber).Once((_, __) => ctr++).Dispose();

            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);
            await this.channel.Publish(0);

            await this.fiber.Wait();

            Assert.AreEqual(0, ctr);
        }

        [Test]
        public async Task Batched()
        {
            int actual = 0;

            using (this.channel.With(this.fiber).Batched(TimeSpan.FromSeconds(0.1), (_, vals) => actual += vals.Sum()))
            {
                for (var ctr = 0; ctr < 5; ctr++)
                {
                    await this.channel.Publish(1);
                    await this.channel.Publish(10);
                    await this.channel.Publish(100);

                    Assert.AreEqual(0, actual);

                    await Task.Delay(TimeSpan.FromSeconds(0.2));

                    Assert.AreEqual(111, actual);

                    actual = 0;
                }
            }

            await this.channel.Publish(666);
            await Task.Delay(TimeSpan.FromSeconds(0.2));
            Assert.AreEqual(0, actual);
        }
    }
}