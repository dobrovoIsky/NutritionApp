using System.Text.Json.Serialization;

namespace NutritionApp.Models
{
    public class UserProfile
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("goal")]
        public string Goal { get; set; }

        [JsonPropertyName("activityLevel")]
        public string ActivityLevel { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("avatarId")]
        public int AvatarId { get; set; }

        [JsonPropertyName("bju")]
        public BjuResult Bju { get; set; }
    }
}