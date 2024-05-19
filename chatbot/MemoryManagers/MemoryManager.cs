using System;
using System.Collections.Generic;
using System.Linq;

namespace chatbot.MemoryManagers
{
    /// <summary>
    /// Represents an abstract base class for memory managers in the chatbot system that 
    /// implements <c>IMemoryManager</c> inferface.
    /// </summary>
    public abstract class MemoryManager : IMemoryManager
    {
        protected LinkedList<string> chatHistory = new LinkedList<string>();

        /// <summary>
        /// Adds a new message to the chat history.
        /// If the last message in the chat history is "AI: " (added before calling LLM)
        /// and the new message is not from the user, the new message is concatenated to
        /// the last message because it must be in format Who: What. Otherwise, the new
        /// message is added as a separate entry in the chat history.
        /// </summary>
        /// <param name="message">The message to be added to the chat history.</param>
        public virtual void AddMessage(string message)
        {
            if (chatHistory.Any() &&
                ((chatHistory.Last?.Value.Equals("AI: ") == true) ||
                (chatHistory.Last?.Value.StartsWith("AI: ") == true && !message.StartsWith("User: "))))
            {
                // Concatenate the new message to the last message
                string lastMessage = chatHistory.Last.Value;
                chatHistory.RemoveLast();
                chatHistory.AddLast(lastMessage + " " + message);
            }
            else
            {
                chatHistory.AddLast(message);
            }
        }

        /// <summary>
        /// Resets the summary memory by clearing the chat history.
        /// </summary>
        public virtual void Reset()
        {
            chatHistory.Clear();
        }

        /// <summary>
        /// This method is called when the end of an AI message is received.
        /// It does nothing in default.
        /// </summary>
        public virtual void ReceivedEndAIMessage()
        {
            return;
        }

        /// <summary>
        /// This method is called when the context is set. In default, it does nothing.
        /// </summary>
        /// <param name="context">The context to be set.</param>
        public virtual void SetContext(List<string> context)
        {
            return;
        }

        /// <summary>
        /// Returns the chat history as a list of strings.
        /// </summary>
        /// <returns>The chat history as a list of strings.</returns>
        public virtual List<string> GetChatHistory()
        {
            return new List<string>(chatHistory);
        }

        /// <summary>
        /// Sets the chat history of the SummaryMemoryManager object.
        /// </summary>
        /// <param name="chatHistory">The list of strings representing the chat history.</param>
        public virtual void SetChatHistory(List<string> chatHistory)
        {
            this.chatHistory = new LinkedList<string>(chatHistory);
        }

        /// <summary>
        /// Returns the context summary string.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of context tokens to include.</param>
        /// <returns>The context string.</returns>
        public abstract string GetContextString(int maxContextTokens);

        /// <summary>
        /// Retrieves the context up to the specified maximum number of tokens.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of tokens to retrieve.</param>
        /// <returns>The context.</returns>
        public abstract List<string> GetContext(int maxContextTokens);


    }
}
