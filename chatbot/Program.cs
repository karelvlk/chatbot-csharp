using System;

namespace chatbot
{
    /// <summary>
    /// The main class of the chatbot application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The entry point of the chatbot application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Configure or load settings as needed
                SettingsManager settingsManager = new SettingsManager("./settings.yaml"); // Assuming YAML for settings

                // Create the terminal interface with settingsManager
                TerminalInterface terminalInterface = new TerminalInterface(settingsManager);

                // Create the ChatManager with the terminal interface as the message consumer
                ChatManager chatManager = new ChatManager(settingsManager, terminalInterface);

                // Set the ChatManager in the TerminalInterface
                terminalInterface.SetChatManager(chatManager);

                // Initialize the ChatManager and TerminalInterface
                terminalInterface.InitializeChatManager();

                // Start the interactive session
                terminalInterface.Run();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("An error occurred during the execution of the application: " + e.Message);
            }
        }
    }
}
