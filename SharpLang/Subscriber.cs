using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpLang
{
    public struct Subscriber<TMessage>
    {
        private readonly IChannel<TMessage> channel;
        private readonly IFiber fiber;

        internal Subscriber(IChannel<TMessage> channel, IFiber fiber)
        {
            this.channel = channel;
            this.fiber = fiber;
        }

        public event AsyncMessageHandler<TMessage> PublishedAsync
        {
            add
            {
                this.channel.Subscribe(this.fiber, value);
            }
            remove
            {
                this.channel.Unsubscribe(this.fiber, value);
            }
        }

        public event MessageHandler<TMessage> Published
        {
            add
            {
                this.channel.Subscribe(this.fiber, value);
            }
            remove
            {
                this.channel.Unsubscribe(this.fiber, value);
            }
        }

        public IDisposable Once(AsyncMessageHandler<TMessage> handler)
        {
            var fiber = this.fiber;
            var me = this;

            var called = false;

            AsyncMessageHandler<TMessage> onceAdapter = null;
            onceAdapter = async (channel, message) =>
            {
                if (!called)
                {
                    me.PublishedAsync -= onceAdapter;
                    await handler(channel, message);
                    called = true;
                }
            };

            this.PublishedAsync += onceAdapter;

            return new Disposable(() => me.PublishedAsync -= onceAdapter);
        }

        public IDisposable Once(MessageHandler<TMessage> handler)
        {
            return this.Once((channel, message) =>
            {
                handler(channel, message);
                return Task.FromResult(IntPtr.Zero);
            });
        }

        public IDisposable Batched(TimeSpan delay, AsyncBatchMessageHandler<TMessage> handler)
        {
            var me = this;
            var fiber = this.fiber;

            List<TMessage> batch = null;

            AsyncMessageHandler<TMessage> batchedAdapter = (channel, message) =>
            {
                if (batch == null)
                {
                    batch = new List<TMessage>();

                    fiber.ScheduleOnce(delay, async () =>
                    {
                        await handler(channel, batch);
                        batch = null;
                    });
                }

                batch.Add(message);

                return Task.FromResult<IntPtr>(IntPtr.Zero);
            };

            this.PublishedAsync += batchedAdapter;

            return new Disposable(() => me.PublishedAsync -= batchedAdapter);
        }


        public IDisposable Batched(TimeSpan delay, BatchMessageHandler<TMessage> handler)
        {
            return this.Batched(delay, (channel, message) =>
            {
                handler(channel, message);
                return Task.FromResult(IntPtr.Zero);
            });
        }

        // First

        // Last

        // Keyed
    }
}
