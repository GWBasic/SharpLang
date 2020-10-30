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

        /// <summary>
        /// Occurs when the channel is published
        /// </summary>
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

        /// <summary>
        /// Occurs when the channel is published
        /// </summary>
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

        /// <summary>
        /// Occurs once when the channel is published, automatically unsubscribed
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Occurs once when the channel is published, automatically unsubscribed
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable Once(MessageHandler<TMessage> handler)
        {
            return this.Once((channel, message) =>
            {
                handler(channel, message);
                return Task.FromResult(IntPtr.Zero);
            });
        }

        /// <summary>
        /// Subscribes to the channel, but handles batches of messages based on the delay
        /// </summary>
        /// <param name="delay">Once a message is published, how long to wait to accumulate additional messages before calling the handler</param>
        /// <param name="handler"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Subscribes to the channel, but handles batches of messages based on the delay
        /// </summary>
        /// <param name="delay">Once a message is published, how long to wait to accumulate additional messages before calling the handler</param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable Batched(TimeSpan delay, BatchMessageHandler<TMessage> handler)
        {
            return this.Batched(delay, (channel, message) =>
            {
                handler(channel, message);
                return Task.FromResult(IntPtr.Zero);
            });
        }

        /// <summary>
        /// Subscribes to the channel, but handles batches of messages based on the delay. Batches are keyed
        /// </summary>
        /// <param name="delay">Once a message is published, how long to wait to accumulate additional messages before calling the handler</param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable Keyed<TKey>(TimeSpan delay, Func<TMessage, TKey> getKey, AsyncKeyedMessageHandler<TMessage, TKey> handler)
        {
            var me = this;
            var fiber = this.fiber;

            Dictionary<TKey, TMessage> keyedMessages = null;

            AsyncMessageHandler<TMessage> batchedAdapter = (channel, message) =>
            {
                if (keyedMessages == null)
                {
                    keyedMessages = new Dictionary<TKey, TMessage>();

                    fiber.ScheduleOnce(delay, async () =>
                    {
                        await handler(channel, keyedMessages);
                        keyedMessages = null;
                    });
                }

                keyedMessages[getKey(message)] = message;

                return Task.FromResult<IntPtr>(IntPtr.Zero);
            };

            this.PublishedAsync += batchedAdapter;

            return new Disposable(() => me.PublishedAsync -= batchedAdapter);
        }


        /// <summary>
        /// Subscribes to the channel, but handles batches of messages based on the delay. Batches are keyed
        /// </summary>
        /// <param name="delay">Once a message is published, how long to wait to accumulate additional messages before calling the handler</param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable Keyed<TKey>(TimeSpan delay, Func<TMessage, TKey> getKey, KeyedMessageHandler<TMessage, TKey> handler)
        {
            return this.Keyed(delay, getKey, (channel, message) =>
            {
                handler(channel, message);
                return Task.FromResult(IntPtr.Zero);
            });
        }

        /// <summary>
        /// Subscribes to the channel, but only handles the first message in a given time period
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable First(TimeSpan delay, AsyncMessageHandler<TMessage> handler)
        {
            var me = this;
            var fiber = this.fiber;

            var delayed = false;
            TMessage firstMessage = default(TMessage);

            AsyncMessageHandler<TMessage> batchedAdapter = (channel, message) =>
            {
                if (!delayed)
                {
                    delayed = true;
                    firstMessage = message;

                    fiber.ScheduleOnce(delay, async () =>
                    {
                        await handler(channel, firstMessage);
                        firstMessage = default;
                        delayed = false;
                    });
                }

                return Task.FromResult<IntPtr>(IntPtr.Zero);
            };

            this.PublishedAsync += batchedAdapter;

            return new Disposable(() => me.PublishedAsync -= batchedAdapter);
        }

        /// <summary>
        /// Subscribes to the channel, but only handles the first message in a given time period
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable First(TimeSpan delay, MessageHandler<TMessage> handler)
        {
            return this.First(delay, (channel, message) =>
            {
                handler(channel, message);
                return Task.FromResult(IntPtr.Zero);
            });
        }

        /// <summary>
        /// Subscribes to the channel, but only handles the last message in a given time period
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable Last(TimeSpan delay, AsyncMessageHandler<TMessage> handler)
        {
            var me = this;
            var fiber = this.fiber;

            var accumulating = false;
            TMessage lastMessage = default(TMessage);

            AsyncMessageHandler<TMessage> batchedAdapter = (channel, message) =>
            {
                if (!accumulating)
                {
                    accumulating = true;

                    fiber.ScheduleOnce(delay, async () =>
                    {
                        await handler(channel, lastMessage);
                        lastMessage = default;
                        accumulating = false;
                    });
                }

                lastMessage = message;

                return Task.FromResult<IntPtr>(IntPtr.Zero);
            };

            this.PublishedAsync += batchedAdapter;

            return new Disposable(() => me.PublishedAsync -= batchedAdapter);
        }

        /// <summary>
        /// Subscribes to the channel, but only handles the first message in a given time period
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable Last(TimeSpan delay, MessageHandler<TMessage> handler)
        {
            return this.Last(delay, (channel, message) =>
            {
                handler(channel, message);
                return Task.FromResult(IntPtr.Zero);
            });
        }
    }
}
