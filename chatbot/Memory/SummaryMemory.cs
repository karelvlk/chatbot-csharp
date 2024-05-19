using System;
using System.Collections.Generic;
using System.Linq;

namespace chatbot.Memory
{
    /// <summary>
    /// The <c>SummaryMemory</c> class implements the <c>IMemoryManager</c> interface and
    /// represents the memory component of a chatbot.
    /// It keeps track of the chat history, context, and provides methods to
    /// manipulate and retrieve the chat history and context.
    /// </summary>
    public class SummaryMemory : IMemoryManager
    {
        private LinkedList<string> chatHistory = new LinkedList<string>();
        private string[] context = new string[] { "" };
        private ChatManager chatManager;
        private int maxContextTokens;

        /// <summary>
        /// Constructs a new SummaryMemory object with the specified maximum context
        /// tokens and chat manager.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of context tokens to store.</param>
        /// <param name="chatManager">The chat manager associated with this memory.</param>
        public SummaryMemory(int maxContextTokens, ChatManager chatManager)
        {
            this.maxContextTokens = maxContextTokens;
            this.chatManager = chatManager;
        }

        /// <summary>
        /// Resets the summary memory by clearing the chat history.
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
        /// Returns the context summary string.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of context tokens to include.</param>
        /// <returns>The context string.</returns>
        public string GetContextString(int maxContextTokens)
        {
            return this.context[0];
        }

        /// <summary>
        /// Retrieves the list of strings of size 1 that contains context of the memory.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of context tokens to retrieve.</param>
        /// <returns>A list of strings of size 1 representing the context.</returns>
        public List<string> GetContext(int maxContextTokens)
        {
            List<string> contextList = new List<string>();
            contextList.Add(context[0]);
            return contextList;
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
        /// Sets the chat history of the SummaryMemory object.
        /// </summary>
        /// <param name="chatHistory">The list of strings representing the chat history.</param>
        public void SetChatHistory(List<string> chatHistory)
        {
            this.chatHistory = new LinkedList<string>(chatHistory);
        }

        /// <summary>
        /// Sets the context for the SummaryMemory.
        /// If the context size is less than or equal to 1, the first element of the
        /// context list is stored in the context array.
        /// If the context size is 0, an empty string is stored in the context array.
        /// If the context size is greater than 1, the history is summarized by LLM.
        /// </summary>
        /// <param name="context">The list of strings representing the context.</param>
        public void SetContext(List<string> context)
        {
            if (context.Count <= 1)
            {
                this.context[0] = context.Count > 0 ? context[0] : "";
            }
            else
            {
                SummarizeHistory();
            }
        }

        /// <summary>
        /// Returns a string representing the history context of the chat.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of tokens allowed in the context.</param>
        /// <returns>The history context as a string.</returns>
        private string GetHistoryContext(int maxContextTokens)
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

            return string.Join(" ", context);
        }

        /// <summary>
        /// Summarizes the chat history. The summarized context is obtained by calling
        /// the <c>Summarize</c> method of the <c>chatManager</c> object. If the chat history
        /// context is empty, an empty string is stored in the memory.
        /// </summary>
        private void SummarizeHistory()
        {
            string chatHistoryContext = GetHistoryContext(maxContextTokens - 30);
            if (chatHistoryContext.Length > 0)
            {
                context[0] = chatManager.Summarize(chatHistoryContext);
            }
            else
            {
                context[0] = "";
            }
        }

        /// <summary>
        /// This method is called when an end AI message is received.
        /// It triggers the summarization of the chat history.
        /// </summary>
        public void ReceivedEndAIMessage()
        {
            SummarizeHistory();
        }
    }
}
