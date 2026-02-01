using System.Text.Json.Serialization;

namespace NutritionApp.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }
    }
}