using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace NutritionApp.Views;

[QueryProperty(nameof(PlanText), "PlanText")]
public partial class HistoryDetailPage : ContentPage
{
    private string _planText;
    public string PlanText
    {
        get => _planText;
        set
        {
            _planText = value;
            DisplayPlan(value);
        }
    }

    private readonly Services.ApiService _apiService;

    public HistoryDetailPage(Services.ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private void DisplayPlan(string planJson)
    {
        MealsContainer.Children.Clear();

        if (string.IsNullOrWhiteSpace(planJson))
        {
            AddErrorMessage("Немає даних для відображення.");
            return;
        }

        try
        {
            var cleanJson = CleanJsonResponse(planJson);
            var plan = JsonSerializer.Deserialize<MealPlanData>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (plan != null && plan.Meals != null && plan.Meals.Count > 0)
            {
                // Summary
                if (!string.IsNullOrEmpty(plan.Summary))
                {
                    SummaryLabel.Text = plan.Summary;
                    SummaryLabel.IsVisible = true;
                }

                // Додаємо карточки для кожного прийому їжі
                foreach (var meal in plan.Meals)
                {
                    MealsContainer.Children.Add(CreateMealCard(meal));
                }
            }
            else
            {
                // Показуємо як текст
                AddTextLabel(planJson);
            }
        }
        catch
        {
            AddTextLabel(planJson);
        }
    }

    private Border CreateMealCard(MealData meal)
    {
        var card = new Border
        {
            Stroke = (Color)Application.Current.Resources["BorderColor"],
            BackgroundColor = (Color)Application.Current.Resources["CardBackground"],
            StrokeThickness = 1,
            Padding = new Thickness(16),
            StrokeShape = new RoundRectangle { CornerRadius = 16 }
        };

        var mainStack = new VerticalStackLayout { Spacing = 12 };

        // Заголовок
        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var titleLabel = new Label
        {
            Text = meal.Name,
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current.Resources["TextPrimary"]
        };

        var timeBorder = new Border
        {
            BackgroundColor = Color.FromArgb("#EFF6FF"),
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(8, 4)
        };
        var timeLabel = new Label
        {
            Text = meal.Time,
            FontSize = 13,
            TextColor = (Color)Application.Current.Resources["Primary"],
            FontAttributes = FontAttributes.Bold
        };
        timeBorder.Content = timeLabel;

        headerGrid.Add(titleLabel, 0, 0);
        headerGrid.Add(timeBorder, 1, 0);
        mainStack.Add(headerGrid);

        // Продукти
        if (meal.Foods != null)
        {
            foreach (var food in meal.Foods)
            {
                mainStack.Add(CreateFoodItem(food));
            }
        }

        // Загальні калорії
        if (meal.TotalCalories > 0)
        {
            var totalBorder = new Border
            {
                BackgroundColor = Color.FromArgb("#FFF7ED"),
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Stroke = Colors.Transparent,
                Padding = new Thickness(10, 6),
                HorizontalOptions = LayoutOptions.End
            };
            var totalLabel = new Label
            {
                Text = $"Всього: {meal.TotalCalories} ккал",
                FontSize = 13,
                TextColor = Color.FromArgb("#EA580C"),
                FontAttributes = FontAttributes.Bold
            };
            totalBorder.Content = totalLabel;
            mainStack.Add(totalBorder);
        }

        card.Content = mainStack;
        return card;
    }

    private Border CreateFoodItem(FoodData food)
    {
        var border = new Border
        {
            BackgroundColor = (Color)Application.Current.Resources["PageBackground"],
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(12)
        };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        // Назва продукту
        var nameLabel = new Label
        {
            Text = food.Name,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current.Resources["TextPrimary"]
        };
        grid.Add(nameLabel, 0, 0);

        // Вага
        var weightLabel = new Label
        {
            Text = food.Weight ?? "",
            FontSize = 13,
            TextColor = (Color)Application.Current.Resources["TextSecondary"],
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        grid.Add(weightLabel, 1, 0);

        // Кнопка "+"
        var addBtn = new Label
        {
            Text = "+",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current.Resources["Primary"],
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) => await LogFoodFromHistoryAsync(food);
        addBtn.GestureRecognizers.Add(tapGesture);
        
        // Кнопку ставимо так щоб займала 2 рядки по висоті (RowSpan = 2)
        Grid.SetRowSpan(addBtn, 2);
        grid.Add(addBtn, 2, 0);

        // БЖУ
        var bjuStack = new HorizontalStackLayout { Spacing = 12 };
        bjuStack.Add(new Label { Text = $"{food.Calories} ккал", FontSize = 12, TextColor = Color.FromArgb("#EA580C") });
        bjuStack.Add(new Label { Text = $"Б: {food.Protein}", FontSize = 12, TextColor = Color.FromArgb("#16A34A") });
        bjuStack.Add(new Label { Text = $"Ж: {food.Fat}", FontSize = 12, TextColor = Color.FromArgb("#CA8A04") });
        bjuStack.Add(new Label { Text = $"В: {food.Carbs}", FontSize = 12, TextColor = Color.FromArgb("#2563EB") });

        Grid.SetRow(bjuStack, 1);
        Grid.SetColumnSpan(bjuStack, 2); // БЖУ займає перші дві колонки
        grid.Add(bjuStack);

        border.Content = grid;
        return border;
    }

    private async Task LogFoodFromHistoryAsync(FoodData food)
    {
        if (_apiService == null) return;
        
        int userId = Preferences.Get("UserId", 0);
        if (userId == 0) return;

        double weightNum = 0;
        if (!string.IsNullOrEmpty(food.Weight))
        {
            var cleanedWeight = new string(food.Weight.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            double.TryParse(cleanedWeight.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out weightNum);
        }

        var entry = new Models.FoodEntry
        {
            UserId = userId,
            Name = food.Name,
            Calories = food.Calories,
            Protein = food.Protein,
            Fat = food.Fat,
            Carbs = food.Carbs,
            Weight = weightNum,
            LoggedAt = DateTime.UtcNow
        };

        var saved = await _apiService.LogFoodAsync(entry);
        if (saved != null)
        {
            await Application.Current.MainPage.DisplayAlert("Успіх", $"{food.Name} додано до сьогоднішнього меню!", "ОК");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося зберегти продукт.", "ОК");
        }
    }

    private void AddErrorMessage(string message)
    {
        var label = new Label
        {
            Text = message,
            FontSize = 16,
            TextColor = (Color)Application.Current.Resources["TextSecondary"],
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        MealsContainer.Children.Add(label);
    }

    private void AddTextLabel(string text)
    {
        var label = new Label
        {
            Text = text,
            FontSize = 15,
            TextColor = (Color)Application.Current.Resources["TextPrimary"],
            LineHeight = 1.4
        };
        MealsContainer.Children.Add(label);
    }

    private string CleanJsonResponse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        json = json.Trim();

        if (json.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            json = json.Substring(7);
        else if (json.StartsWith("```"))
            json = json.Substring(3);

        if (json.EndsWith("```"))
            json = json.Substring(0, json.Length - 3);

        return json.Trim();
    }

    private async void OnBackClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // Класи для парсингу JSON
    private class MealPlanData
    {
        public string Summary { get; set; }
        public List<MealData> Meals { get; set; }
    }

    private class MealData
    {
        public string Name { get; set; }
        public string Time { get; set; }
        public List<FoodData> Foods { get; set; }
        public int TotalCalories { get; set; }
    }

    private class FoodData
    {
        public string Name { get; set; }
        public string Weight { get; set; }
        public int Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbs { get; set; }
    }
}
