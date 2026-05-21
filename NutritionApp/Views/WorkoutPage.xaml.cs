using Microsoft.Maui.Controls.Shapes;
using NutritionApp.Services;
using System.Text.Json;

namespace NutritionApp.Views;

public partial class WorkoutPage : ContentPage
{
    private readonly ApiService _apiService;
    private string _selectedType = "";
    private int _selectedDuration = 45;
    private List<string> _selectedEquipment = new();

    public WorkoutPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        
        LoadSavedEquipment();
        UpdateEquipmentButtonText();
    }

    private void LoadSavedEquipment()
    {
        try
        {
            var json = Preferences.Get("FavoriteEquipment", "");
            if (!string.IsNullOrEmpty(json))
            {
                _selectedEquipment = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new();
            }
        }
        catch { }
    }

    private void UpdateEquipmentButtonText()
    {
        if (_selectedEquipment.Count > 0)
        {
            EquipmentButtonLabel.Text = $"Obladnannia ({_selectedEquipment.Count})";
        }
        else
        {
            EquipmentButtonLabel.Text = "Obrati obladnannia";
        }
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
        selectedCard.BackgroundColor = Color.FromArgb("#1A1A1A");

        InitialMessage.IsVisible = false;
        
        EquipmentContainer.IsVisible = (_selectedType == "gym");
    }

    private void OnCardioTapped(object sender, TappedEventArgs e)
    {
        _selectedType = "cardio";
        SelectCard(CardioCard);
    }

    private void OnGymTapped(object sender, TappedEventArgs e)
    {
        _selectedType = "gym";
        SelectCard(GymCard);
    }

    private void OnHomeTapped(object sender, TappedEventArgs e)
    {
        _selectedType = "home";
        SelectCard(HomeCard);
    }

    private async void OnEquipmentClicked(object sender, TappedEventArgs e)
    {
        var page = new EquipmentSelectionPage(_selectedEquipment, OnEquipmentSelected);
        await Shell.Current.Navigation.PushAsync(page);
    }

    private void OnEquipmentSelected(List<string> selectedEquipment)
    {
        _selectedEquipment = selectedEquipment ?? new();
        UpdateEquipmentButtonText();
    }

    private void OnDurationChanged(object sender, ValueChangedEventArgs e)
    {
        _selectedDuration = (int)Math.Round(e.NewValue / 5) * 5;
        DurationSlider.Value = _selectedDuration;
        DurationLabel.Text = $"{_selectedDuration} khv";
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedType))
        {
            await DisplayAlert("Uvaga", "Spochatku oberit typ!", "OK");
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
                await DisplayAlert("Pomylka", "Ne znaideno korystuvacha.", "OK");
                return;
            }

            var goal = _selectedType switch
            {
                "cardio" => "kardio trenuvannia",
                "gym" => "sylove trenuvannia v zali",
                "home" => "trenuvannia vdoma",
                _ => "zahalne trenuvannia"
            };

            var equipment = _selectedType == "gym" ? _selectedEquipment : null;

            var plan = await _apiService.GenerateWorkoutAsync(
                userId,
                goal,
                "medium",
                _selectedDuration,
                equipment);

            if (plan != null && !string.IsNullOrEmpty(plan.PlanText))
            {
                DisplayWorkout(plan.PlanText);
            }
            else
            {
                await DisplayAlert("Pomylka", "Ne vdalosya zgeneruvaty.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Pomylka", $"Stalasya pomylka: {ex.Message}", "OK");
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

            TypeIcon.Text = _selectedType switch
            {
                "cardio" => "R",
                "gym" => "G",
                "home" => "H",
                _ => "T"
            };
            TypeLabel.Text = _selectedType switch
            {
                "cardio" => "Kardio",
                "gym" => "Zal",
                "home" => "Vdoma",
                _ => "Trenuvannia"
            };
            DurationResultLabel.Text = $"{_selectedDuration} khv";
            CaloriesLabel.Text = $"~{workout.TotalCalories} kkal";
            SummaryLabel.Text = workout.Summary;

            if (workout.Warmup != null)
            {
                WarmupDurationLabel.Text = $"{workout.Warmup.Duration} khv";
                WarmupExercisesLabel.Text = string.Join("\n", workout.Warmup.Exercises?.Select(ex => $"* {ex}") ?? Array.Empty<string>());
                WarmupContainer.IsVisible = true;
            }

            ExercisesContainer.Children.Clear();
            if (workout.Workout != null)
            {
                int index = 1;
                foreach (var exercise in workout.Workout)
                {
                    ExercisesContainer.Children.Add(CreateExerciseCard(exercise, index++));
                }
            }

            if (workout.Cooldown != null)
            {
                CooldownDurationLabel.Text = $"{workout.Cooldown.Duration} khv";
                CooldownExercisesLabel.Text = string.Join("\n", workout.Cooldown.Exercises?.Select(ex => $"* {ex}") ?? Array.Empty<string>());
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
            Text = $"Rest: {exercise.Rest}",
            FontSize = 12,
            TextColor = (Color)Application.Current.Resources["TextSecondary"]
        });
        infoStack.Add(detailsStack);

        if (!string.IsNullOrEmpty(exercise.Tips))
        {
            infoStack.Add(new Label
            {
                Text = $"Tip: {exercise.Tips}",
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
