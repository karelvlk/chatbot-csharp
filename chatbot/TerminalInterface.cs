using System;
using System.Collections.Generic;

namespace chatbot
{
    /// <summary>
    /// Represents the possible states of the user interface input.
    /// </summary>
    public enum UIState
    {
        COMMAND, CHAT
    }

    /// <summary>
    /// The <c>TerminalInterface</c> class represents a terminal-based user interface for
    /// interacting with a chatbot.
    /// It implements the <c>IMessageConsumer</c> interface to receive messages from the chatbot.
    /// The class provides methods for initializing the chat manager, accepting user input, displaying messages,
    /// and showing various options and menus.
    /// </summary>
    public class TerminalInterface : IMessageConsumer
    {
        private ChatManager? chatManager;
        private bool exitApplication = false;
        private bool llmProcessing = false;
        private int maxTokens = 300;

        /// <summary>
        /// Constructs a new TerminalInterface object with the specified settings manager.
        /// </summary>
        /// <param name="settingsManager">The settings manager to use for retrieving configuration settings.</param>
        public TerminalInterface(SettingsManager settingsManager)
        {
            this.maxTokens = int.Parse(settingsManager.GetSetting("maxTotalTokens"));
        }

        /// <summary>
        /// Sets the chat manager for the terminal interface.
        /// </summary>
        /// <param name="chatManager">The chat manager to be set.</param>
        public void SetChatManager(ChatManager chatManager)
        {
            this.chatManager = chatManager;
        }

        /// <summary>
        /// Initializes the chat manager.
        /// </summary>
        /// <exception cref="Exception">If an error occurs during initialization.</exception>
        public void InitializeChatManager()
        {
            getChatManager().Initialize();
        }

        /// <summary>
        /// Accepts a message from the chat and processes it accordingly.
        /// If the message is "$END$", it sets the llmProcessing flag to false, displays a new line,
        /// and notifies the chat manager that the end AI message has been received.
        /// Otherwise, if the message is not empty, it adds the message to the chat history.
        /// Finally, it displays the message.
        /// </summary>
        /// <param name="message">The message received from the chat.</param>
        public void AcceptMessage(string message)
        {
            if (message == "$END$")
            {
                llmProcessing = false;
                DisplayMessage("\n");
                getChatManager().ReceivedEndAIMessage();
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                {
                    getChatManager().AddToHistory(message);
                }
                DisplayMessage(message);
            }
        }

        /// <summary>
        /// Reads user input from the console and returns it as a String.
        /// </summary>
        /// <param name="state">The current UI state of the user input.</param>
        /// <returns>The user input as a String.</returns>
        public string? GetInput(UIState state)
        {
            if (state == UIState.COMMAND)
            {
                Console.Write("Your action: ");
            }
            else
            {
                Console.Write("You: ");
            }
            return Console.ReadLine();
        }

        /// <summary>
        /// Gets the user input and processes it according to the current UI state.
        /// If the input is a command, it checks if it is a valid command and processes it accordingly.
        /// If the input is not a command, it returns the input as a String.
        /// </summary>
        /// <param name="state">The current UI state of the user input.</param>
        /// <returns>The user input as a String.</returns>
        public string? GetUserInput(UIState state)
        {
            string? input = GetInput(state);
            switch (string.IsNullOrWhiteSpace(input) ? null : input.ToLower())
            {
                case "/new":
                    ShowNewChat();
                    break;
                case "/help":
                    ShowHelp();
                    break;
                case "/home":
                    ShowWelcomeMenu();
                    break;
                case "/history":
                    ShowHistory();
                    break;
                case "/memory":
                    ShowMemoryOptions();
                    break;
                case "/model":
                    ShowModelOptions();
                    break;
                case "/exit":
                    getChatManager().EndChatSession();
                    exitApplication = true;
                    break;
                default:
                    return input;
            }
            return null;
        }

        /// <summary>
        /// Displays a message on the terminal interface.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        public void DisplayMessage(string message)
        {
            Console.Write(message);
        }

        /// <summary>
        /// Displays the welcome menu for the chatbot application.
        /// The menu includes options for starting a new chat, viewing chat history, and accessing help.
        /// The user is prompted to select a mode or enter a command.
        /// If an invalid option is entered, the user is prompted to try again.
        /// Once a valid option is selected, the corresponding action is performed.
        /// After the action is completed, the user is prompted to enter another command.
        /// </summary>
        public void ShowWelcomeMenu()
        {
            getChatManager().EndChatSession();
            string welcomeMessage = "Welcome back human!";
            string menu = "1. New Chat (or `/new`)\n2. Chat History (or `/history`)\n3. Help (or `/help`)";

            Console.WriteLine("\n" + welcomeMessage);
            Console.WriteLine("----------------------------");
            Console.WriteLine("During chat sessions, type your messages and the chatbot will respond.");
            Console.WriteLine("Use the commands (available by typing '/help') at any time to navigate.");
            Console.WriteLine("\n" + menu);
            Console.WriteLine("\nSelect the mode (number) or type a command");
            Console.WriteLine("----------------------------\n");

            int command = 0;
            do
            {
                string? input = GetUserInput(UIState.COMMAND);
                if (exitApplication)
                {
                    return;
                }

                if (!int.TryParse(input, out command))
                {
                    Console.WriteLine("Invalid option, please try again.");
                    continue;
                }
                if (command < 1 || command > 3)
                {
                    Console.WriteLine("Invalid option, please try again.");
                }
            } while (command < 1 || command > 3);

            switch (command)
            {
                case 1:
                    ShowNewChat();
                    break;
                case 2:
                    ShowHistory();
                    break;
                case 3:
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again.");
                    break;
            }
            WaitForCommand();
        }

        /// <summary>
        /// Displays the help message for the chatbot application.
        /// The help message provides information about the available commands and their usage.
        /// </summary>
        public void ShowHelp()
        {
            Console.WriteLine("\nApplication Help:\n----------------------------");
            Console.WriteLine("This application allows you to interact with a chatbot. Commands:");
            Console.WriteLine("/new - Start a new chat session.\n/help - Display this help message.");
            Console.WriteLine("/home - Return to the welcome menu.\n/history - View chat history.");
            Console.WriteLine("/memory - Memory management options.\n/model - Model selection options.");
            Console.WriteLine("/exit - Exit the application.");
            Console.WriteLine("\nDuring a chat session, type your messages and the chatbot will respond.");
            Console.WriteLine("----------------------------\n");
            WaitForCommand();
        }

        /// <summary>
        /// Displays the chat history and allows the user to select a history to load.
        /// If no chat history is available, a message is displayed.
        /// Once a history is selected, the method loads the history and continues the chat process.
        /// </summary>
        public void ShowHistory()
        {
            getChatManager().EndChatSession();
            List<string> histories = getChatManager().ListHistories();
            if (histories.Count == 0)
            {
                Console.WriteLine("\nNo chat history available.");
                WaitForCommand();
            }
            else
            {
                Console.WriteLine("\nAvailable Chat Histories:\n----------------------------");
                for (int i = 0; i < histories.Count; i++)
                {
                    Console.WriteLine($"{i + 1}: {histories[i]}");
                }
                Console.WriteLine("\nSelect a history to load (number)\n----------------------------\n");

                int choice = -1;
                do
                {
                    string? input = GetUserInput(UIState.COMMAND);
                    if (exitApplication)
                    {
                        return;
                    }

                    if (!int.TryParse(input, out choice))
                    {
                        Console.WriteLine("Invalid input. Please enter a number.");
                        continue;
                    }
                    choice -= 1;
                    if (choice < 0 || choice >= histories.Count)
                    {
                        Console.WriteLine("Invalid selection. Please try again.");
                        continue;
                    }
                } while (choice < 0 || choice >= histories.Count);

                Console.WriteLine("\nContinuing in " + histories[choice].Replace(".json", "") + " ...\n(model: "
                        + getChatManager().GetModelType() + ", memory: "
                        + getChatManager().GetMemoryType() + ")\n----------------------------\n");

                getChatManager().LoadHistory(histories[choice]);
                ProcessChat();
            }
        }

        /// <summary>
        /// Waits for a command from the user and processes it.
        /// This method runs in a loop until the exitApplication flag is set to true.
        /// It prompts the user for input and handles invalid commands.
        /// </summary>
        public void WaitForCommand()
        {
            while (!exitApplication)
            {
                string? input = GetUserInput(UIState.COMMAND);
                if (input != null)
                {
                    Console.WriteLine("Invalid command. Please try again.");
                }
            }
        }

        /// <summary>
        /// Processes the chat messages from the user.
        /// This method runs in a loop until the exitApplication flag is set to true.
        /// It prompts the user for input and passes the input to the chatManager for processing.
        /// If the input is too long, it displays an error message.
        /// </summary>
        public void ProcessChat()
        {
            while (!exitApplication)
            {
                if (!llmProcessing)
                {
                    string? input = GetUserInput(UIState.CHAT);
                    if (input != null)
                    {
                        int tokenCount = input.Split(' ').Length;
                        if (tokenCount > maxTokens - 30)
                        {
                            Console.WriteLine("Message too long. Please try again.");
                        }
                        else
                        {
                            getChatManager().ProcessInput(input);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows a new chat session.
        /// This method ends the current chat session, starts a new chat session, and processes the chat.
        /// It prints information about the model type and memory type being used for the chat session.
        /// </summary>
        public void ShowNewChat()
        {
            getChatManager().EndChatSession();
            Console.WriteLine("\nStarting new chat session...\n(model: " + getChatManager().GetModelType() + ", memory: "
                    + getChatManager().GetMemoryType() + ")\n----------------------------\n");
            getChatManager().StartChatSession();
            ProcessChat();
        }

        /// <summary>
        /// Displays the memory management options to the user and updates the configuration based on the user's selection.
        /// The user is prompted to choose between a buffer or summary memory management option.
        /// If the user enters an invalid input, they will be prompted again until a valid option is selected.
        /// After the configuration is updated, the selected memory option is displayed to the user.
        /// This method also waits for the user's command after the configuration is updated.
        /// </summary>
        public void ShowMemoryOptions()
        {
            Console.WriteLine("\nMemory Management Options:\n----------------------------\n");
            Console.WriteLine("1. Buffer (recommended)");
            Console.WriteLine("2. Summary (takes 2x more time and do not work well with these small models)");
            Console.WriteLine("\nSelect an option (1 or 2)");
            Console.WriteLine("----------------------------\n");

            int option = -1;
            do
            {
                string? input = GetUserInput(UIState.COMMAND);
                if (exitApplication)
                {
                    return;
                }

                if (!int.TryParse(input, out option))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }
                if (option < 1 || option > 2)
                {
                    Console.WriteLine("Invalid option. Please select 1 or 2.");
                    continue;
                }
            } while (option < 1 || option > 2);

            string choosenMemory = option == 1 ? "Summary" : "Buffer";
            if (option == 1)
            {
                getChatManager().UpdateMemory(MemoryType.BUFFER);
            }
            else
            {
                getChatManager().UpdateMemory(MemoryType.SUMMARY);
            }

            Console.WriteLine(choosenMemory + " memory selected. Configuration updated.\n");
            WaitForCommand();
        }

        /// <summary>
        /// Displays the model selection options to the user and updates the chat manager's model based on the user's choice.
        /// The user is prompted to enter an option (1 or 2) and the corresponding model is selected.
        /// If an invalid input is entered, the user is prompted to enter a valid option.
        /// After selecting a model, the configuration is updated and a message is printed to confirm the selection.
        /// Finally, the method waits for the next command.
        /// </summary>
        public void ShowModelOptions()
        {
            Console.WriteLine("\nModel Selection Options:\n----------------------------\n");
            Console.WriteLine("1. Phi-2 (Recommended)");
            Console.WriteLine("2. TinyLlama");
            Console.WriteLine("\nSelect an option (1 or 2)");
            Console.WriteLine("----------------------------\n");

            int option = -1;
            do
            {
                string? input = GetUserInput(UIState.COMMAND);
                if (exitApplication)
                {
                    return;
                }

                if (!int.TryParse(input, out option))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }
                if (option < 1 || option > 2)
                {
                    Console.WriteLine("Invalid option. Please select 1 or 2.");
                    continue;
                }
            } while (option < 1 || option > 2);

            string choosenModel = option == 1 ? "Phi-2" : "TinyLlama";
            if (option == 1)
            {
                getChatManager().UpdateModel(ModelType.PHI2);
            }
            else
            {
                getChatManager().UpdateModel(ModelType.TINYLLAMA);
            }

            Console.WriteLine("Model " + choosenModel + " selected. Configuration updated.\n");
            WaitForCommand();
        }

        /// <summary>
        /// Runs the terminal interface for the chatbot.
        /// This method displays the welcome menu.
        /// </summary>
        public void Run()
        {
            ShowWelcomeMenu();
        }

        private ChatManager getChatManager()
        {
            if (chatManager == null)
            {
                throw new Exception("ChatManager is not set.");
            }
            return chatManager;
        }
    }
}
