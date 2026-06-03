using Fonts;
using Microsoft.Maui.Controls.Shapes;
using NutritionApp.Models;

namespace NutritionApp.Controls;

public partial class FloatingTabBar : ContentView
{
    public static readonly BindableProperty SelectedRouteProperty =
        BindableProperty.Create(nameof(SelectedRoute), typeof(string), typeof(FloatingTabBar), string.Empty,
            propertyChanged: (bindable, _, _) =>
            {
                if (bindable is FloatingTabBar bar)
                    bar.UpdateTabVisuals();
            });

    private readonly IReadOnlyList<DockTabItem> _tabs =
    [
        new() { Route = "MainPage", Title = "Головна", IconImage = "home_silhouette.png" },
        new() { Route = "MealPlanPage", Title = "Раціон", IconImage = "restaurant_icon.png" },
        new() { Route = "SavedRecipesPage", Title = "Рецепти", IconImage = "history_icon.png" },
        new() { Route = "ProfilePage", Title = "Профіль", IconImage = "avatar_icon.png" }
    ];

    private readonly Dictionary<string, (Border Pill, Image Icon, Label Title)> _tabViews = new();
    private bool _isNavigating;

    public string SelectedRoute
    {
        get => (string)GetValue(SelectedRouteProperty);
        set => SetValue(SelectedRouteProperty, value);
    }

    public FloatingTabBar()
    {
        InitializeComponent();
        BuildTabs();
    }

    private void BuildTabs()
    {
        TabsHost.Children.Clear();
        _tabViews.Clear();

        foreach (var tab in _tabs)
        {
            var icon = new Image
            {
                Source = tab.IconImage,
                WidthRequest = 18,
                HeightRequest = 18,
                Aspect = Aspect.AspectFit,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(13, 0, 0, 0), // Centered exactly in a 44px wide pill: (44 - 18) / 2 = 13
                Opacity = 0.7 // Brighter unselected icons
            };

            var title = new Label
            {
                Text = tab.Title,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#FFFFFF"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(36, 0, 0, 0), // Next to the icon
                Opacity = 0,
                LineBreakMode = LineBreakMode.NoWrap
            };

            var content = new Grid
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };
            
            content.Add(title);
            content.Add(icon);

            var pill = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 18 },
                BackgroundColor = Colors.Transparent,
                Padding = 0, // Removed padding since we use exact margins
                WidthRequest = 44, // Initial unselected width
                HeightRequest = 36,
                VerticalOptions = LayoutOptions.Center,
                Content = content
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) => await NavigateToTabAsync(tab.Route);
            pill.GestureRecognizers.Add(tap);

            _tabViews[tab.Route] = (pill, icon, title);
            TabsHost.Children.Add(pill);
        }

        UpdateTabVisuals(false);
    }

    private async Task NavigateToTabAsync(string route)
    {
        if (_isNavigating || Shell.Current == null || SelectedRoute == route)
            return;

        _isNavigating = true;
        try
        {
            if (Shell.Current is not Shell shell)
                return;

            var tabBar = shell.Items.OfType<TabBar>().FirstOrDefault(i => i.Route == "MainApp")
                ?? shell.CurrentItem as TabBar;

            var target = tabBar?.Items.FirstOrDefault(i => i.Route == route);
            if (tabBar != null && target != null)
            {
                shell.CurrentItem = tabBar;
                tabBar.CurrentItem = target;
                SelectedRoute = route;
                return;
            }

            await shell.GoToAsync($"//MainApp/{route}", animate: false);
            SelectedRoute = route;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Tab navigation error: {ex}");
        }
        finally
        {
            _isNavigating = false;
        }
    }

    public void UpdateSelectedRoute(string? route)
    {
        if (string.IsNullOrEmpty(route) || SelectedRoute == route)
            return;

        SelectedRoute = route;
    }

    private void UpdateTabVisuals(bool animate = true)
    {
        foreach (var (route, visuals) in _tabViews)
        {
            var isSelected = route == SelectedRoute;
            
            visuals.Pill.BackgroundColor = isSelected ? Color.FromArgb("#1A1A1A") : Colors.Transparent;
            
            double targetWidth = isSelected ? 110 : 44;
            double targetIconOpacity = isSelected ? 1.0 : 0.7;
            double targetTitleOpacity = isSelected ? 1.0 : 0.0;

            if (animate)
            {
                visuals.Pill.WidthRequest = targetWidth;
                visuals.Icon.FadeTo(targetIconOpacity, 150);
                visuals.Title.FadeTo(targetTitleOpacity, 150);
            }
            else
            {
                visuals.Pill.WidthRequest = targetWidth;
                visuals.Icon.Opacity = targetIconOpacity;
                visuals.Title.Opacity = targetTitleOpacity;
            }
        }
    }
}
