using System;
using System.Collections.Generic;
using System.Linq;

namespace chatbot.MemoryManagers
{
    /// <summary>
    /// The <c>BufferMemoryManager</c> class extends the <c>MemoryManager</c> abstract class and
    /// represents a memory manager that stores chat history in a buffer.
    /// It keeps track of the chat history and provides methods to add messages,
    /// retrieve context, and reset the memory.
    /// </summary>
    public class BufferMemoryManager : MemoryManager
    {
        /// <summary>
        /// Retrieves the context from the chat history. Context is a list of previous
        /// messages in maximum length of maxContextTokens.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of tokens allowed in the context.</param>
        /// <returns>A list of strings representing the context from the chat history.</returns>
        public override List<string> GetContext(int maxContextTokens)
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
        public override string GetContextString(int maxContextTokens)
        {
            return String.Join(", ", GetContext(maxContextTokens));
        }
    }
}
