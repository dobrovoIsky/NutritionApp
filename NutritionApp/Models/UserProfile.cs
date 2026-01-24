namespace NutritionApp.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }
        public string Goal { get; set; }
        public string ActivityLevel { get; set; }
        public BjuResult CalculatedBju { get; set; }
    }
}