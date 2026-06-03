using System.Diagnostics;
using System.Globalization;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.Views;

public partial class EditProfilePage : ContentPage
{
    private readonly ApiService _apiService;
    private UserProfile _profile;

    public EditProfilePage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            SaveButton.IsEnabled = false;

            int userId = Preferences.Get("UserId", 0);
            if (userId <= 0) return;

            _profile = await _apiService.GetUserProfileAsync(userId);
            if (_profile == null) return;

            // Заповнюємо поля даними з профілю
            UsernameEntry.Text = _profile.Username ?? "";
            HeightEntry.Text = _profile.Height.ToString(CultureInfo.InvariantCulture);
            WeightEntry.Text = _profile.Weight.ToString(CultureInfo.InvariantCulture);
            AgeEntry.Text = _profile.Age.ToString();

            // Встановлюємо Picker через SelectedIndex (без binding!)
            GoalPicker.SelectedIndex = GetGoalIndex(_profile.Goal);
            ActivityPicker.SelectedIndex = GetActivityIndex(_profile.ActivityLevel);
            GenderPicker.SelectedIndex = GetGenderIndex(_profile.Gender);

            Debug.WriteLine($"EditProfile loaded: {_profile.Username}, H={_profile.Height}, W={_profile.Weight}, Goal={_profile.Goal}, Activity={_profile.ActivityLevel}, Gender={_profile.Gender}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"EditProfile load error: {ex.Message}");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            SaveButton.IsEnabled = true;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_profile == null) return;

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            SaveButton.IsEnabled = false;

            int userId = Preferences.Get("UserId", 0);
            if (userId <= 0)
            {
                await DisplayAlert("Помилка", "Не вдалося ідентифікувати користувача.", "OK");
                return;
            }

            // Зчитуємо дані з полів
            _profile.Username = UsernameEntry.Text?.Trim() ?? _profile.Username;
            _profile.Height = ParseDouble(HeightEntry.Text);
            _profile.Weight = ParseDouble(WeightEntry.Text);
            _profile.Age = ParseInt(AgeEntry.Text);
            _profile.Goal = ConvertGoalToEn(GoalPicker.SelectedItem as string);
            _profile.ActivityLevel = ConvertActivityToEn(ActivityPicker.SelectedItem as string);
            _profile.Gender = ConvertGenderToEn(GenderPicker.SelectedItem as string);

            Debug.WriteLine($"EditProfile saving: H={_profile.Height}, W={_profile.Weight}, Goal={_profile.Goal}, Activity={_profile.ActivityLevel}, Gender={_profile.Gender}");

            var updatedProfile = await _apiService.UpdateUserProfileAsync(userId, _profile);
            if (updatedProfile != null)
            {
                // Очищуємо кеш профілю
                await DisplayAlert("Успіх", "Профіль оновлено!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Помилка", "Не вдалося оновити профіль.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"EditProfile save error: {ex.Message}");
            await DisplayAlert("Помилка", $"Помилка: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            SaveButton.IsEnabled = true;
        }
    }

    // === Конвертація індексів ===

    private int GetGoalIndex(string goal) => goal switch
    {
        "lose weight" => 0,
        "gain muscle" => 1,
        "maintain weight" => 2,
        _ => 2
    };

    private int GetActivityIndex(string activity) => activity switch
    {
        "sedentary" => 0,
        "lightly active" => 1,
        "moderately active" => 2,
        "very active" => 3,
        "extra active" => 4,
        _ => 2
    };

    private int GetGenderIndex(string gender) => gender switch
    {
        "male" => 0,
        "female" => 1,
        _ => 0
    };

    // === Конвертація назад в англійську ===

    private string ConvertGoalToEn(string goal) => goal switch
    {
        "Схуднення" => "lose weight",
        "Набір маси" => "gain muscle",
        "Підтримка ваги" => "maintain weight",
        _ => "maintain weight"
    };

    private string ConvertActivityToEn(string activity) => activity switch
    {
        "Сидячий спосіб життя" => "sedentary",
        "Легка активність" => "lightly active",
        "Помірна активність" => "moderately active",
        "Висока активність" => "very active",
        "Дуже висока активність" => "extra active",
        _ => "moderately active"
    };

    private string ConvertGenderToEn(string gender) => gender switch
    {
        "Чоловіча" => "male",
        "Жіноча" => "female",
        _ => "male"
    };

    // === Парсери ===

    private double ParseDouble(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v);
        return v;
    }

    private int ParseInt(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        int.TryParse(s, out var v);
        return v;
    }
}