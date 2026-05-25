using NutritionApp.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;

namespace NutritionApp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly CacheService _cacheService;

    // In-memory cache with TTL
    private static readonly ConcurrentDictionary<string, (object Data, DateTime Expiry)> _memoryCache = new();
    private static readonly TimeSpan ProfileCacheTTL = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan HistoryCacheTTL = TimeSpan.FromMinutes(2);

    private const string GOOGLE_CLIENT_ID = "51820459176-b6fadepnveqmnrrdncuejb7h2balavk0.apps.googleusercontent.com";
    private const string SERVER_URL = "https://bjuapiserver1.onrender.com";
    private const string REDIRECT_URI = SERVER_URL + "/api/auth/google-callback";

    public ApiService(CacheService cacheService)
    {
        _cacheService = cacheService;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(SERVER_URL);
        _httpClient.Timeout = TimeSpan.FromSeconds(60); // Increased from 15s to 60s for AI generation and Render cold starts

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
    }

    // Memory cache helpers
    private T GetFromMemoryCache<T>(string key) where T : class
    {
        if (_memoryCache.TryGetValue(key, out var cached) && cached.Expiry > DateTime.UtcNow)
        {
            Debug.WriteLine($"Memory cache HIT: {key}");
            return cached.Data as T;
        }
        return null;
    }

    private void SetMemoryCache<T>(string key, T data, TimeSpan ttl) where T : class
    {
        if (data != null)
        {
            _memoryCache[key] = (data, DateTime.UtcNow.Add(ttl));
            Debug.WriteLine($"Memory cache SET: {key}");
        }
    }

    public static void ClearMemoryCache()
    {
        _memoryCache.Clear();
        Debug.WriteLine("Memory cache CLEARED");
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
        string cacheKey = $"profile_{userId}";
        
        // Check memory cache first
        var cached = GetFromMemoryCache<UserProfile>(cacheKey);
        if (cached != null) return cached;
        
        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                throw new Exception("Offline");

            Debug.WriteLine($"Get profile for userId: {userId}");
            var json = await _httpClient.GetStringAsync($"/api/Profile/{userId}");
            Debug.WriteLine($"Profile response: {json}");
            
            var profile = JsonSerializer.Deserialize<UserProfile>(json, _jsonOptions);
            if (profile != null)
            {
                SetMemoryCache(cacheKey, profile, ProfileCacheTTL);
                await _cacheService.SaveAsync(cacheKey, profile);
                return profile;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get profile exception (using cache): {ex.Message}");
        }

        var cachedProfile = await _cacheService.GetAsync<UserProfile>(cacheKey);
        return cachedProfile ?? new UserProfile();
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
                var profile = JsonSerializer.Deserialize<UserProfile>(responseContent, _jsonOptions);
                // Оновлюємо кеш
                if (profile != null)
                {
                    await _cacheService.SaveAsync($"profile_{userId}", profile);
                }
                return profile;
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
        public double TotalCalories { get; set; }
    }

    public class FoodItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; }

        [JsonPropertyName("calories")]
        public double Calories { get; set; }

        [JsonPropertyName("protein")]
        public double Protein { get; set; }

        [JsonPropertyName("fat")]
        public double Fat { get; set; }

        [JsonPropertyName("carbs")]
        public double Carbs { get; set; }
    }

    public async Task<MealPlanJsonResponse> GenerateMealPlanAsync(int userId, List<string> availableProducts = null)
    {
        // Генерація завжди має йти через інтернет
        try
        {
            Debug.WriteLine($"Generate meal plan for userId: {userId}, products count: {availableProducts?.Count ?? 0}");
            
            var requestBody = new { 
                UserId = userId, 
                AvailableProducts = availableProducts 
            };
            
            var response = await _httpClient.PostAsJsonAsync("/api/Nutrition/generate-custom-plan", requestBody);
            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Meal plan raw response: {json}");

            response.EnsureSuccessStatusCode();
            json = CleanJsonResponse(json);

            var payload = JsonSerializer.Deserialize<MealPlanJsonResponse>(json, _jsonOptions);

            if (payload == null || payload.Meals == null || payload.Meals.Count == 0)
                throw new Exception("Порожня відповідь від сервера");
            
            // Можна зберегти як "останній згенерований план"
            await _cacheService.SaveAsync($"last_mealplan_{userId}", payload);

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
        string cacheKey = $"meal_history_{userId}";
        
        // Check memory cache first
        var cached = GetFromMemoryCache<List<Models.MealPlan>>(cacheKey);
        if (cached != null) return cached;
        
        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                throw new Exception("Offline");

            var json = await _httpClient.GetStringAsync($"/api/nutrition/history/{userId}");
            var history = JsonSerializer.Deserialize<List<Models.MealPlan>>(json, _jsonOptions);
            
            if (history != null)
            {
                SetMemoryCache(cacheKey, history, HistoryCacheTTL);
                await _cacheService.SaveAsync(cacheKey, history);
                return history;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get meal plan history exception (using cache): {ex.Message}");
        }

        var cachedHistory = await _cacheService.GetAsync<List<Models.MealPlan>>(cacheKey);
        return cachedHistory ?? new List<Models.MealPlan>();
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

    public async Task<WorkoutPlan> GenerateWorkoutAsync(int userId, string goal, string intensity, int duration, List<string>? availableEquipment = null)
    {
        var payload = new { 
            UserId = userId, 
            Goal = goal, 
            Intensity = intensity, 
            DurationMinutes = duration,
            AvailableEquipment = availableEquipment 
        };
        try
        {
            Debug.WriteLine($"Generate workout payload: {JsonSerializer.Serialize(payload)}");
            var response = await _httpClient.PostAsJsonAsync("/api/Workouts/generate", payload);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Workout response: {json}");
            var workout = JsonSerializer.Deserialize<WorkoutPlan>(json, _jsonOptions);

            // Оновлюємо локальний кеш історії, щоб користувач одразу бачив тренування
            try
            {
                var historyKey = $"workouts_v2_{userId}";
                var history = await _cacheService.GetAsync<List<WorkoutPlan>>(historyKey) ?? new List<WorkoutPlan>();
                
                // Додаємо нове тренування на початок списку
                // Перевіряємо на дублікат (за Id, якщо є, або просто додаємо)
                if (!history.Any(w => w.Id == workout.Id && w.Id != 0))
                {
                    history.Insert(0, workout);
                    await _cacheService.SaveAsync(historyKey, history);
                    Debug.WriteLine("Workout added to local history cache");
                }
            }
            catch (Exception ex)
            {
                 Debug.WriteLine($"Error updating workout cache: {ex.Message}");
            }

            return workout;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Generate workout exception: {ex.Message}");
            throw;
        }
    }

    public async Task<List<WorkoutPlan>> GetUserWorkoutsAsync(int userId)
    {
        string cacheKey = $"workouts_v2_{userId}";
        
        // Check memory cache first
        var cached = GetFromMemoryCache<List<WorkoutPlan>>(cacheKey);
        if (cached != null) return cached;
        
        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                throw new Exception("Offline");

            var response = await _httpClient.GetAsync($"/api/Workouts/history/{userId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var serverWorkouts = JsonSerializer.Deserialize<List<WorkoutPlan>>(json, _jsonOptions);
            
            if (serverWorkouts != null)
            {
                var localCache = await _cacheService.GetAsync<List<WorkoutPlan>>(cacheKey) ?? new List<WorkoutPlan>();
                
                foreach(var local in localCache)
                {
                    if (local.Id > 0 && !serverWorkouts.Any(s => s.Id == local.Id))
                    {
                        serverWorkouts.Add(local);
                    }
                }

                serverWorkouts = serverWorkouts.OrderByDescending(w => w.CreatedAt).ToList();

                SetMemoryCache(cacheKey, serverWorkouts, HistoryCacheTTL);
                await _cacheService.SaveAsync(cacheKey, serverWorkouts);
                return serverWorkouts;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get workouts exception (using cache): {ex.Message}");
        }

        var cachedWorkouts = await _cacheService.GetAsync<List<WorkoutPlan>>(cacheKey);
        return cachedWorkouts ?? new List<WorkoutPlan>();
    }

    // ===== TRACKER =====

    public async Task<FoodEntry> LogFoodAsync(FoodEntry entry)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/tracker/log", entry);
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"LogFood response: {response.StatusCode}, Content: {content}");
            
            if (response.IsSuccessStatusCode)
            {
                var savedEntry = JsonSerializer.Deserialize<FoodEntry>(content, _jsonOptions);
                ClearMemoryCache();
                return savedEntry;
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LogFood exception: {ex.Message}");
            return null;
        }
    }

    public async Task<FoodEntry> UpdateFoodEntryAsync(int id, FoodEntry entry)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/tracker/log/{id}", entry);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var updatedEntry = JsonSerializer.Deserialize<FoodEntry>(content, _jsonOptions);
                ClearMemoryCache();
                return updatedEntry;
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UpdateFoodEntry exception: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteFoodEntryAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/tracker/{id}");
            if (response.IsSuccessStatusCode)
            {
                ClearMemoryCache();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeleteFoodEntry exception: {ex.Message}");
            return false;
        }
    }

    public async Task<List<FoodEntry>> GetDailyFoodEntriesAsync(int userId, DateTime? date = null)
    {
        var dateStr = date?.ToString("yyyy-MM-dd");
        string cacheKey = $"food_entries_{userId}_{dateStr}";

        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                throw new Exception("Offline");

            var url = $"/api/tracker/daily/{userId}" + (dateStr != null ? $"?date={dateStr}" : "");
            var json = await _httpClient.GetStringAsync(url);
            var result = JsonSerializer.Deserialize<List<FoodEntry>>(json, _jsonOptions) ?? new List<FoodEntry>();
            
            await _cacheService.SaveAsync(cacheKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetDailyFoodEntriesAsync exception: {ex.Message}");
            var cached = await _cacheService.GetAsync<List<FoodEntry>>(cacheKey);
            return cached ?? new List<FoodEntry>();
        }
    }

    public async Task<DailySummary> GetDailySummaryAsync(int userId, DateTime? date = null)
    {
        var dateStr = date?.ToString("yyyy-MM-dd");
        string cacheKey = $"daily_summary_{userId}_{dateStr}";

        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                throw new Exception("Offline");

            var url = $"/api/tracker/summary/{userId}" + (dateStr != null ? $"?date={dateStr}" : "");
            var json = await _httpClient.GetStringAsync(url);
            var result = JsonSerializer.Deserialize<DailySummary>(json, _jsonOptions) ?? new DailySummary();

            await _cacheService.SaveAsync(cacheKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetDailySummaryAsync exception: {ex.Message}");
            var cached = await _cacheService.GetAsync<DailySummary>(cacheKey);
            return cached ?? new DailySummary();
        }
    }

    public async Task<bool> ToggleMealPlanFavoriteAsync(int planId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"/api/nutrition/history/{planId}/favorite", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ToggleMealPlanFavoriteAsync exception: {ex.Message}");
            return false;
        }
    }

    public async Task<List<FoodDatabaseItem>> GetFoodDatabaseItemsAsync()
    {
        string cacheKey = "food_database_items";
        try
        {
            var cached = GetFromMemoryCache<List<FoodDatabaseItem>>(cacheKey);
            if (cached != null) return cached;

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                throw new Exception("Offline");

            var json = await _httpClient.GetStringAsync("/api/tracker/products");
            var items = JsonSerializer.Deserialize<List<FoodDatabaseItem>>(json, _jsonOptions);
            
            if (items != null)
            {
                SetMemoryCache(cacheKey, items, TimeSpan.FromHours(24));
                await _cacheService.SaveAsync(cacheKey, items);
                return items;
            }
            return new List<FoodDatabaseItem>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetFoodDatabaseItemsAsync exception: {ex.Message}");
            var persistentCached = await _cacheService.GetAsync<List<FoodDatabaseItem>>(cacheKey);
            return persistentCached ?? new List<FoodDatabaseItem>();
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