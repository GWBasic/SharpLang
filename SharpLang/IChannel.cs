using System;
using System.Threading.Tasks;

namespace SharpLang
{
    /// <summary>
    /// A channel is an event that can be both published and subscribed to
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IChannel<TMessage> : IEvent<TMessage>, IPublisher<TMessage>
    {
        internal void Subscribe(IFiber fiber, AsyncMessageHandler<TMessage> handler);
        internal void Unsubscribe(IFiber fiber, AsyncMessageHandler<TMessage> handler);

        internal void Subscribe(IFiber fiber, MessageHandler<TMessage> handler);
        internal void Unsubscribe(IFiber fiber, MessageHandler<TMessage> handler);
    }
}
