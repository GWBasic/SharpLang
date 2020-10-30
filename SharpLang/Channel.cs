using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpLang
{
    public class Channel<TMessage> : IChannel<TMessage>
    {
        private object sync = new object();

        public Channel(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public Dictionary<IFiber, HashSet<AsyncMessageHandler<TMessage>>> asyncSubscriptions = new Dictionary<IFiber, HashSet<AsyncMessageHandler<TMessage>>>();
        public Dictionary<IFiber, HashSet<MessageHandler<TMessage>>> syncSubscriptions = new Dictionary<IFiber, HashSet<MessageHandler<TMessage>>>();

        public event AsyncMessageHandler<TMessage> BeforePublished;
        public event AsyncMessageHandler<TMessage> AfterPublished;

        public async Task Publish(TMessage message)
        {
            var beforePublished = this.BeforePublished;
            if (beforePublished != null)
            {
                await beforePublished.Invoke(this, message);
            }

            lock (this.sync)
            {
                foreach (var kvp in this.asyncSubscriptions)
                {
                    var subscriptionsForFiber = kvp.Value;
                    var fiber = kvp.Key;

                    foreach (var handler in subscriptionsForFiber)
                    {
                        fiber.QueueToRun(async () => await handler(this, message));
                    }
                }

                foreach (var kvp in this.syncSubscriptions)
                {
                    var subscriptionsForFiber = kvp.Value;
                    var fiber = kvp.Key;

                    foreach (var handler in subscriptionsForFiber)
                    {
                        fiber.QueueToRun(() => handler(this, message));
                    }
                }
            }

            var afterPublished = this.AfterPublished;
            if (afterPublished != null)
            {
                await afterPublished.Invoke(this, message);
            }
        }

        public Subscriber<TMessage> With(IFiber fiber)
        {
            return new Subscriber<TMessage>(this, fiber);
        }

        void IChannel<TMessage>.Subscribe(IFiber fiber, AsyncMessageHandler<TMessage> handler)
        {
            lock (this.sync)
            {
                if (!this.asyncSubscriptions.TryGetValue(fiber, out var subscriptionsForFiber))
                {
                    subscriptionsForFiber = new HashSet<AsyncMessageHandler<TMessage>>();
                    this.asyncSubscriptions[fiber] = subscriptionsForFiber;
                }

                subscriptionsForFiber.Add(handler);
            }
        }

        void IChannel<TMessage>.Unsubscribe(IFiber fiber, AsyncMessageHandler<TMessage> handler)
        {
            lock (this.sync)
            {
                if (this.asyncSubscriptions.TryGetValue(fiber, out var subscriptionsForFiber))
                {
                    subscriptionsForFiber.Remove(handler);

                    if (subscriptionsForFiber.Count == 0)
                    {
                        this.asyncSubscriptions.Remove(fiber);
                    }
                }
            }
        }

        void IChannel<TMessage>.Subscribe(IFiber fiber, MessageHandler<TMessage> handler)
        {
            lock (this.sync)
            {
                if (!this.syncSubscriptions.TryGetValue(fiber, out var subscriptionsForFiber))
                {
                    subscriptionsForFiber = new HashSet<MessageHandler<TMessage>>();
                    this.syncSubscriptions[fiber] = subscriptionsForFiber;
                }

                subscriptionsForFiber.Add(handler);
            }
        }

        void IChannel<TMessage>.Unsubscribe(IFiber fiber, MessageHandler<TMessage> handler)
        {
            lock (this.sync)
            {
                if (this.syncSubscriptions.TryGetValue(fiber, out var subscriptionsForFiber))
                {
                    subscriptionsForFiber.Remove(handler);

                    if (subscriptionsForFiber.Count == 0)
                    {
                        this.syncSubscriptions.Remove(fiber);
                    }
                }
            }
        }
    }
}
