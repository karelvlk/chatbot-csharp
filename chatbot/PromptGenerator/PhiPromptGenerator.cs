using System;

namespace chatbot.PromptGenerator
{
    /// <summary>
    /// This class represents a prompt generator for the Phi chatbot. It extends the
    /// base <c>PromptGenerator</c> class.
    /// The <c>PhiPromptGenerator</c> generates prompts for the chatbot by formatting the
    /// system prompt, user input, and context.
    /// It also provides a method to generate a prompt for summarizing the chat.
    /// </summary>
    public class PhiPromptGenerator : PromptGenerator
    {
        private string systemPrompt;

        /// <summary>
        /// Creates a new instance of the <c>PhiPromptGenerator</c> class with the specified
        /// system prompt.
        /// </summary>
        /// <param name="systemPrompt">The system prompt to be used by the PhiPromptGenerator.</param>
        public PhiPromptGenerator(string systemPrompt)
        {
            this.systemPrompt = systemPrompt;
        }

        /// <summary>
        /// Generates a prompt for the chatbot conversation.
        /// </summary>
        /// <param name="userInput">The user's input.</param>
        /// <param name="context">The context of the conversation.</param>
        /// <returns>The generated prompt.</returns>
        public override string GeneratePrompt(string userInput, string context)
        {
            string pattern = "System:{0}\n{1}\nUser:{2}\nAI:";
            return string.Format(pattern, this.systemPrompt, context, userInput);
        }

        /// <summary>
        /// Generates a prompt for summarizing the chat based on the user input.
        /// </summary>
        /// <param name="userInput">The user input to be summarized.</param>
        /// <returns>The generated prompt for summarizing the chat.</returns>
        public override string GetSummarizePrompt(string userInput)
        {
            string pattern = "Instruct: Summarize the chat\n{0}\nOutput:";
            return string.Format(pattern, userInput);
        }
    }
}
