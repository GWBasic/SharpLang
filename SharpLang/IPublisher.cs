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
        /// Publishes the message. Waits until all synchronous handlers complete, does not block for asynchronous handlers
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Publish(TMessage message);
    }
}
