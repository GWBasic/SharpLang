using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpLang
{
    /// <summary>
    /// Delegate type for handling a batch of messages
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <param name="batch">The batch of messages</param>
    public delegate Task BatchMessageHandler<TMessage>(IChannel<TMessage> channel, IEnumerable<TMessage> batch);
}