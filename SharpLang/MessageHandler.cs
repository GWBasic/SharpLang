using System;
using System.Threading.Tasks;

namespace SharpLang
{
    /// <summary>
    /// Delegate type for all messages
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <param name="message">The message</param>
    public delegate Task MessageHandler<TMessage>(IChannel<TMessage> channel, TMessage message);
}