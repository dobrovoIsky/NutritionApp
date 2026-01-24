namespace NutritionApp.Models
{
    public class WorkoutPlan
    {
        public int Id { get; set; }
        public string Goal { get; set; }
        public string Intensity { get; set; }
        public int DurationMinutes { get; set; }
        public string PlanText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
