using System;

namespace chatbot.PromptGenerator
{
    /// <summary>
    /// This class represents a prompt generator for the TinyLLama chatbot.
    /// It extends the base class <c>PromptGenerator</c> and provides methods to generate
    /// prompts for the chatbot.
    /// </summary>
    public class TinyLLamaPromptGenerator : PromptGenerator
    {
        private string systemPrompt;

        /// <summary>
        /// Constructs a new TinyLLamaPromptGenerator object with the specified system
        /// prompt.
        /// </summary>
        /// <param name="systemPrompt">The system prompt to be used by the prompt generator.</param>
        public TinyLLamaPromptGenerator(string systemPrompt)
        {
            this.systemPrompt = systemPrompt;
        }

        /// <summary>
        /// Generates a prompt by formatting the given user input and context into a
        /// pattern.
        /// </summary>
        /// <param name="userInput">The user's input.</param>
        /// <param name="context">The context for the prompt.</param>
        /// <returns>The generated prompt.</returns>
        public override string GeneratePrompt(string userInput, string context)
        {
            string pattern = "\n{0}\n{1}\n\n{2}\n";
            return string.Format(pattern, this.systemPrompt, context, userInput);
        }

        /// <summary>
        /// Generates a prompt for chat summarization.
        /// </summary>
        /// <param name="userInput">The user input to be summarized.</param>
        /// <returns>The generated prompt for chat summarization.</returns>
        public override string GetSummarizePrompt(string userInput)
        {
            string pattern = "\nYou are chat summarization bot.\n\nSummarize: {0}\n";
            return string.Format(pattern, userInput);
        }
    }
}
