using System.Collections.Generic;

namespace chatbot.MemoryManagers
{
    /// <summary>
    /// The <c>IMemoryManager</c> interface represents a memory manager for a chatbot.
    /// It provides methods to add messages, reset the memory, retrieve context
    /// information, and manage the chat history.
    /// </summary>
    public interface IMemoryManager
    {
        /// <summary>
        /// Adds a message to the memory.
        /// </summary>
        /// <param name="message">The message to be added.</param>
        void AddMessage(string message);

        /// <summary>
        /// Resets the memory, clearing all stored information.
        /// </summary>
        void Reset();

        /// <summary>
        /// Retrieves the context string up to the specified maximum number of tokens.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of tokens to retrieve.</param>
        /// <returns>The context string.</returns>
        string GetContextString(int maxContextTokens);

        /// <summary>
        /// Retrieves the chat history.
        /// </summary>
        /// <returns>The chat history.</returns>
        List<string> GetChatHistory();

        /// <summary>
        /// Retrieves the context up to the specified maximum number of tokens.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of tokens to retrieve.</param>
        /// <returns>The context.</returns>
        List<string> GetContext(int maxContextTokens);

        /// <summary>
        /// Sets the chat history.
        /// </summary>
        /// <param name="chatHistory">The chat history to set.</param>
        void SetChatHistory(List<string> chatHistory);

        /// <summary>
        /// Sets the context.
        /// </summary>
        /// <param name="context">The context to set.</param>
        void SetContext(List<string> context);

        /// <summary>
        /// Notifies the memory manager that an end AI message has been received.
        /// </summary>
        void ReceivedEndAIMessage();
    }
}
