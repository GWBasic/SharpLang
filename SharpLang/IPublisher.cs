using System;
using System.Threading.Tasks;

namespace SharpLang
{
    /// <summary>
    /// An entity that can publish messages
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IPublisher<TMessage>
    {
        /// <summary>
        /// Publishes the message. Waits until all synchronous handlers complete, does not block for asynchronous handlers.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <remarks>Tasks will be queued into fibers before this returns</remarks>
        Task Publish(TMessage message);
    }
}
