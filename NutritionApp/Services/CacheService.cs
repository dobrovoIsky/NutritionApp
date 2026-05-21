using System.Text.Json;

namespace NutritionApp.Services;

public class CacheService
{
    private const int CACHE_EXPIRATION_DAYS = 7;

    public async Task SaveAsync<T>(string key, T data)
    {
        try
        {
            if (data == null) return;
            var json = JsonSerializer.Serialize(data);
            Preferences.Set(key, json);
            Preferences.Set($"{key}_timestamp", DateTime.Now);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache Save Error: {ex.Message}");
        }
    }

    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            if (!Preferences.ContainsKey(key)) return default;

            var json = Preferences.Get(key, string.Empty);
            if (string.IsNullOrEmpty(json)) return default;

            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache Get Error: {ex.Message}");
            return default;
        }
    }

    public bool HasData(string key)
    {
        return Preferences.ContainsKey(key);
    }

    public void Clear(string key)
    {
        Preferences.Remove(key);
        Preferences.Remove($"{key}_timestamp");
    }
}
