using System.Text.Json.Serialization;

namespace NutritionApp.Models
{
    public class BjuResult
    {
        [JsonPropertyName("calories")]
        public double Calories { get; set; }

        [JsonPropertyName("proteins")]
        public double Proteins { get; set; }

        [JsonPropertyName("fats")]
        public double Fats { get; set; }

        [JsonPropertyName("carbs")]
        public double Carbs { get; set; }
    }
}