using System.Text.Json.Serialization;

namespace NutritionApp.Models
{
    public class FoodEntry
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("calories")]
        public double Calories { get; set; }

        [JsonPropertyName("protein")]
        public double Protein { get; set; }

        [JsonPropertyName("fat")]
        public double Fat { get; set; }

        [JsonPropertyName("carbs")]
        public double Carbs { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }

        [JsonPropertyName("mealType")]
        public string MealType { get; set; }

        [JsonPropertyName("loggedAt")]
        public DateTime LoggedAt { get; set; }
    }

    public class DailySummary
    {
        [JsonPropertyName("totalCalories")]
        public double TotalCalories { get; set; }

        [JsonPropertyName("totalProtein")]
        public double TotalProtein { get; set; }

        [JsonPropertyName("totalFat")]
        public double TotalFat { get; set; }

        [JsonPropertyName("totalCarbs")]
        public double TotalCarbs { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
