using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using chatbot.MemoryManagers;
using chatbot.PromptGenerators;

namespace chatbot
{
    /// <summary>
    /// The <c>ChatManager</c> class is responsible for managing the chat functionality of
    /// the chatbot. It handles the communication between the user and the AI model,
    /// manages the chat history, and provides methods for updating the model and
    /// memory settings.
    /// </summary>
    public class ChatManager
    {
        private IMemoryManager memoryManager;
        private IPromptGenerator promptGenerator;
        private ChatHistoryManager chatHistoryManager;
        private LLMClient llmClient;
        private IMessageConsumer messageConsumer; // Reference to the CLI interface
        private string? sessionId = null;
        private int maxTokens;
        private SettingsManager settingsManager;

        // Add fields related to the loading animation
        private Thread? animationThread;  // Declare as nullable
        private volatile bool running;

        /// <summary>
        /// Constructs a new instance of the ChatManager class.
        /// </summary>
        /// <param name="settingsManager">The settings manager used to retrieve configuration settings.</param>
        /// <param name="messageConsumer">The message consumer used to consume chat messages.</param>
        public ChatManager(SettingsManager settingsManager, IMessageConsumer messageConsumer)
        {
            this.maxTokens = int.Parse(settingsManager.GetSetting("maxTotalTokens"));
            this.memoryManager = BuildMemoryManager(settingsManager);
            this.chatHistoryManager = BuildChatHistoryManager(settingsManager);
            this.promptGenerator = BuildPromptGenerator(settingsManager);
            this.messageConsumer = new MessageConsumerWithStopAnimation(messageConsumer, this); // Initialize the message consumer
            this.llmClient = BuildLLMClient(settingsManager);
            this.settingsManager = settingsManager;
        }

        /// <summary>
        /// Builds and returns an instance of the <c>LLMClient</c> class.
        /// </summary>
        /// <param name="settingsManager">The settings manager to be used by the <c>LLMClient</c>.</param>
        /// <returns>An instance of the <c>LLMClient</c> class.</returns>
        private LLMClient BuildLLMClient(SettingsManager settingsManager)
        {
            return new LLMClient(settingsManager, messageConsumer);
        }

        /// <summary>
        /// Builds and returns an instance of the <c>PromptGenerator</c> class based on the model type.
        /// </summary>
        /// <param name="settingsManager">The settings manager to be used by the <c>PromptGenerator</c>.</param>
        /// <returns>An instance of the <c>PromptGenerator</c> class.</returns>
        private PromptGenerator BuildPromptGenerator(SettingsManager settingsManager)
        {
            if (settingsManager.GetSetting("model").Contains("Phi2"))
            {
                return new PhiPromptGenerator(settingsManager.GetSetting("systemPromptPhi"));
            }
            else if (settingsManager.GetSetting("model").Contains("TinyLlama"))
            {
                return new TinyLLamaPromptGenerator(settingsManager.GetSetting("systemPromptTinyLlama"));
            }
            else
            {
                throw new ArgumentException("Unknown model or prompt type");
            }
        }

        /// <summary>
        /// Builds and returns an instance of the <c>ChatHistoryManager</c> class.
        /// </summary>
        /// <param name="settingsManager">The settings manager to be used by the <c>ChatHistoryManager</c>.</param>
        /// <returns>An instance of the <c>ChatHistoryManager</c> class.</returns>
        private ChatHistoryManager BuildChatHistoryManager(SettingsManager settingsManager)
        {
            return new ChatHistoryManager(settingsManager.GetSetting("historyPath"));
        }

        /// <summary>
        /// Builds and returns an instance of the <c>MemoryManager</c> class based on the memory type.
        /// </summary>
        /// <param name="settingsManager">The settings manager to be used by the <c>IMemoryManager</c>.</param>
        /// <returns>An instance of the MemoryManager class.</returns>
        private MemoryManager BuildMemoryManager(SettingsManager settingsManager)
        {
            if (settingsManager.GetSetting("memory").Equals("summary"))
            {
                return new SummaryMemoryManager(this.maxTokens, this);
            }
            else if (settingsManager.GetSetting("memory").Equals("buffer"))
            {
                return new BufferMemoryManager();
            }
            else
            {
                throw new ArgumentException("Unknown memory type: " + settingsManager.GetSetting("memory"));
            }
        }

        /// <summary>
        /// Adds a message to the memory manager's chat history.
        /// </summary>
        /// <param name="message">The message to be added to the history.</param>
        public void AddToHistory(string message)
        {
            memoryManager.AddMessage(message);
        }

        /// <summary>
        /// Method to be called when the AI model sends the whole message.
        /// </summary>
        public void ReceivedEndAIMessage()
        {

            memoryManager.ReceivedEndAIMessage();
        }

        /// <summary>
        /// Updates the model used by the chatbot. This method updates the settings
        /// manager, ends the current chat session, rebuilds the prompt generator with
        /// the updated settings, and starts a new chat session.
        /// </summary>
        /// <param name="model">The new model to be used.</param>
        public void UpdateModel(ModelType model)
        {
            settingsManager.UpdateModel(model);
            EndChatSession();
            promptGenerator = BuildPromptGenerator(settingsManager);
            llmClient.InitializeLLM();
            StartChatSession();
        }

        /// <summary>
        /// Updates the memory of the chat manager with the specified memory type.
        /// This method updates the settings manager, ends the current chat session,
        /// rebuilds the memory manager with the updated settings, and starts a new chat session.
        /// </summary>
        /// <param name="memory">The memory type to update the chat manager with.</param>
        public void UpdateMemory(MemoryType memory)
        {
            settingsManager.UpdateMemory(memory);
            EndChatSession();
            memoryManager = BuildMemoryManager(settingsManager);
            StartChatSession();
        }

        /// <summary>
        /// Ends the current chat session by storing the chat history and context.
        /// If there is no active session, this method does nothing.
        /// </summary>
        public void EndChatSession()
        {
            if (sessionId == null)
            {
                return;
            }

            chatHistoryManager.StoreChatHistory(sessionId, memoryManager.GetChatHistory(),
                memoryManager.GetContext(this.maxTokens - 30));
            sessionId = null; // Clear session id after storing the history
        }

        /// <summary>
        /// Returns the model type from setting manager.
        /// </summary>
        /// <returns>The model type as a String.</returns>
        public string GetModelType()
        {
            return settingsManager.GetSetting("model");
        }

        /// <summary>
        /// Returns the memory type from setting manager.
        /// </summary>
        /// <returns>The memory type setting as a String.</returns>
        public string GetMemoryType()
        {
            return settingsManager.GetSetting("memory");
        }

        /// <summary>
        /// Loads the chat history from the specified file. Chat history and context are
        /// set in the memory manager.
        /// </summary>
        /// <param name="fileName">The name of the file containing the chat history.</param>
        public void LoadHistory(string fileName)
        {
            var pattern = new Regex("\\d+");
            var matcher = pattern.Match(fileName);

            if (matcher.Success)
            {
                this.sessionId = matcher.Value;
            }

            var session = chatHistoryManager.LoadChatHistory(fileName);
            List<string> history = session.ChatHistory;
            List<string> context = session.Memory;

            memoryManager.SetChatHistory(history);
            memoryManager.SetContext(context);

            foreach (string message in history)
            {
                if (message.StartsWith("User: "))
                {
                    messageConsumer.AcceptMessage(message.Replace("User: ", "You: "));
                }
                else
                {
                    messageConsumer.AcceptMessage(message);
                }

                messageConsumer.AcceptMessage("\n");
            }
        }

        /// <summary>
        /// Returns a list of chat histories.
        /// </summary>
        /// <returns>A list of chat histories.</returns>
        public List<string> ListHistories()
        {
            return chatHistoryManager.ListChatHistories();
        }

        /// <summary>
        /// Starts a new chat session.
        /// Generates a new session ID based on the current system time and resets the
        /// memory manager.
        /// </summary>
        public void StartChatSession()
        {
            this.sessionId = DateTime.UtcNow.Ticks.ToString();
            memoryManager.Reset();
        }

        /// <summary>
        /// Processes the user input.
        /// </summary>
        /// <param name="input">The user input to process.</param>
        public void ProcessInput(string input)
        {
            try
            {
                string prompt = promptGenerator.GeneratePrompt(input, memoryManager.GetContextString(GetMaxTokensForContext(input)));
                AddToHistory("User: " + input);
                messageConsumer.AcceptMessage("AI: ");
                StartLoadingAnimation();
                llmClient.SendQuery(prompt);
            }
            catch (Exception e)
            {
                messageConsumer.AcceptMessage("Error: " + e.Message);
            }
        }

        /// <summary>
        /// Returns the maximum number of tokens allowed for a given input context.
        /// The maximum number of tokens is calculated by subtracting the length of the
        /// input string (split by spaces) from the maximum tokens value, and then
        /// subtracting an additional 30 tokens.
        /// </summary>
        /// <param name="input">The input string representing the context.</param>
        /// <returns>The maximum number of tokens allowed for the given context.</returns>
        private int GetMaxTokensForContext(string input)
        {
            return maxTokens - input.Split(" ").Length - 30;
        }

        /// <summary>
        /// Summarizes the given input using a prompt generator and sends the query to
        /// the LLM client.
        /// </summary>
        /// <param name="input">The input to be summarized.</param>
        /// <returns>The summarized output as a string.</returns>
        public string Summarize(string input)
        {
            string prompt = promptGenerator.GetSummarizePrompt(input);
            return llmClient.SendQuerySync(prompt);
        }

        /// <summary>
        /// Starts the loading animation.
        /// The animation consists of a rotating set of characters that are printed to
        /// the console.
        /// The animation continues until the <c>running</c> flag is set to false.
        /// The animation is cleared from the terminal once it is stopped.
        /// </summary>
        private void StartLoadingAnimation()
        {
            running = true;
            animationThread = new Thread(() =>
            {
                string[] animationChars = { "|", "/", "-", "\\" };
                int i = 0;
                while (running)
                {
                    Console.Write("\rAI: " + animationChars[i++ % animationChars.Length]);
                    try
                    {
                        Thread.Sleep(100);
                    }
                    catch (ThreadInterruptedException)
                    {
                        Thread.CurrentThread.Interrupt();
                    }
                }
                // Clear the animation from the terminal
                Console.Write("\rAI: \rAI: ");
            });
            animationThread.Start();
        }

        /// <summary>
        /// Stops the loading animation and interrupts the animation thread.
        /// </summary>
        private void StopLoadingAnimation()
        {
            running = false;
            if (animationThread != null)
            {
                animationThread.Join();
            }
        }

        /// <summary>
        /// A decorator class that stops the loading animation before accepting and
        /// processing a message.
        /// </summary>
        private class MessageConsumerWithStopAnimation : IMessageConsumer
        {
            private IMessageConsumer original;
            private ChatManager parent;

            public MessageConsumerWithStopAnimation(IMessageConsumer original, ChatManager parent)
            {
                this.original = original;
                this.parent = parent;
            }

            /// <summary>
            /// Accepts a message and performs necessary actions.
            /// This method stops the loading animation before showing the message.
            /// </summary>
            /// <param name="message">The message to be accepted.</param>
            public void AcceptMessage(string message)
            {
                // Stop the loading animation before showing the message
                parent.StopLoadingAnimation();
                original.AcceptMessage(message);
            }
        }

        /// <summary>
        /// Initializes the ChatManager's LLMClient.
        /// </summary>
        public void Initialize()
        {
            Console.WriteLine("Initializing ChatManager's LLMClient...");
            try
            {
                if (llmClient.InitializeLLM())
                {
                    Console.WriteLine("LLM initialized successfully");
                }
                else
                {
                    Console.WriteLine("LLM initialization failed");
                    throw new InvalidOperationException("LLM initialization failed");
                }
            }
            catch (Exception e)
            {
                messageConsumer.AcceptMessage("Failed to run ChatManager: " + e.Message);
            }
        }
    }
}
