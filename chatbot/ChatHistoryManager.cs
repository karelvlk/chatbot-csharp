using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace chatbot
{
    /// <summary>
    /// The <c>ChatHistoryManager</c> class is responsible for managing the chat history
    /// of a chatbot session. It provides methods to store and load chat history, as
    /// well as list all available chat history files.
    /// </summary>
    public class ChatHistoryManager
    {
        private string historyDirectory;

        /// <summary>
        /// Constructs a new ChatHistoryManager object with the specified directory path.
        /// </summary>
        /// <param name="directoryPath">The path to the directory where chat history will be stored.</param>
        public ChatHistoryManager(string directoryPath)
        {
            this.historyDirectory = directoryPath;

            // Ensure the directory exists
            if (!Directory.Exists(historyDirectory))
            {
                try
                {
                    Directory.CreateDirectory(historyDirectory);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e.Message);
                    throw new InvalidOperationException("Failed to create history directory.");
                }
            }
        }

        /// <summary>
        /// Returns the file name for a chat session based on the session ID.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session.</param>
        /// <returns>The file name for the chat session.</returns>
        public string GetChatSessionFileName(string sessionId)
        {
            return $"chat-{sessionId}.yaml";
        }

        /// <summary>
        /// Stores the chat history and memory for a given session.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session.</param>
        /// <param name="chatHistory">The list of chat messages in the session.</param>
        /// <param name="memory">The list of memory items associated with the session.</param>
        public void StoreChatHistory(string sessionId, List<string> chatHistory, List<string> memory)
        {
            if (string.IsNullOrEmpty(sessionId) || chatHistory.Count == 0)
            {
                return;
            }
            string fileName = GetChatSessionFileName(sessionId);
            string filePath = Path.Combine(historyDirectory, fileName);

            ChatSession session = new ChatSession(chatHistory, memory);
            string yaml = session.ToYaml();
            try
            {
                File.WriteAllText(filePath, yaml);
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.WriteLine("Failed to save chat history.");
            }
        }

        /// <summary>
        /// Load a specific chat history by file name.
        /// </summary>
        /// <param name="fileName">The name of the file to load the chat history from.</param>
        /// <returns>The loaded chat session.</returns>
        public ChatSession LoadChatHistory(string fileName)
        {
            string filePath = Path.Combine(historyDirectory, fileName);
            try
            {
                string yaml = File.ReadAllText(filePath);
                return ChatSession.FromYaml(yaml);
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.WriteLine("Failed to load chat history from " + fileName);
                return new ChatSession(new List<string>(), new List<string>());
            }
        }

        /// <summary>
        /// Returns a list of chat histories.
        /// </summary>
        /// <returns>A list of chat histories.</returns>
        public List<string> ListChatHistories()
        {
            List<string> histories = new List<string>();
            try
            {
                histories = Directory.GetFiles(historyDirectory)
                                     .Where(file => Path.GetExtension(file) == ".yaml")
                                     .Select(Path.GetFileName)
                                     .OfType<string>()
                                     .ToList();
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.WriteLine("Failed to list chat histories.");
                histories = new List<string>();
            }
            return histories;
        }

        /// <summary>
        /// Inner class to represent a chat session including chat history and memory.
        /// </summary>
        public class ChatSession
        {
            /// <summary>
            /// Gets or sets the chat history.
            /// </summary>
            public List<string> ChatHistory { get; set; } = new List<string>();

            /// <summary>
            /// Gets or sets the memory.
            /// </summary>
            public List<string> Memory { get; set; } = new List<string>();

            /// <summary>
            /// Creates a new instance of the ChatSession class without history or memory.
            /// </summary>
            public ChatSession() { }

            /// <summary>
            /// Creates a new instance of the ChatSession class with the specified chat history and memory.
            /// </summary>
            /// <param name="chatHistory">The chat history.</param>
            /// <param name="memory">The memory.</param>
            public ChatSession(List<string> chatHistory, List<string> memory)
            {
                ChatHistory = chatHistory;
                Memory = memory;
            }

            /// <summary>
            /// Converts the <c>ChatSession</c> object to a YAML string representation.
            /// </summary>
            /// <returns>The YAML string representation of the <c>ChatSession</c> object including chat history and memory.</returns>
            public string ToYaml()
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                return serializer.Serialize(this);
            }

            /// <summary>
            /// Loads a <c>ChatSession</c> object from a YAML string.
            /// </summary>
            /// <param name="yamlString">The YAML string to load the <c>ChatSession</c> object from.</param>
            /// <returns>The <c>ChatSession</c> object loaded from the YAML string.</returns>
            public static ChatSession FromYaml(string yamlString)
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                return deserializer.Deserialize<ChatSession>(yamlString);
            }
        }
    }
}
