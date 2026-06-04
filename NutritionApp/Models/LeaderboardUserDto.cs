using System.Text.Json.Serialization;

namespace NutritionApp.Models
{
    public class LeaderboardUserDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("avatarId")]
        public int AvatarId { get; set; }

        [JsonPropertyName("points")]
        public int Points { get; set; }

        [JsonPropertyName("currentStreak")]
        public int CurrentStreak { get; set; }
    }
}
