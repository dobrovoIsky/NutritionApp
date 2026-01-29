using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NutritionApp.Models;

namespace NutritionApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            var baseUrl = "https://bjuapiserver20260127151810.azurewebsites.net"; // прод-сервер
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task<AuthResponse> RegisterUserAsync(object payload)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>();

            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResponse { Message = errorContent, UserId = 0 };
        }

        public async Task<AuthResponse> LoginUserAsync(object payload)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>();

            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResponse { Message = errorContent, UserId = 0 };
        }

        public async Task<UserProfile> GetUserProfileAsync(int userId) =>
            await _httpClient.GetFromJsonAsync<UserProfile>($"/api/Profile/{userId}");

        public async Task<UserProfile> UpdateUserProfileAsync(int userId, object payload)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Profile/{userId}", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserProfile>();
            return null;
        }

        // Відповідь бекенду: { "mealPlan": "<текст>" }
        public class MealPlanResponse
        {
            [JsonPropertyName("mealPlan")]
            public string MealPlan { get; set; }
        }

        public async Task<MealPlanResponse> GenerateMealPlanAsync(int userId)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Nutrition/generate-custom-plan", new { userId });
            response.EnsureSuccessStatusCode(); // якщо 4xx/5xx — кине виняток з текстом помилки

            var payload = await response.Content.ReadFromJsonAsync<MealPlanResponse>();
            if (payload == null || string.IsNullOrWhiteSpace(payload.MealPlan))
                throw new Exception("Порожня відповідь від сервера");

            return payload;
        }

        public async Task<List<MealPlan>> GetMealPlanHistoryAsync(int userId) =>
            await _httpClient.GetFromJsonAsync<List<MealPlan>>($"/api/nutrition/history/{userId}");

        public async Task<WorkoutPlan> GenerateWorkoutAsync(object payload)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Workouts/generate", payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WorkoutPlan>();
        }

        public async Task<List<WorkoutPlan>> GetUserWorkoutsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"/api/Workouts/history/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<WorkoutPlan>>() ?? new List<WorkoutPlan>();
        }

        // Виклик AI напряму (prompt рядком)
        public class AIMealPlanResponse { public string Result { get; set; } }
        public async Task<string> GenerateAiMealPlanAsync(string prompt)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ai/mealplan", prompt);
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<AIMealPlanResponse>();
            return payload?.Result ?? string.Empty;
        }
    }
}