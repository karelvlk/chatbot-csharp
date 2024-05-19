using System;

namespace chatbot.PromptGenerators
{
    /// <summary>
    /// The IPromptGenerator interface provides methods for generating prompts in a chatbot.
    /// </summary>
    public interface IPromptGenerator
    {
        /// <summary>
        /// Generates a prompt based on the user input and context.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        /// <param name="context">The context of the conversation.</param>
        /// <returns>The generated prompt.</returns>
        string GeneratePrompt(string userInput, string context);

        /// <summary>
        /// Generates a prompt for summarizing the user input.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        /// <returns>The generated prompt for summarization.</returns>
        string GetSummarizePrompt(string userInput);
    }
}
