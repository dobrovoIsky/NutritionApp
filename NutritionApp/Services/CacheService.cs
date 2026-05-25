using System.Text.Json;

namespace NutritionApp.Services;

public class CacheService
{
    private static readonly string CacheDirectory = Path.Combine(FileSystem.CacheDirectory, "ApiCache");

    public CacheService()
    {
        if (!Directory.Exists(CacheDirectory))
            Directory.CreateDirectory(CacheDirectory);
    }

    public async Task SaveAsync<T>(string key, T data)
    {
        try
        {
            var fileName = GetFileName(key);
            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(fileName, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache save error: {ex.Message}");
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var fileName = GetFileName(key);
            if (!File.Exists(fileName))
                return default;

            var json = await File.ReadAllTextAsync(fileName);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache load error: {ex.Message}");
            return default;
        }
    }
    
    public bool HasData(string key)
    {
        var fileName = GetFileName(key);
        return File.Exists(fileName);
    }
    
    public void Clear(string key)
    {
        var fileName = GetFileName(key);
        if (File.Exists(fileName))
            File.Delete(fileName);
    }
    
    public async Task SaveStringAsync(string key, string data)
    {
        try
        {
            var fileName = GetFileName(key);
            await File.WriteAllTextAsync(fileName, data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache save error: {ex.Message}");
        }
    }

    public async Task<string?> LoadStringAsync(string key)
    {
        try
        {
            var fileName = GetFileName(key);
            if (!File.Exists(fileName))
                return null;

            return await File.ReadAllTextAsync(fileName);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache load error: {ex.Message}");
            return null;
        }
    }

    private string GetFileName(string key)
    {
        // Simple hash to avoid invalid characters in filenames
        var safeKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key))
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");
        return Path.Combine(CacheDirectory, $"{safeKey}.json");
    }
}
