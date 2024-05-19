using System;

namespace chatbot.PromptGenerators
{
    /// <summary>
    /// The abstract class <c>PromptGenerator</c> is the base class for generating prompts
    /// in a chatbot.
    /// It provides a common structure and behavior for prompt generators.
    /// </summary>
    public abstract class PromptGenerator : IPromptGenerator
    {
        /// <summary>
        /// Generates a base prompt based on the user input and context.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        /// <param name="context">The context of the conversation.</param>
        /// <returns>The generated prompt.</returns>
        public virtual string GeneratePrompt(string userInput, string context)
        {
            return "User: " + userInput + "\nAI:";
        }

        /// <summary>
        /// Generates a base prompt for chat summarization.
        /// </summary>
        /// <param name="userInput">The user input to be summarized.</param>
        /// <returns>The generated prompt for chat summarization.</returns>
        public virtual string GetSummarizePrompt(string userInput)
        {
            return "Summarize: " + userInput + "\nAI:";
        }
    }
}
