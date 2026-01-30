using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using NutritionApp.Models;

namespace NutritionApp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://bjuapiserver20260127151810.azurewebsites.net");
    }

    public async Task<AuthResponse> LoginUserAsync(object payload)
    {
        try
        {
            Debug.WriteLine($"Login payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", payload);
            Debug.WriteLine($"Login response status: {response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Login response content: {content}");
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                Debug.WriteLine($"Deserialized UserId: {authResponse?.UserId}");
                return authResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Login error: {errorContent}");
                return new AuthResponse { Message = errorContent, UserId = 0 };
            }
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
            var response = await _httpClient.GetAsync($"/api/Profile/{userId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserProfile>();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserProfile> UpdateUserProfileAsync(int userId, object payload)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Profile/{userId}", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserProfile>();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserProfile> RegisterUserAsync(object payload)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserProfile>();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<MealPlan> GenerateMealPlanAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/MealPlan/generate/{userId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<MealPlan>();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<MealPlan>> GetMealPlanHistoryAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/MealPlan/history/{userId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<MealPlan>>();
            return new List<MealPlan>();
        }
        catch
        {
            return new List<MealPlan>();
        }
    }

    // Додані методи для тренувань
    public async Task<WorkoutPlan> GenerateWorkoutAsync(int userId, string goal, string intensity, int duration)
    {
        try
        {
            var payload = new { goal, intensity, duration };
            var response = await _httpClient.PostAsJsonAsync($"/api/Workout/generate/{userId}", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<WorkoutPlan>();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<WorkoutPlan>> GetUserWorkoutsAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/Workout/user/{userId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<WorkoutPlan>>();
            return new List<WorkoutPlan>();
        }
        catch
        {
            return new List<WorkoutPlan>();
        }
    }
}