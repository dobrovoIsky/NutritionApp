using NutritionApp.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NutritionApp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string GOOGLE_CLIENT_ID = "51820459176-b6fadepnveqmnrrdncuejb7h2balavk0.apps.googleusercontent.com";
    private const string SERVER_URL = "https://bjuapiserver20260127151810.azurewebsites.net";
    private const string REDIRECT_URI = SERVER_URL + "/api/auth/google-callback";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(SERVER_URL);
        _httpClient.Timeout = TimeSpan.FromSeconds(120);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // ===== AUTH =====

    public async Task<AuthResponse> RegisterUserAsync(object payload)
    {
        try
        {
            Debug.WriteLine($"Register payload: {JsonSerializer.Serialize(payload)}");
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", payload);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Register response: {json}");
                return JsonSerializer.Deserialize<AuthResponse>(json, _jsonOptions);
            }
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
            Debug.WriteLine($"Login payload: {JsonSerializer.Serialize(payload)}");
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", payload);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Login response: {json}");
                return JsonSerializer.Deserialize<AuthResponse>(json, _jsonOptions);
            }
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

    // ===== GOOGLE AUTH =====

    public async Task OpenGoogleAuthAsync()
    {
        var state = Guid.NewGuid().ToString("N");
        Preferences.Set("google_auth_state", state);

        var authUrl =
            $"https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={GOOGLE_CLIENT_ID}" +
            $"&redirect_uri={Uri.EscapeDataString(REDIRECT_URI)}" +
            $"&response_type=code" +
            $"&scope=openid%20email%20profile" +
            $"&state={state}" +
            $"&access_type=offline" +
            $"&prompt=select_account";

        Debug.WriteLine($"Opening Google auth URL: {authUrl}");
        await Browser.Default.OpenAsync(authUrl, BrowserLaunchMode.SystemPreferred);
    }

    // ===== FORGOT PASSWORD =====

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/forgot-password", new { Email = email });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Forgot password exception: {ex.Message}");
            return false;
        }
    }

    // ===== PROFILE =====

    public async Task<UserProfile> GetUserProfileAsync(int userId)
    {
        try
        {
            Debug.WriteLine($"Get profile for userId: {userId}");
            var json = await _httpClient.GetStringAsync($"/api/Profile/{userId}");
            Debug.WriteLine($"Profile response: {json}");
            var profile = JsonSerializer.Deserialize<UserProfile>(json, _jsonOptions);
            return profile ?? new UserProfile();
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
            Debug.WriteLine($"Update profile for userId: {userId}");
            var response = await _httpClient.PutAsJsonAsync($"/api/Profile/{userId}", payload);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<UserProfile>(responseContent, _jsonOptions);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update profile exception: {ex.Message}");
            return null;
        }
    }

    // ===== MEAL PLAN =====

    public class MealPlanJsonResponse
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("meals")]
        public List<MealItem> Meals { get; set; }
    }

    public class MealItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("foods")]
        public List<FoodItem> Foods { get; set; }

        [JsonPropertyName("totalCalories")]
        public int TotalCalories { get; set; }
    }

    public class FoodItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; }

        [JsonPropertyName("calories")]
        public int Calories { get; set; }

        [JsonPropertyName("protein")]
        public double Protein { get; set; }

        [JsonPropertyName("fat")]
        public double Fat { get; set; }

        [JsonPropertyName("carbs")]
        public double Carbs { get; set; }
    }

    public async Task<MealPlanJsonResponse> GenerateMealPlanAsync(int userId)
    {
        try
        {
            Debug.WriteLine($"Generate meal plan for userId: {userId}");
            var response = await _httpClient.PostAsJsonAsync("/api/Nutrition/generate-custom-plan", new { userId });
            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Meal plan raw response: {json}");

            response.EnsureSuccessStatusCode();
            json = CleanJsonResponse(json);

            var payload = JsonSerializer.Deserialize<MealPlanJsonResponse>(json, _jsonOptions);

            if (payload == null || payload.Meals == null || payload.Meals.Count == 0)
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
            var json = await _httpClient.GetStringAsync($"/api/nutrition/history/{userId}");
            return JsonSerializer.Deserialize<List<Models.MealPlan>>(json, _jsonOptions) ?? new List<Models.MealPlan>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get meal plan history exception: {ex.Message}");
            return new List<Models.MealPlan>();
        }
    }

    // ===== WORKOUT =====

    public class WorkoutJsonResponse
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("warmup")]
        public WarmupCooldown Warmup { get; set; }

        [JsonPropertyName("workout")]
        public List<ExerciseItem> Workout { get; set; }

        [JsonPropertyName("cooldown")]
        public WarmupCooldown Cooldown { get; set; }

        [JsonPropertyName("totalCalories")]
        public int TotalCalories { get; set; }
    }

    public class WarmupCooldown
    {
        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("exercises")]
        public List<string> Exercises { get; set; }
    }

    public class ExerciseItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("sets")]
        public int Sets { get; set; }

        [JsonPropertyName("reps")]
        public string Reps { get; set; }

        [JsonPropertyName("rest")]
        public string Rest { get; set; }

        [JsonPropertyName("tips")]
        public string Tips { get; set; }
    }

    public async Task<WorkoutPlan> GenerateWorkoutAsync(int userId, string goal, string intensity, int duration)
    {
        var payload = new { UserId = userId, Goal = goal, Intensity = intensity, DurationMinutes = duration };
        try
        {
            Debug.WriteLine($"Generate workout payload: {JsonSerializer.Serialize(payload)}");
            var response = await _httpClient.PostAsJsonAsync("/api/Workouts/generate", payload);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Workout response: {json}");
            return JsonSerializer.Deserialize<WorkoutPlan>(json, _jsonOptions);
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
            var response = await _httpClient.GetAsync($"/api/Workouts/history/{userId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<WorkoutPlan>>(json, _jsonOptions) ?? new List<WorkoutPlan>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get workouts exception: {ex.Message}");
            return new List<WorkoutPlan>();
        }
    }

    // ===== HELPERS =====

    private string CleanJsonResponse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;

        json = json.Trim();
        if (json.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            json = json.Substring(7);
        else if (json.StartsWith("```"))
            json = json.Substring(3);

        if (json.EndsWith("```"))
            json = json.Substring(0, json.Length - 3);

        return json.Trim();
    }
}