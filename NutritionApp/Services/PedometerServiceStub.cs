namespace NutritionApp.Services;

public class PedometerServiceStub : IPedometerService
{
    public bool IsSupported => false;

    public int TodaySteps => 0;

    public event EventHandler<int>? StepsChanged;

    public Task<bool> EnsurePermissionAsync() => Task.FromResult(false);

    public Task StartAsync() => Task.CompletedTask;

    public void Stop() { }
}
