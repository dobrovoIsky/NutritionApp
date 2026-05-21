namespace NutritionApp.Services;

public interface IPedometerService
{
    bool IsSupported { get; }

    Task<bool> EnsurePermissionAsync();

    Task StartAsync();

    void Stop();

    int TodaySteps { get; }

    event EventHandler<int>? StepsChanged;
}
