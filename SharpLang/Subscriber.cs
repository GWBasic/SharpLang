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

        public static Subscriber<TMessage> operator +(Subscriber<TMessage> subscriber, MessageHandler<TMessage> handler)
        {
            subscriber.channel.Subscribe(subscriber.fiber, handler);
            return subscriber;
        }

        public static Subscriber<TMessage> operator -(Subscriber<TMessage> subscriber, MessageHandler<TMessage> handler)
        {
            subscriber.channel.Unsubscribe(subscriber.fiber, handler);
            return subscriber;
        }

        public void Once(MessageHandler<TMessage> handler)
        {
            var fiber = this.fiber;
            var me = this;

            var called = false;

            MessageHandler<TMessage> onceAdapter = null;
            onceAdapter = async (channel, message) =>
            {
                if (!called)
                {
                    me -= onceAdapter;
                    await handler(channel, message);
                    called = true;
                }
            };

            this += onceAdapter;
        }

        public IDisposable Batched(TimeSpan delay, BatchMessageHandler<TMessage> handler)
        {
            var me = this;
            var fiber = this.fiber;

            List<TMessage> batch = null;

            MessageHandler<TMessage> batchedAdapter = (channel, message) =>
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

            this += batchedAdapter;

            return new Disposable(() => me -= batchedAdapter);
        }
    }
}
