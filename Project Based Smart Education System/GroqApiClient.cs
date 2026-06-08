namespace SmartEducationSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

public class GroqApiClient
{
    // Removed hardcoded key to comply with GitHub Security Scanning rules
    private static readonly string ApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "YOUR_API_KEY_HERE";
    private const string BaseUrl = "https://api.groq.com/openai/v1/";
    private readonly HttpClient httpClient;

    public GroqApiClient()
    {
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        try
        {
            var response = await httpClient.GetAsync($"{BaseUrl}models");
            response.EnsureSuccessStatusCode();
            
            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            
            var models = new List<string>();
            foreach (var element in doc.RootElement.GetProperty("data").EnumerateArray())
            {
                string id = element.GetProperty("id").GetString() ?? "";
                if (!string.IsNullOrEmpty(id) && !id.Contains("whisper")) // filter out audio models
                {
                    models.Add(id);
                }
            }
            return models.OrderBy(m => m).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching models: {ex.Message}");
            // Fallback default models if API call fails
            return new List<string> { "llama3-8b-8192", "llama3-70b-8192", "mixtral-8x7b-32768", "gemma-7b-it" };
        }
    }

    public async Task<string> SendMessageAsync(string model, List<ChatMessage> messages)
    {
        try
        {
            var requestBody = new
            {
                model = model,
                messages = messages,
                temperature = 0.7
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{BaseUrl}chat/completions", content);
            
            response.EnsureSuccessStatusCode();
            
            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            
            var answer = doc.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();
                            
            return answer ?? "Error: Empty response from AI.";
        }
        catch (Exception ex)
        {
            return $"Error connecting to Groq API: {ex.Message}";
        }
    }

    public async Task StreamMessageAsync(string model, List<ChatMessage> messages, Action<string> onChunkReceived)
    {
        try
        {
            var requestBody = new
            {
                model = model,
                messages = messages,
                temperature = 0.7,
                stream = true
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}chat/completions") { Content = content };
            
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6).Trim();
                    if (data == "[DONE]") break;

                    try 
                    {
                        using var doc = JsonDocument.Parse(data);
                        var choices = doc.RootElement.GetProperty("choices");
                        if (choices.GetArrayLength() > 0)
                        {
                            var delta = choices[0].GetProperty("delta");
                            if (delta.TryGetProperty("content", out var contentProp))
                            {
                                string? chunk = contentProp.GetString();
                                if (!string.IsNullOrEmpty(chunk))
                                {
                                    onChunkReceived(chunk);
                                }
                            }
                        }
                    }
                    catch { /* ignore partial json errors */ }
                }
            }
        }
        catch (Exception ex)
        {
            onChunkReceived($"\n[Error: {ex.Message}]");
        }
    }
}
