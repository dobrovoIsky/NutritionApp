using Microsoft.Maui.Controls.Shapes;
using NutritionApp.Services;
using System.Text.Json;

namespace NutritionApp.Views;

public partial class WorkoutPage : ContentPage
{
    private readonly ApiService _apiService;
    private string _selectedType = "";
    private int _selectedDuration = 45;

    public WorkoutPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private void SelectCard(Border selectedCard)
    {
        CardioCard.Stroke = (Color)Application.Current.Resources["BorderColor"];
        CardioCard.BackgroundColor = (Color)Application.Current.Resources["CardBackground"];
        GymCard.Stroke = (Color)Application.Current.Resources["BorderColor"];
        GymCard.BackgroundColor = (Color)Application.Current.Resources["CardBackground"];
        HomeCard.Stroke = (Color)Application.Current.Resources["BorderColor"];
        HomeCard.BackgroundColor = (Color)Application.Current.Resources["CardBackground"];

        selectedCard.Stroke = (Color)Application.Current.Resources["Primary"];
        selectedCard.BackgroundColor = Color.FromArgb("#EFF6FF");

        InitialMessage.IsVisible = false;
    }

    private void OnCardioTapped(object sender, TappedEventArgs e)
    {
        SelectCard(CardioCard);
        _selectedType = "cardio";
    }

    private void OnGymTapped(object sender, TappedEventArgs e)
    {
        SelectCard(GymCard);
        _selectedType = "gym";
    }

    private void OnHomeTapped(object sender, TappedEventArgs e)
    {
        SelectCard(HomeCard);
        _selectedType = "home";
    }

    private void OnDurationChanged(object sender, ValueChangedEventArgs e)
    {
        _selectedDuration = (int)Math.Round(e.NewValue / 5) * 5;
        DurationSlider.Value = _selectedDuration;
        DurationLabel.Text = $"{_selectedDuration} хвилин";
    }

    private async void OnGenerateClicked(object sender, TappedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedType))
        {
            await DisplayAlert("Увага", "Спочатку обери тип тренування!", "OK");
            return;
        }

        try
        {
            GenerateButton.IsEnabled = false;
            LoadingContainer.IsVisible = true;
            ResultContainer.IsVisible = false;
            InitialMessage.IsVisible = false;

            int userId = Preferences.Get("UserId", 0);
            if (userId <= 0)
            {
                await DisplayAlert("Помилка", "Не вдалося ідентифікувати користувача.", "OK");
                return;
            }

            var goal = _selectedType switch
            {
                "cardio" => "кардіо тренування",
                "gym" => "силове тренування в залі",
                "home" => "тренування вдома без обладнання",
                _ => "загальне тренування"
            };

            var plan = await _apiService.GenerateWorkoutAsync(
                userId,
                goal,
                "medium",
                _selectedDuration);

            if (plan != null && !string.IsNullOrEmpty(plan.PlanText))
            {
                DisplayWorkout(plan.PlanText);
            }
            else
            {
                await DisplayAlert("Помилка", "Не вдалося згенерувати тренування.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "OK");
        }
        finally
        {
            GenerateButton.IsEnabled = true;
            LoadingContainer.IsVisible = false;
        }
    }

    private void DisplayWorkout(string json)
    {
        try
        {
            json = CleanJson(json);

            var workout = JsonSerializer.Deserialize<ApiService.WorkoutJsonResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (workout == null)
            {
                ShowAsText(json);
                return;
            }

            // Оновлюємо UI
            TypeIcon.Text = _selectedType switch
            {
                "cardio" => "??",
                "gym" => "???",
                "home" => "??",
                _ => "??"
            };
            TypeLabel.Text = _selectedType switch
            {
                "cardio" => "Кардіо",
                "gym" => "Силове",
                "home" => "Вдома",
                _ => "Тренування"
            };
            DurationResultLabel.Text = $"{_selectedDuration} хв";
            CaloriesLabel.Text = $"~{workout.TotalCalories} ккал";
            SummaryLabel.Text = workout.Summary;

            // Розминка
            if (workout.Warmup != null)
            {
                WarmupDurationLabel.Text = $"{workout.Warmup.Duration} хв";
                WarmupExercisesLabel.Text = string.Join("\n", workout.Warmup.Exercises?.Select(ex => $"• {ex}") ?? Array.Empty<string>());
                WarmupContainer.IsVisible = true;
            }

            // Вправи
            ExercisesContainer.Children.Clear();
            if (workout.Workout != null)
            {
                int index = 1;
                foreach (var exercise in workout.Workout)
                {
                    ExercisesContainer.Children.Add(CreateExerciseCard(exercise, index++));
                }
            }

            // Заминка
            if (workout.Cooldown != null)
            {
                CooldownDurationLabel.Text = $"{workout.Cooldown.Duration} хв";
                CooldownExercisesLabel.Text = string.Join("\n", workout.Cooldown.Exercises?.Select(ex => $"• {ex}") ?? Array.Empty<string>());
                CooldownContainer.IsVisible = true;
            }

            ResultContainer.IsVisible = true;
        }
        catch
        {
            ShowAsText(json);
        }
    }

    private Border CreateExerciseCard(ApiService.ExerciseItem exercise, int index)
    {
        var card = new Border
        {
            Stroke = (Color)Application.Current.Resources["BorderColor"],
            BackgroundColor = (Color)Application.Current.Resources["CardBackground"],
            StrokeThickness = 1,
            Padding = new Thickness(14),
            StrokeShape = new RoundRectangle { CornerRadius = 14 }
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(40) },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 12
        };

        // Номер вправи
        var numberBorder = new Border
        {
            WidthRequest = 36,
            HeightRequest = 36,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            BackgroundColor = (Color)Application.Current.Resources["Primary"],
            Stroke = Colors.Transparent
        };
        var numberLabel = new Label
        {
            Text = index.ToString(),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        numberBorder.Content = numberLabel;
        grid.Add(numberBorder, 0, 0);

        // Інформація про вправу
        var infoStack = new VerticalStackLayout { Spacing = 4 };

        infoStack.Add(new Label
        {
            Text = exercise.Name,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current.Resources["TextPrimary"]
        });

        var detailsStack = new HorizontalStackLayout { Spacing = 12 };
        detailsStack.Add(new Label
        {
            Text = $"{exercise.Sets} x {exercise.Reps}",
            FontSize = 13,
            TextColor = (Color)Application.Current.Resources["Primary"],
            FontAttributes = FontAttributes.Bold
        });
        detailsStack.Add(new Label
        {
            Text = $"Відпочинок: {exercise.Rest}",
            FontSize = 12,
            TextColor = (Color)Application.Current.Resources["TextSecondary"]
        });
        infoStack.Add(detailsStack);

        if (!string.IsNullOrEmpty(exercise.Tips))
        {
            infoStack.Add(new Label
            {
                Text = $"?? {exercise.Tips}",
                FontSize = 12,
                TextColor = (Color)Application.Current.Resources["TextSecondary"],
                LineBreakMode = LineBreakMode.WordWrap
            });
        }

        grid.Add(infoStack, 1, 0);
        card.Content = grid;
        return card;
    }

    private void ShowAsText(string text)
    {
        ExercisesContainer.Children.Clear();
        ExercisesContainer.Children.Add(new Label
        {
            Text = text,
            FontSize = 14,
            TextColor = (Color)Application.Current.Resources["TextPrimary"],
            LineHeight = 1.5
        });
        WarmupContainer.IsVisible = false;
        CooldownContainer.IsVisible = false;
        ResultContainer.IsVisible = true;
    }

    private string CleanJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;

        json = json.Trim();
        if (json.StartsWith("```json")) json = json.Substring(7);
        else if (json.StartsWith("```")) json = json.Substring(3);
        if (json.EndsWith("```")) json = json.Substring(0, json.Length - 3);

        return json.Trim();
    }
}