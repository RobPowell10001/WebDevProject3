using Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI.Chat;
using Settings;
using System.Text;

namespace API
{
    public class LLM
    {
        private readonly OAIConfig _oai;
        public string systemPrompt { get; set; }

        public LLM(IOptions<Settings.OAIConfig> oaiConfig)
        {
            _oai = oaiConfig.Value;
        }
        public async Task<string> CallChatGPT(string prompt)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", _oai.Key);

                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 950
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_oai.Endpoint}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return result.choices[0].message.content;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Request to Azure OpenAI failed: {errorContent}");
                }
            }
        }

        public async Task<string> CallWithSystemPrompt(string prompt)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", _oai.Key);

                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt},
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 950
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_oai.Endpoint}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return result.choices[0].message.content;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Request to Azure OpenAI failed: {errorContent}");
                }
            }
        }

        public async Task<string> CallWithSystemPromptAndExample(string exUser, string exAssistant, string prompt)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", _oai.Key);

                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt},
                        new { role = "user", content = exUser },
                        new { role = "user", content = exAssistant },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 950
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_oai.Endpoint}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return result.choices[0].message.content;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Request to Azure OpenAI failed: {errorContent}");
                }
            }
        }
    }
}
