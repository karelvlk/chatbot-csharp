using System;

namespace chatbot
{
    /// <summary>
    /// The IMessageConsumer interface represents an object that can consume messages.
    /// Implementations of this interface can be used to process incoming messages in a chatbot system.
    /// </summary>
    public interface IMessageConsumer
    {
        /// <summary>
        /// Accepts a message for processing.
        /// </summary>
        /// <param name="message">The message to be processed.</param>
        void AcceptMessage(string message);
    }
}
