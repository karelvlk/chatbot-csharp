using System;
using System.Collections.Generic;
using System.Linq;

namespace chatbot.MemoryManagers
{
    /// <summary>
    /// The <c>SummaryMemoryManager</c> class extends the <c>MemoryManager</c> abstract class and
    /// represents the memory component of a chatbot that summarizes the history.
    /// It keeps track of the chat history, context, and provides methods to
    /// manipulate and retrieve the chat history and context.
    /// </summary>
    public class SummaryMemoryManager : MemoryManager
    {
        private string[] context = new string[] { "" };
        private ChatManager chatManager;
        private int maxContextTokens;

        /// <summary>
        /// Constructs a new SummaryMemoryManager object with the specified maximum context
        /// tokens and chat manager.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of context tokens to store.</param>
        /// <param name="chatManager">The chat manager associated with this memory.</param>
        public SummaryMemoryManager(int maxContextTokens, ChatManager chatManager)
        {
            this.maxContextTokens = maxContextTokens;
            this.chatManager = chatManager;
        }

        /// <summary>
        /// Returns the context summary string.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of context tokens to include.</param>
        /// <returns>The context string.</returns>
        public override string GetContextString(int maxContextTokens)
        {
            return this.context[0];
        }

        /// <summary>
        /// Retrieves the list of strings of size 1 that contains context of the memory.
        /// </summary>
        /// <param name="maxContextTokens">The maximum number of context tokens to retrieve.</param>
        /// <returns>A list of strings of size 1 representing the context.</returns>
        public override List<string> GetContext(int maxContextTokens)
        {
            List<string> contextList = new List<string>();
            contextList.Add(context[0]);
            return contextList;
        }

        /// <summary>
        /// Sets the context for the SummaryMemoryManager.
        /// If the context size is less than or equal to 1, the first element of the
        /// context list is stored in the context array.
        /// If the context size is 0, an empty string is stored in the context array.
        /// If the context size is greater than 1, the history is summarized by LLM.
        /// </summary>
        /// <param name="context">The list of strings representing the context.</param>
        public override void SetContext(List<string> context)
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
        /// This method is called when an end AI message is received.
        /// It triggers the summarization of the chat history.
        /// </summary>
        public override void ReceivedEndAIMessage()
        {
            SummarizeHistory();
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
    }
}
