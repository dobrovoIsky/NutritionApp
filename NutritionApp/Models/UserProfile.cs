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
        public string Gender { get; set; }
        public string Theme { get; set; }
        public string Language { get; set; }
        public int AvatarId { get; set; }
        public BjuResult Bju { get; set; }
    }
}