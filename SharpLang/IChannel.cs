using System;
using System.Threading.Tasks;

namespace SharpLang
{
    /// <summary>
    /// A channel represents an event in SharpLang
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IChannel<TMessage>
    {
        /// <summary>
        /// Publishes the message. Waits until all synchronous handlers complete, does not block for asynchronous handlers
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Publish(TMessage message);

        /// <summary>
        /// Allows handling the message on the thread (fiber) before the message is published to other fibers
        /// </summary>
        event MessageHandler<TMessage> BeforePublished;

        /// <summary>
        /// Allows handling the message on the thread (fiber) after the message is published is published to other fibers
        /// </summary>
        event MessageHandler<TMessage> AfterPublished;

        /// <summary>
        /// Use to subscribe to the channel on a fiber
        /// </summary>
        /// <param name="fiber"></param>
        /// <returns></returns>
        Subscriber<TMessage> With(IFiber fiber);

        internal void Subscribe(IFiber fiber, MessageHandler<TMessage> handler);
        internal void Unsubscribe(IFiber fiber, MessageHandler<TMessage> handler);
    }
}
