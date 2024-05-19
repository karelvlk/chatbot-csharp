using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace chatbot
{
    /// <summary>
    /// The <c>LLMClient</c> class represents a client for communicating with the LLM
    /// server. It provides methods for initializing the LLM, sending queries, and
    /// retrieving responses.
    /// </summary>
    public class LLMClient
    {
        private HttpClient client;
        private SettingsManager settings;
        private IMessageConsumer messageConsumer;

        /// <summary>
        /// Constructs a new LLMClient with the specified settings and message consumer.
        /// </summary>
        /// <param name="settings">The settings manager for the client.</param>
        /// <param name="messageConsumer">The message consumer for processing received messages.</param>
        public LLMClient(SettingsManager settings, IMessageConsumer messageConsumer)
        {
            this.client = new HttpClient();
            this.settings = settings;
            this.messageConsumer = messageConsumer;
        }

        /// <summary>
        /// Initializes the LLM by sending an HTTP POST request to the LLM server.
        /// This method constructs a JSON payload with the necessary settings and sends
        /// it as the request body.
        /// The response from the server is printed to the console.
        /// </summary>
        /// <returns>true if the initialization is successful, false otherwise.</returns>
        public bool InitializeLLM()
        {
            Console.WriteLine("[Communicating with LLM Server] Initializing LLM...");
            string json = $"{{\"model\": \"{settings.GetSetting("model")}\", \"max_total_tokens\": {settings.GetSetting("maxTotalTokens")}, \"stop_at\": \"{settings.GetSetting("stopAt")}\", \"quantization\": \"{settings.GetSetting("quantization")}\"}}";
            Console.WriteLine("[Communicating with LLM Server] Http POST body: " + json);

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(settings.GetSetting("baseUrl") + "/initialize/"),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            try
            {
                HttpResponseMessage response = client.SendAsync(request).Result;
                Console.WriteLine("[Communicating with LLM Server] Response status code: " + response.StatusCode);

                // Check if the response status code is 200 (OK)
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("[Communicating with LLM Server] Response status code is not 200. Operation failed.");
                    return false;
                }

                Console.WriteLine("[Communicating with LLM Server] Response body: " + response.Content.ReadAsStringAsync().Result);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Sanitizes the given prompt by escaping special characters.
        /// </summary>
        /// <param name="prompt">The prompt to be sanitized.</param>
        /// <returns>The sanitized prompt.</returns>
        private string SanitizePrompt(string prompt)
        {
            string escapedPrompt = prompt.Replace("\\", "\\\\");
            escapedPrompt = escapedPrompt.Replace("\"", "\\\"");
            escapedPrompt = escapedPrompt.Replace("\n", "\\n");
            escapedPrompt = escapedPrompt.Replace("\t", "\\t");
            return escapedPrompt;
        }

        /// <summary>
        /// Sends a query to the server and processes the response.
        /// </summary>
        /// <param name="query">The query to send to the server.</param>
        public void SendQuery(string query)
        {
            // Construct JSON string
            string json = "{\"prompt\": \"" + SanitizePrompt(query) + "\"}";

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(settings.GetSetting("baseUrl") + "/generate/"),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            client.SendAsync(request)
                .ContinueWith(responseTask =>
                {
                    var response = responseTask.Result;
                    response.EnsureSuccessStatusCode();
                    return response.Content.ReadAsStreamAsync();
                })
                .Unwrap()
                .ContinueWith(streamTask =>
                {
                    using var reader = new StreamReader(streamTask.Result);
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        messageConsumer.AcceptMessage(line);
                    }
                })
                .ContinueWith(_ => messageConsumer.AcceptMessage("$END$"))
                .Wait();
        }

        /// <summary>
        /// Sends a synchronous query to the server and returns the response as a string.
        /// </summary>
        /// <param name="query">The query to send to the server.</param>
        /// <returns>The response from the server as a string.</returns>
        public string SendQuerySync(string query)
        {
            string json = JsonSerializer.Serialize(new
            {
                prompt = SanitizePrompt(query)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(settings.GetSetting("baseUrl") + "/generate/"))
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            try
            {
                var response = client.SendAsync(request).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error sending request: " + e.Message);
                return "";
            }
        }
    }
}
