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

        public Dictionary<IFiber, HashSet<MessageHandler<TMessage>>> subscriptions = new Dictionary<IFiber, HashSet<MessageHandler<TMessage>>>();

        public event MessageHandler<TMessage> BeforePublished;
        public event MessageHandler<TMessage> AfterPublished;

        public async Task Publish(TMessage message)
        {
            await this.BeforePublished?.Invoke(this, message);

            lock (this.sync)
            {
                foreach (var kvp in this.subscriptions)
                {
                    var subscriptionsForFiber = kvp.Value;
                    var fiber = kvp.Key;

                    foreach (var handler in subscriptionsForFiber)
                    {
                        fiber.QueueToRun(async () => await handler(this, message));
                    }
                }
            }

            await this.AfterPublished?.Invoke(this, message);
        }

        public Subscriber<TMessage> With(IFiber fiber)
        {
            return new Subscriber<TMessage>(this, fiber);
        }

        void IChannel<TMessage>.Subscribe(IFiber fiber, MessageHandler<TMessage> handler)
        {
            lock (this.sync)
            {
                if (!this.subscriptions.TryGetValue(fiber, out var subscriptionsForFiber))
                {
                    subscriptionsForFiber = new HashSet<MessageHandler<TMessage>>();
                    this.subscriptions[fiber] = subscriptionsForFiber;
                }

                subscriptionsForFiber.Add(handler);
            }
        }

        void IChannel<TMessage>.Unsubscribe(IFiber fiber, MessageHandler<TMessage> handler)
        {
            lock (this.sync)
            {
                if (this.subscriptions.TryGetValue(fiber, out var subscriptionsForFiber))
                {
                    subscriptionsForFiber.Remove(handler);

                    if (subscriptionsForFiber.Count == 0)
                    {
                        this.subscriptions.Remove(fiber);
                    }
                }
            }
        }
    }
}
