namespace NutritionApp.Models
{
    public class MealPlan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Plan { get; set; }
        public DateTime Date { get; set; }
    }
}