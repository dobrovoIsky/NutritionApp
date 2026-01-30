using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using NutritionApp.Models;

namespace NutritionApp.Services.Api;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://bjuapiserver20260127151810.azurewebsites.net");
    }

    public async Task<AuthResponse> RegisterUserAsync(object payload)
    {
        try
        {
            Debug.WriteLine($"Register payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Register error: {errorContent}");
            return new AuthResponse { Message = errorContent, UserId = 0 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Register exception: {ex.Message}");
            return new AuthResponse { Message = $"Error: {ex.Message}", UserId = 0 };
        }
    }

    public async Task<AuthResponse> LoginUserAsync(object payload)
    {
        try
        {
            Debug.WriteLine($"Login payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Login error: {errorContent}");
            return new AuthResponse { Message = errorContent, UserId = 0 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Login exception: {ex.Message}");
            return new AuthResponse { Message = $"Error: {ex.Message}", UserId = 0 };
        }
    }

    public async Task<UserProfile> GetUserProfileAsync(int userId)
    {
        try
        {
            Debug.WriteLine($"Get profile for userId: {userId}");
            return await _httpClient.GetFromJsonAsync<UserProfile>($"/api/Profile/{userId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get profile exception: {ex.Message}");
            return new UserProfile();
        }
    }

    public async Task<UserProfile> UpdateUserProfileAsync(int userId, object payload)
    {
        try
        {
            Debug.WriteLine($"Update profile payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");
            var response = await _httpClient.PutAsJsonAsync($"/api/Profile/{userId}", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserProfile>();
            Debug.WriteLine($"Update profile error: {await response.Content.ReadAsStringAsync()}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update profile exception: {ex.Message}");
            return null;
        }
    }

    // Відповідь бекенду: { "mealPlan": "<текст>" }
    public class MealPlanResponse
    {
        [JsonPropertyName("mealPlan")]
        public string MealPlan { get; set; }
    }

    public async Task<MealPlanResponse> GenerateMealPlanAsync(int userId)
    {
        try
        {
            Debug.WriteLine($"Generate meal plan for userId: {userId}");
            var response = await _httpClient.PostAsJsonAsync("/api/Nutrition/generate-custom-plan", new { userId });
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<MealPlanResponse>();
            if (payload == null || string.IsNullOrWhiteSpace(payload.MealPlan))
                throw new Exception("Порожня відповідь від сервера");
            return payload;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Generate meal plan exception: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Models.MealPlan>> GetMealPlanHistoryAsync(int userId)
    {
        try
        {
            Debug.WriteLine($"Get meal plan history for userId: {userId}");
            return await _httpClient.GetFromJsonAsync<List<Models.MealPlan>>($"/api/nutrition/history/{userId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get meal plan history exception: {ex.Message}");
            return new List<Models.MealPlan>();
        }
    }

    public async Task<WorkoutPlan> GenerateWorkoutAsync(int userId, string goal, string intensity, int duration)
    {
        var payload = new { UserId = userId, Goal = goal, Intensity = intensity, DurationMinutes = duration };
        try
        {
            Debug.WriteLine($"Generate workout payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");
            var response = await _httpClient.PostAsJsonAsync("/api/Workouts/generate", payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WorkoutPlan>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Generate workout exception: {ex.Message}");
            throw;
        }
    }

    public async Task<List<WorkoutPlan>> GetUserWorkoutsAsync(int userId)
    {
        try
        {
            Debug.WriteLine($"Get workouts for userId: {userId}");
            var response = await _httpClient.GetAsync($"/api/Workouts/history/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<WorkoutPlan>>() ?? new List<WorkoutPlan>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get workouts exception: {ex.Message}");
            return new List<WorkoutPlan>();
        }
    }

    // Виклик AI напряму (prompt рядком)
    public class AIMealPlanResponse { public string Result { get; set; } }
    public async Task<string> GenerateAiMealPlanAsync(string prompt)
    {
        try
        {
            Debug.WriteLine($"Generate AI meal plan prompt: {prompt}");
            var response = await _httpClient.PostAsJsonAsync("/api/ai/mealplan", prompt);
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<AIMealPlanResponse>();
            return payload?.Result ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Generate AI meal plan exception: {ex.Message}");
            return string.Empty;
        }
    }
}