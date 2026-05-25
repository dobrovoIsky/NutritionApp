using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Android.Content.PM;
using Android.Hardware;
using Microsoft.Maui.Storage;
using System;

namespace NutritionApp.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeHealth, Exported = false)]
public class PedometerForegroundService : Service, ISensorEventListener
{
    private const int SERVICE_ID = 10001;
    private const string CHANNEL_ID = "PedometerChannel";
    private const string BaselineDateKey = "PedometerBaselineDate";
    private const string BaselineValueKey = "PedometerBaselineValue";

    private SensorManager? _sensorManager;
    private Sensor? _stepCounter;

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        CreateNotificationChannel();

        var notification = new NotificationCompat.Builder(this, CHANNEL_ID)
            .SetContentTitle("TrackBite")
            .SetContentText("Фонове відстеження кроків активне")
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetOngoing(true)
            .Build();

        StartForeground(SERVICE_ID, notification, ForegroundService.TypeHealth);

        _sensorManager = (SensorManager?)GetSystemService(SensorService);
        _stepCounter = _sensorManager?.GetDefaultSensor(SensorType.StepCounter);

        if (_stepCounter != null)
        {
            _sensorManager?.RegisterListener(this, _stepCounter, SensorDelay.Normal);
        }

        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        _sensorManager?.UnregisterListener(this);
        base.OnDestroy();
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(CHANNEL_ID, "Step Counter", NotificationImportance.Low)
            {
                Description = "Used for step counting in background"
            };
            var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
            notificationManager.CreateNotificationChannel(channel);
        }
    }

    public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy) { }

    public void OnSensorChanged(SensorEvent? e)
    {
        if (e?.Values == null || e.Values.Count == 0)
            return;

        var cumulative = e.Values[0];
        EnsureBaselineForToday(cumulative);
        UpdateSteps(cumulative);
    }

    private void EnsureBaselineForToday(float cumulative)
    {
        var today = DateTime.Today.ToString("O");
        var savedDate = Preferences.Get(BaselineDateKey, string.Empty);
        
        if (savedDate != today)
        {
            Preferences.Set(BaselineDateKey, today);
            Preferences.Set(BaselineValueKey, cumulative);
        }
    }

    private void UpdateSteps(float cumulative)
    {
        var baseline = Preferences.Get(BaselineValueKey, 0f);
        var steps = (int)(cumulative - baseline);
        
        if (steps < 0) steps = 0;
        
        Preferences.Set("TodaySteps", steps);
        
        Microsoft.Maui.Controls.MessagingCenter.Send(this, "StepsUpdated", steps);
    }
}
