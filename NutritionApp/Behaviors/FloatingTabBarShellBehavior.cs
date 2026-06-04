using NutritionApp.Controls;

namespace NutritionApp.Behaviors;

public class FloatingTabBarShellBehavior : Behavior<Shell>
{
    public static readonly BindableProperty IsDockHostProperty =
        BindableProperty.CreateAttached(
            "IsDockHost",
            typeof(bool),
            typeof(FloatingTabBarShellBehavior),
            false);

    private static readonly HashSet<string> RootTabPages =
    [
        nameof(Views.MainPage),
        nameof(Views.MealPlanPage),
        nameof(Views.SavedRecipesPage),
        nameof(Views.LeaderboardPage),
        nameof(Views.ProfilePage)
    ];

    private Shell? _shell;
    private FloatingTabBar? _tabBar;

    protected override void OnAttachedTo(Shell shell)
    {
        base.OnAttachedTo(shell);
        _shell = shell;
        _tabBar = new FloatingTabBar();
        shell.Navigated += OnShellNavigated;
    }

    protected override void OnDetachingFrom(Shell shell)
    {
        shell.Navigated -= OnShellNavigated;
        base.OnDetachingFrom(shell);
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        if (_shell == null || _tabBar == null)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_shell.CurrentPage is not ContentPage page)
            {
                _tabBar.IsVisible = false;
                return;
            }

            var pageName = page.GetType().Name;
            if (!RootTabPages.Contains(pageName))
            {
                _tabBar.IsVisible = false;
                return;
            }

            EnsureTabBarOnPage(page);
            _tabBar.UpdateSelectedRoute(pageName);
            _tabBar.IsVisible = true;
        });
    }

    private void EnsureTabBarOnPage(ContentPage page)
    {
        if (_tabBar == null)
            return;

        if (_tabBar.Parent is Grid oldHost)
            oldHost.Children.Remove(_tabBar);

        if (page.GetValue(IsDockHostProperty) is not true)
        {
            var content = page.Content;
            if (content == null)
                return;

            var host = new Grid();
            host.Children.Add(content);
            page.Content = host;
            page.SetValue(IsDockHostProperty, true);
        }

        if (page.Content is Grid hostGrid && !hostGrid.Children.Contains(_tabBar))
            hostGrid.Children.Add(_tabBar);
    }
}
