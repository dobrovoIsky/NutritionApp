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

        LoadWaterReminderSettings();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadUserProfileAsync(forceRefresh: true);
        await _viewModel.CheckStreakAsync();
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
                _waterReminderService.StartRemindersInBackground();
                await DisplayAlert("Enabled", "Water reminders enabled!", "OK");
            }
            else
            {
                WaterReminderSwitch.Toggled -= OnWaterReminderToggled;
                WaterReminderSwitch.IsToggled = false;
                WaterReminderSwitch.Toggled += OnWaterReminderToggled;
                _waterReminderService.IsEnabled = false;
                IntervalPickerContainer.IsVisible = false;
                await DisplayAlert("No Permission", "Notification permission required.", "OK");
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
            _waterReminderService.RestartRemindersInBackground();
        }
    }

    private async void OnLogoutButtonClicked(object sender, EventArgs e)
    {
        _waterReminderService.StopReminders();
        _viewModel.Reset();
        ApiService.ClearMemoryCache();
        Preferences.Clear();
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {           
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }


}
