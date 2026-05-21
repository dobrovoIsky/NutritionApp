using Android;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Maui.Storage;
using NutritionApp.Services;

namespace NutritionApp.Platforms.Android;

public class PedometerService : Java.Lang.Object, IPedometerService, ISensorEventListener
{
    private const string BaselineDateKey = "PedometerBaselineDate";
    private const string BaselineValueKey = "PedometerBaselineValue";

    private SensorManager? _sensorManager;
    private Sensor? _stepCounter;
    private float _baseline;
    private int _todaySteps;

    public bool IsSupported { get; private set; }

    public int TodaySteps => _todaySteps;

    public event EventHandler<int>? StepsChanged;

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

        var context = Platform.CurrentActivity ?? Platform.AppContext;
        _sensorManager = (SensorManager?)context.GetSystemService(Context.SensorService);
        _stepCounter = _sensorManager?.GetDefaultSensor(SensorType.StepCounter);

        if (_stepCounter == null)
        {
            IsSupported = false;
            return;
        }

        IsSupported = true;
        EnsureBaselineForToday(0);
        _sensorManager.RegisterListener(this, _stepCounter, SensorDelay.Normal);
    }

    public void Stop()
    {
        if (_sensorManager != null && _stepCounter != null)
            _sensorManager.UnregisterListener(this, _stepCounter);
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
            _baseline = cumulative > 0 ? cumulative : 0;
            Preferences.Set(BaselineDateKey, today);
            Preferences.Set(BaselineValueKey, _baseline);
            UpdateSteps(cumulative);
            return;
        }

        _baseline = (float)Preferences.Get(BaselineValueKey, 0f);
    }

    private void UpdateSteps(float cumulative)
    {
        var steps = (int)Math.Max(0, cumulative - _baseline);
        if (steps == _todaySteps)
            return;

        _todaySteps = steps;
        StepsChanged?.Invoke(this, _todaySteps);
    }
}
