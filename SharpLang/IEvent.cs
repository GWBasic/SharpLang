using System;
namespace SharpLang
{
    /// <summary>
    /// An entity that can be subscribed to
    /// </summary>
    public interface IEvent<TMessage>
    {
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
    }
}
