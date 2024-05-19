using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace chatbot
{
    /// <summary>
    /// Represents the type of memory used by the chatbot.
    /// The chatbot can use either a buffer memory or a summary memory.
    /// </summary>
    public enum MemoryType
    {
        /// <summary>
        /// Buffer memory type using chat history buffer to remember up to max tokens.
        /// </summary>
        BUFFER,

        /// <summary>
        /// Summary memory type using only summaries provided by LLM from chat history.
        /// </summary>
        SUMMARY
    }


    /// <summary>
    /// Represents the type of model used in the chatbot.
    /// The available model types are PHI2 and TINYLLAMA.
    /// </summary>
    public enum ModelType
    {
        /// <summary>
        /// Microsoft's PHI2 model.
        /// </summary>
        PHI2,
        /// <summary>
        /// TinyLlama model.
        /// </summary>
        TINYLLAMA
    }

    /// <summary>
    /// The SettingsManager class is responsible for managing the settings of the
    /// chatbot.
    /// It provides methods to load, save, and update the settings.
    /// </summary>
    public class SettingsManager
    {
        private IConfigurationRoot settings;
        private string settingsPath;

        /// <summary>
        /// Constructs a new SettingsManager object with the specified settings path.
        /// </summary>
        /// <param name="settingsPath">The path to the settings file.</param>
        public SettingsManager(string settingsPath)
        {
            this.settingsPath = settingsPath;
            this.settings = LoadSettings();
        }

        /// <summary>
        /// Loads the settings from a file. If the file exists, it loads the settings
        /// from the file.
        /// If the file does not exist, it sets default values for the settings.
        /// If an error occurs while loading the settings, it prints an error message and
        /// sets default values.
        /// </summary>
        private IConfigurationRoot LoadSettings()
        {

            try
            {
                if (File.Exists(settingsPath))
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(settingsPath, optional: true, reloadOnChange: true);

                    return builder.Build();
                }
                else
                {
                    // Set default values if no settings file is found
                    return SetDefaultSettings();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error loading settings: " + e.Message);
                return SetDefaultSettings();
            }
        }

        /// <summary>
        /// Sets the default settings for the chatbot.
        /// These settings include the base URL, model, memory, stopAt, maxTotalTokens,
        /// quantization, skills, historyPath, and system prompts.
        /// </summary>
        private IConfigurationRoot SetDefaultSettings()
        {
            var defaultSettings = new Dictionary<string, string?>
            {
                ["baseUrl"] = "http://server:9000",
                ["model"] = "Phi2",
                ["memory"] = "buffer",
                ["stopAt"] = "User:",
                ["maxTotalTokens"] = "300",
                ["quantization"] = "q4",
                ["skills"] = "SummarizationSkill,QASkill,CodeSkill",
                ["historyPath"] = "/app/history",
                ["systemPromptTinyLlama"] = "You are a kind AI chatbot.",
                ["systemPromptPhi"] = "A chat between a curious user and an artificial intelligence assistant. The assistant gives helpful answers to the user's questions."
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(defaultSettings)
                .Build();
        }

        /// <summary>
        /// Saves the current settings to a file.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                // Ensure the directory exists
                string? directory = Path.GetDirectoryName(settingsPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(settings.AsEnumerable())
                    .Build();

                File.WriteAllText(settingsPath,
                    JsonConvert.SerializeObject(configuration.AsEnumerable().ToDictionary(k => k.Key, k => k.Value), Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error saving settings: " + e.Message);
                // Consider using a logger here
            }
        }

        /// <summary>
        /// Retrieves the value of the specified setting.
        /// </summary>
        /// <param name="key">The key of the setting to retrieve.</param>
        /// <returns>The value of the setting, or null if the setting does not exist.</returns>
        public string GetSetting(string key)
        {

            return settings[key]!;
        }

        /// <summary>
        /// Updates the model setting with the specified model type.
        /// If the model type is PHI2, the "model" property is set to "Phi2".
        /// Otherwise, the "model" property is set to "TinyLlama".
        /// After updating the setting, the changes are saved.
        /// </summary>
        /// <param name="model">The model type to update.</param>
        public void UpdateModel(ModelType model)
        {
            if (model == ModelType.PHI2)
            {
                settings["model"] = "Phi2";
            }
            else
            {
                settings["model"] = "TinyLlama";
            }
            SaveSettings();
        }

        /// <summary>
        /// Updates the memory setting of the chatbot.
        /// If the specified memory type is BUFFER, the memory setting is set to "buffer".
        /// If the specified memory type is not BUFFER, the memory setting is set to "summary".
        /// After updating the memory setting, the updated settings are saved.
        /// </summary>
        /// <param name="memory">The memory type to be set (BUFFER or any other type).</param>
        public void UpdateMemory(MemoryType memory)
        {
            if (memory == MemoryType.BUFFER)
            {
                settings["memory"] = "buffer";
            }
            else
            {
                settings["memory"] = "summary";
            }
            SaveSettings();
        }

        /// <summary>
        /// Updates the value of a setting identified by the given key.
        /// </summary>
        /// <param name="key">The key of the setting to be updated.</param>
        /// <param name="value">The new value for the setting.</param>
        public void UpdateSetting(string key, string value)
        {
            settings[key] = value;
            SaveSettings();
        }
    }
}
