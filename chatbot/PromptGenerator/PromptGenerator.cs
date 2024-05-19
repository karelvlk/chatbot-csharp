using System;

namespace chatbot.PromptGenerator
{
    /// <summary>
    /// The abstract class <c>PromptGenerator</c> is the base class for generating prompts
    /// in a chatbot.
    /// It provides a common structure and behavior for prompt generators.
    /// </summary>
    public abstract class PromptGenerator : IPromptGenerator
    {
        /// <summary>
        /// Constructs a new PromptGenerator object.
        /// </summary>
        protected PromptGenerator()
        {
        }

        /// <summary>
        /// Generates a prompt based on the user input and context.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        /// <param name="context">The context of the conversation.</param>
        /// <returns>The generated prompt.</returns>
        public abstract string GeneratePrompt(string userInput, string context);

        /// <summary>
        /// Generates a prompt for chat summarization.
        /// </summary>
        /// <param name="userInput">The user input to be summarized.</param>
        /// <returns>The generated prompt for chat summarization.</returns>
        public abstract string GetSummarizePrompt(string userInput);
    }
}
