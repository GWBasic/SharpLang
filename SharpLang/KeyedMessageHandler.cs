using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpLang
{
    /// <summary>
    /// Delegate type for handling a keyed batch of messages
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <param name="batch">The batch of messages</param>
    public delegate void KeyedMessageHandler<TMessage, TKey>(IChannel<TMessage> channel, IDictionary<TKey, TMessage> keyedMessages);
}