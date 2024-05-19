using System;
using System.Collections.Generic;
using System.Linq;

namespace chatbot.Memory
{
    /// <summary>
    /// The <c>BufferMemory</c> class implements the <c>IMemoryManager</c> interface and
    /// represents a memory manager that stores chat history in a buffer.
    /// It keeps track of the chat history and provides methods to add messages,
    /// retrieve context, and reset the memory.
    /// </summary>
    public class BufferMemory : IMemoryManager
    {
        private LinkedList<string> chatHistory = new LinkedList<string>();

        /// <summary>
        /// Resets the buffer memory by clearing the chat history.
        /// </summary>
        public void Reset()
        {
            chatHistory.Clear();
        }

        /// <summary>
        /// Adds a new message to the chat history.
        /// If the last message in the chat history is "AI: " (added before calling LLM)
        /// and the new message is not from the user, the new message is concatenated to
        /// the last message because it must be in format Who: What. Otherwise, the new
        /// message is added as a separate entry in the chat history.
        /// </summary>
        /// <param name="message">The message to be added to the chat history.</param>
        public void AddMessage(string message)
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
        /// Retrieves the context from the chat history. Context is a list of previous
        /// messages in maximum length of maxContextTokens.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of tokens allowed in the context.</param>
        /// <returns>A list of strings representing the context from the chat history.</returns>
        public List<string> GetContext(int maxContextTokens)
        {
            int currentTokenCount = 0;
            LinkedList<string> context = new LinkedList<string>();

            // Iterate over the chatHistory from the end to the beginning
            var iterator = chatHistory.Reverse().GetEnumerator();
            while (iterator.MoveNext())
            {
                string message = iterator.Current;
                int messageTokenCount = message.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                // Check if adding this message would exceed the maxContextTokens
                if (currentTokenCount + messageTokenCount <= maxContextTokens)
                {
                    context.AddFirst(message); // Add message to the front
                    currentTokenCount += messageTokenCount;
                }
                else
                {
                    // If we can't add this message without exceeding the limit, stop
                    break;
                }
            }

            return context.ToList();
        }

        /// <summary>
        /// Returns a string representation of the context tokens up to the specified
        /// maximum number of tokens.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of context tokens to include in
        /// the string representation.</param>
        /// <returns>A string representation of the context tokens.</returns>
        public string GetContextString(int maxContextTokens)
        {
            return String.Join(", ", GetContext(maxContextTokens));
        }

        /// <summary>
        /// Returns the chat history as a list of strings.
        /// </summary>
        /// <returns>The chat history as a list of strings.</returns>
        public List<string> GetChatHistory()
        {
            return new List<string>(chatHistory);
        }

        /// <summary>
        /// Sets the chat history of the buffer memory.
        /// </summary>
        /// <param name="chatHistory">The list of strings representing the chat history.</param>
        public void SetChatHistory(List<string> chatHistory)
        {
            this.chatHistory = new LinkedList<string>(chatHistory);
        }

        /// <summary>
        /// This method is here only because of the interface; context is taken from history.
        /// </summary>
        /// <param name="context">The context to be set.</param>
        public void SetContext(List<string> context)
        {
            // Not necessary for BufferMemory
        }

        /// <summary>
        /// This method is called when the end of an AI message is received.
        /// It is not necessary for the BufferMemory implementation, so it does nothing.
        /// </summary>
        public void ReceivedEndAIMessage()
        {
            // Not necessary for BufferMemory
        }
    }
}
