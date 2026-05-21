using System.Text.Json.Serialization;

namespace NutritionApp.Models
{
    public class FoodDatabaseItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("caloriesPer100g")]
        public double CaloriesPer100g { get; set; }
        
        [JsonPropertyName("proteinPer100g")]
        public double ProteinPer100g { get; set; }
        
        [JsonPropertyName("fatPer100g")]
        public double FatPer100g { get; set; }
        
        [JsonPropertyName("carbsPer100g")]
        public double CarbsPer100g { get; set; }
    }
}
