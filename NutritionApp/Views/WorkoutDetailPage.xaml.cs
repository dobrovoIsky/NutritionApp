using System.Text.Json;
using NutritionApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace NutritionApp.Views;

[QueryProperty(nameof(WorkoutJson), "WorkoutJson")]
[QueryProperty(nameof(Goal), "Goal")]
public partial class WorkoutDetailPage : ContentPage
{
    public string WorkoutJson
    {
        set => LoadWorkout(value);
    }

    public string Goal
    {
        set => GoalLabel.Text = Uri.UnescapeDataString(value ?? "");
    }

    public WorkoutDetailPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void LoadWorkout(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return;

            // Clean JSON if needed
            var cleanJson = json.Trim();
            if (cleanJson.StartsWith("```json")) cleanJson = cleanJson.Substring(7).Trim();
            if (cleanJson.EndsWith("```")) cleanJson = cleanJson.Substring(0, cleanJson.Length - 3).Trim();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var plan = JsonSerializer.Deserialize<ApiService.WorkoutJsonResponse>(cleanJson, options);

            if (plan == null) return;

            ContentContainer.Children.Clear();

            // Summary
            if (!string.IsNullOrEmpty(plan.Summary))
            {
                ContentContainer.Children.Add(new Label 
                { 
                    Text = plan.Summary, 
                    TextColor = Color.Parse("#64748B"), 
                    HorizontalTextAlignment = TextAlignment.Center 
                });
            }

            // Warmup
            if (plan.Warmup != null)
            {
                AddSectionHeader("🔥 Розминка", $"{plan.Warmup.Duration} хв");
                foreach (var ex in plan.Warmup.Exercises)
                {
                    AddExerciseItem(ex);
                }
            }

            // Workout
            if (plan.Workout != null)
            {
                AddSectionHeader("💪 Основна частина", "");
                foreach (var ex in plan.Workout)
                {
                    AddExerciseCard(ex);
                }
            }

            // Cooldown
            if (plan.Cooldown != null)
            {
                AddSectionHeader("🧘 Заминка", $"{plan.Cooldown.Duration} хв");
                foreach (var ex in plan.Cooldown.Exercises)
                {
                    AddExerciseItem(ex);
                }
            }
        }
        catch (Exception ex)
        {
            ContentContainer.Children.Add(new Label { Text = "Не вдалося завантажити деталі: " + ex.Message, TextColor = Colors.Red });
            ContentContainer.Children.Add(new Label { Text = json, TextColor = Colors.Gray, FontSize = 10 });
        }
    }

    private void AddSectionHeader(string title, string subtitle)
    {
        var stack = new HorizontalStackLayout { Spacing = 10, Margin = new Thickness(0, 10, 0, 5) };
        stack.Children.Add(new Label { Text = title, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.Parse("#0F172A") });
        if (!string.IsNullOrEmpty(subtitle))
        {
            stack.Children.Add(new Label { Text = subtitle, FontSize = 14, TextColor = Color.Parse("#64748B"), VerticalOptions = LayoutOptions.Center });
        }
        ContentContainer.Children.Add(stack);
    }

    private void AddExerciseItem(string name)
    {
        var frame = new Border 
        { 
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Stroke = Color.Parse("#E2E8F0"),
            BackgroundColor = Colors.White,
            Padding = 10,
            Margin = new Thickness(0, 0, 0, 8)
        };
        frame.Content = new Label { Text = "• " + name, TextColor = Color.Parse("#334155") };
        ContentContainer.Children.Add(frame);
    }

    private void AddExerciseCard(ApiService.ExerciseItem ex)
    {
        var border = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Stroke = Color.Parse("#E2E8F0"),
            BackgroundColor = Colors.White,
            Padding = 12,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var grid = new Grid { RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto }, new RowDefinition { Height = GridLength.Auto } } };
        
        // Name & Sets/Reps
        var header = new HorizontalStackLayout { Spacing = 8 };
        header.Children.Add(new Label { Text = ex.Name, FontAttributes = FontAttributes.Bold, TextColor = Color.Parse("#0F172A"), FontSize = 16 });
        
        var details = new VerticalStackLayout { Spacing = 4, Margin = new Thickness(0, 6, 0, 0) };
        details.Add(new Label { Text = $"{ex.Sets} підходи x {ex.Reps}", TextColor = Color.Parse("#6366F1"), FontAttributes = FontAttributes.Bold });
        if (!string.IsNullOrEmpty(ex.Rest)) details.Add(new Label { Text = $"Відпочинок: {ex.Rest}", TextColor = Color.Parse("#64748B"), FontSize = 12 });
        if (!string.IsNullOrEmpty(ex.Tips)) details.Add(new Label { Text = $"💡 {ex.Tips}", TextColor = Color.Parse("#94A3B8"), FontSize = 11, FontAttributes = FontAttributes.Italic });

        var contentStack = new VerticalStackLayout();
        contentStack.Add(header);
        contentStack.Add(details);

        border.Content = contentStack;
        ContentContainer.Children.Add(border);
    }
}
