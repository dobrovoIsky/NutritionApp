using NutritionApp.Services;
using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;
    private readonly WaterReminderService _waterReminderService;

    public ProfilePage(ProfileViewModel viewModel, WaterReminderService waterReminderService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _waterReminderService = waterReminderService;
        BindingContext = _viewModel;

        if (Application.Current.Resources.TryGetValue("PageBackgroundColor", out var colorValue) && colorValue is Color color)
        {
            ThemeSwitch.IsToggled = color != Colors.White;
        }

        LoadWaterReminderSettings();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadUserProfileAsync();
    }

    private void LoadWaterReminderSettings()
    {
        WaterReminderSwitch.Toggled -= OnWaterReminderToggled;
        WaterReminderSwitch.IsToggled = _waterReminderService.IsEnabled;
        WaterReminderSwitch.Toggled += OnWaterReminderToggled;

        IntervalPickerContainer.IsVisible = _waterReminderService.IsEnabled;

        var interval = _waterReminderService.IntervalMinutes;
        IntervalPicker.SelectedIndexChanged -= OnIntervalChanged;
        IntervalPicker.SelectedIndex = interval switch
        {
            30 => 0,
            45 => 1,
            60 => 2,
            90 => 3,
            120 => 4,
            _ => 2
        };
        IntervalPicker.SelectedIndexChanged += OnIntervalChanged;
    }

    private async void OnWaterReminderToggled(object sender, ToggledEventArgs e)
    {
        _waterReminderService.IsEnabled = e.Value;
        IntervalPickerContainer.IsVisible = e.Value;

        if (e.Value)
        {
            var hasPermission = await _waterReminderService.RequestPermissionAsync();
            if (hasPermission)
            {
                // Запускаємо у фоні - UI не блокується!
                _waterReminderService.StartRemindersInBackground();

                await DisplayAlert("Нагадування",
                    "Нагадування про воду увімкнено!\nСповіщення з 8:00 до 22:00.", "OK");
            }
            else
            {
                WaterReminderSwitch.Toggled -= OnWaterReminderToggled;
                WaterReminderSwitch.IsToggled = false;
                WaterReminderSwitch.Toggled += OnWaterReminderToggled;
                _waterReminderService.IsEnabled = false;
                IntervalPickerContainer.IsVisible = false;

                await DisplayAlert("Дозвіл потрібен",
                    "Для нагадувань потрібен дозвіл на сповіщення.", "OK");
            }
        }
        else
        {
            _waterReminderService.StopReminders();
        }
    }

    private void OnIntervalChanged(object sender, EventArgs e)
    {
        var interval = IntervalPicker.SelectedIndex switch
        {
            0 => 30,
            1 => 45,
            2 => 60,
            3 => 90,
            4 => 120,
            _ => 60
        };

        _waterReminderService.IntervalMinutes = interval;

        if (_waterReminderService.IsEnabled)
        {
            // Перезапускаємо у фоні
            _waterReminderService.RestartRemindersInBackground();
        }
    }

    private void OnThemeToggled(object sender, ToggledEventArgs e)
    {
        Color customGray = Color.FromArgb("#EAEAEA");

        if (e.Value)
        {
            Application.Current.Resources["PageBackgroundColor"] = customGray;
        }
        else
        {
            Application.Current.Resources["PageBackgroundColor"] = Colors.White;
        }
    }

    private void OnLogoutButtonClicked(object sender, EventArgs e)
    {
        _waterReminderService.StopReminders();
        Preferences.Clear();
        Shell.Current.GoToAsync("//LoginPage");
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }

    private void OnAvatarTapped(object sender, TappedEventArgs e)
    {
        AvatarPopup.IsVisible = true;
    }

    private void OnClosePopup(object sender, EventArgs e)
    {
        AvatarPopup.IsVisible = false;
    }

    private void OnAvatarSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
            {
                AvatarPopup.IsVisible = false;
            });
        }
    }
}