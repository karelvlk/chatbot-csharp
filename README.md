# LLM Chatbot

## Overview

The LLM Chatbot is a C#-based application that interacts with users via a terminal interface. It communicates with a Python backend hosting the Large Language Model (LLM) of choice, providing chat management functionality and the ability to save conversations. The Python backend operates locally on a CPU, which may result in slower performance and response times due to the lack of GPU/TPU acceleration.

**Note:** The Docker image for the LLM is approximately 4GB in size. Ensure that your system has enough RAM and storage to handle this efficiently, especially when running multiple instances.

**Note:** Shell scripts provided in this project may not work as expected on Windows due to differences in the Unix and Windows command-line environments. To fix this, you can try to use it via **WSL**.

## Quickstart

### System Requirements

Before initiating, ensure your system meets the following prerequisites:

- Docker installed (^4.28.0)
  - Supported versions:
    - Docker Desktop (^4.28.0)
    - Docker Engine (^26.0.0)
- A compatible operating system and CPU architecture:
  - Linux, Windows with x86_64 (amd64) CPU
  - macOS with Apple Silicon (arm64) CPU
- At least 5GB of available disk space
- At least 4GB of RAM

### Running the Application

Depending on your CPU architecture, use the appropriate shell command to start the application.

- **For Apple Silicon ARM CPU:**

```sh
sh run.arm.sh
```

- **For amd64 CPU:**

```sh
sh run.amd64.sh
```

The difference lies in the Docker image used for the LLM, which needs to be compiled separately for ARM and amd64 architectures to ensure optimal performance.

### Troubleshooting Startup Scripts

If you encounter any issues with the startup scripts, follow these steps to manually start the application:

1. **Start the LLM Docker Container:**

Replace `<LLM>` with `<llmb-amd64>` for amd64 CPUs or `<llmb>` for Apple Silicon ARM CPUs:

     ```sh
     docker run --rm -d -p 9000:9000 --name server vlkkarel/<LLM>
     ```

2. **Build the C# Client App Docker Container:**

   ```sh
   docker build --no-cache -t chatbot .
   ```

3. **Run the Built C# Client Docker Container:**

   ```sh
   docker run --rm -it --link server chatbot
   ```

## Functionalities

The User Interface allows users to manage and interact with the chatbot effectively. Here are the key functionalities that offers:

1. **Dynamic Input Handling**:

- Two input handling states: `Command State` and `Chat State`, where each type of user input is handled differently (refer to the section below, titled #Usage).

2. **Command Support**: Users can execute several commands to control the chatbot:

   - `/new`: Starts a new chat session.
   - `/help`: Displays help information about the application and available commands.
   - `/home`: Returns to the main menu.
   - `/history`: Shows the chat history.
   - `/memory`: Provides options to configure memory management settings.
   - `/model`: Allows selection between different LLM models.
   - `/exit`: Exits the application.

3. **Chat Management**: Manages the flow of the chat sessions and processes user input with the following features:

   - _Chat History Addition_:
     - Adds messages to the history as they are processed.
   - _Session Management_:
     - Start and end chat sessions.
     - Loading historical chats that allows users to view and load previous chat sessions.

4. **Memory and Model Configuration**:
   - _Memory Options_:
     - **Buffer memory**: Utilizes a fixed-size buffer to maintain recent conversation context, offering fast performance suitable for most real-time interactions.
     - **Summary memory**: Summarizes the conversation's context by LLM to preserve critical information over longer interactions, but can increase response times due to LLM summary request.
   - _Model Options_:
     - **Phi-2**: A balanced language model known for efficient processing and high accuracy across diverse conversational scenarios.
     - **TinyLlama**: A smaller, more lightweight model optimized for faster responses and lower resource usage.

## Usage

When the application starts, a simple terminal-based UI appears. This interface allows the user to navigate through the entire application and interact with features by typing commands and text into the AI chatbot. There are two UI input states, distinguished by the prompt messages: `Your action: ` and `You: `. The first is referred to as the command state, and the second as the chat state.

- **Command State**: Here, the application awaits a command or a number corresponding to the options displayed. Each number corresponds to an item in an ordered list of options. Commands are those that can be viewed by typing the `/help` command.
- **Chat State**: In this state, you interact directly with the bot by typing messages. If the input doesn't exactly match any available commands, the message is sent to the AI chatbot; otherwise, the command is executed.

This structure allows for intuitive navigation and interaction within the application, providing a seamless user experience.

## Implementation Details

### App architecture diagram

<p align="center">
  <img src="./images/diagram.png" alt="Alt text" title="App diagram">
</p>

### System Components

- **ChatManager**: The central hub coordinating communication between the user, LLM client, and other system components. It handles message exchange, updates chat history, and manages settings.
- **ChatHistoryManager**: Manages storage and retrieval of chat history, preserving past interactions for context and review.
- **LLMClient**: Interfaces with the external LLM service, sending user-generated prompts and receiving responses.
- **SettingsManager**: Manages configuration settings, allowing customization and configuration of various parameters.
- **TerminalInterface**: Provides a command-line interface (CLI) for user interaction, enabling users to input messages and view responses.
- **MemoryManager**: Manages short-term and long-term memory for maintaining context during conversations. Includes subclasses like BufferMemory and SummaryMemory.
- **PromptGenerator**: Generates prompts for the LLM based on user input and context from MemoryManager.

### Key fundaments and limitations

#### Event-Based Interaction through CLI

The system uses an event-based interaction model through the CLI. The `TerminalInterface` component captures user inputs as events and forwards them to the `ChatManager`. This event-driven approach allows the system to respond to user actions in real-time, providing a dynamic and interactive user experience.

#### Callbacks and Asynchronous Operations

Callbacks are used extensively in the system to handle asynchronous operations. For instance, when the `ChatManager` sends a prompt to the `LLMClient`, it registers a callback to process the response once it arrives. This non-blocking design ensures that the system remains responsive while waiting for potentially slow operations, such as querying the LLM.

#### Potential Slow Response Times

Since the models (e.g., PHI2 and TinyLlama) run locally, the system may experience slow response times. Local execution of these large models can be resource-intensive and time-consuming, affecting the overall responsiveness of the system. Users should be aware of potential delays when interacting with the LLM.

#### Summary Memory Limitations

The `SummaryMemory` component is intended to provide long-term context by summarizing past interactions. However, due to the limited capabilities of the models used, this feature does not perform as well as expected. The summarization may not capture the full context accurately, impacting the quality of generated prompts and responses.

#### Context Limitations

Another limitation is that the context for the LLM is restricted to 300 tokens due to running locally on a CPU. Handling more tokens would require significantly more RAM and greatly increase response times. This constraint affects the ability to maintain extensive context in conversations.

#### Models Used: PHI2 and TinyLlama

The system utilizes the PHI2 and TinyLlama models for generating responses. These models are selected for their balance between performance and resource requirements. Despite their capabilities, users may notice limitations in the quality of summarization and contextual understanding, as mentioned earlier.

#### YAML File Format for History Saving

The chat history is saved using the YAML file format. YAML (YAML Ain't Markup Language) is chosen for its human-readable format, which makes it easy for developers to inspect and debug saved histories. Additionally, YAML supports hierarchical data structures, making it suitable for storing complex chat interactions. The benefits of using YAML include:

- **Readability**: The clear and straightforward syntax of YAML makes it easy to read and understand, even for those who are not familiar with the format.
- **Flexibility**: YAML supports complex data structures, allowing for the storage of detailed and nested chat histories.
- **Interoperability**: Many programming languages have libraries for parsing and writing YAML, making it a versatile choice for cross-language compatibility.

### Design Approach

The system is designed with a clear separation of concerns, leveraging object-oriented principles to ensure each component has a well-defined responsibility. The interaction between components is managed through well-defined interfaces and methods, promoting modularity and ease of maintenance. This architecture not only makes the system robust but also allows for easy extension and integration of additional features or components in the future.

## Documentation

The programming documentation, generated from XML comments within the code, is located in the `docs/` directory.
