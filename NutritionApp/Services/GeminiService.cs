using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NutritionApp.Models;

namespace NutritionApp.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "AIzaSyDpdCxusbEBRoVCT4TY1TCzwZynpTGRxuI";
    private readonly string _endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public GeminiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<FoodDatabaseItem?> AnalyzeFoodImageAsync(byte[] imageBytes)
    {
        try
        {
            var base64Image = Convert.ToBase64String(imageBytes);
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = "Analyze this food image. Identify what it is, and provide a reasonable estimation of its nutritional values per 100g. Return ONLY a valid JSON object matching exactly this structure with no markdown formatting: {\"Name\": \"Food name in Ukrainian\", \"CaloriesPer100g\": 250, \"ProteinPer100g\": 10.5, \"FatPer100g\": 8.2, \"CarbsPer100g\": 30.1}" },
                            new
                            {
                                inlineData = new
                                {
                                    mimeType = "image/jpeg",
                                    data = base64Image
                                }
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.4,
                    responseMimeType = "application/json"
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var url = $"{_endpoint}?key={_apiKey}";

            var response = await _httpClient.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

            var resultText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            
            if (!string.IsNullOrEmpty(resultText))
            {
                // clean up potential markdown formatting if model ignores response_mime_type
                resultText = resultText.Replace("```json", "").Replace("```", "").Trim();
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };
                var parsedItem = JsonSerializer.Deserialize<FoodDatabaseItem>(resultText, options);
                
                if (parsedItem != null)
                {
                    parsedItem.Id = new Random().Next(1000, 99999);
                    return parsedItem;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error analyzing food image: {ex.Message}");
            throw; // Rethrow to let the UI display the actual error
        }
    }

    // JSON mapping classes for Gemini API response
    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; } = new();
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public GeminiContent Content { get; set; } = new();
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new();
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }
}
