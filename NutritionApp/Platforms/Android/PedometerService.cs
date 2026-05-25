using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Maui.Storage;
using NutritionApp.Services;

namespace NutritionApp.Platforms.Android;

public class PedometerService : IPedometerService
{
    private int _todaySteps;
    public bool IsSupported { get; private set; }

    public int TodaySteps => _todaySteps;

    public event EventHandler<int>? StepsChanged;

    public PedometerService()
    {
        Microsoft.Maui.Controls.MessagingCenter.Subscribe<PedometerForegroundService, int>(this, "StepsUpdated", (sender, steps) =>
        {
            _todaySteps = steps;
            StepsChanged?.Invoke(this, steps);
        });
        
        // Initial load
        _todaySteps = Preferences.Get("TodaySteps", 0);
    }

    public Task<bool> EnsurePermissionAsync()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
            return Task.FromResult(true);

        var activity = Platform.CurrentActivity;
        if (activity == null)
            return Task.FromResult(false);

        if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.ActivityRecognition) == Permission.Granted)
            return Task.FromResult(true);

        ActivityCompat.RequestPermissions(activity, new[] { Manifest.Permission.ActivityRecognition }, 9001);
        var granted = ContextCompat.CheckSelfPermission(activity, Manifest.Permission.ActivityRecognition) == Permission.Granted;
        return Task.FromResult(granted);
    }

    public async Task StartAsync()
    {
        if (!await EnsurePermissionAsync())
        {
            IsSupported = false;
            return;
        }

        IsSupported = true;
        
        var context = Platform.CurrentActivity ?? Platform.AppContext;
        var intent = new Intent(context, typeof(PedometerForegroundService));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            context.StartForegroundService(intent);
        else
            context.StartService(intent);
            
        // Initial trigger to refresh UI
        StepsChanged?.Invoke(this, _todaySteps);
    }

    public void Stop()
    {
        var context = Platform.CurrentActivity ?? Platform.AppContext;
        var intent = new Intent(context, typeof(PedometerForegroundService));
        context.StopService(intent);
    }
}
