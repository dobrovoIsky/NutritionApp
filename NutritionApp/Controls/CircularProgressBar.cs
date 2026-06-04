using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace NutritionApp.Controls
{
    public class CircularProgressBar : GraphicsView
    {
        public static readonly BindableProperty ProgressProperty =
            BindableProperty.Create(nameof(Progress), typeof(double), typeof(CircularProgressBar), 0.0, propertyChanged: OnPropertyChanged);

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public static readonly BindableProperty TrackColorProperty =
            BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(CircularProgressBar), Colors.LightGray, propertyChanged: OnPropertyChanged);

        public Color TrackColor
        {
            get => (Color)GetValue(TrackColorProperty);
            set => SetValue(TrackColorProperty, value);
        }

        public static readonly BindableProperty ProgressColorProperty =
            BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(CircularProgressBar), Colors.Blue, propertyChanged: OnPropertyChanged);

        public Color ProgressColor
        {
            get => (Color)GetValue(ProgressColorProperty);
            set => SetValue(ProgressColorProperty, value);
        }

        public static readonly BindableProperty ThicknessProperty =
            BindableProperty.Create(nameof(Thickness), typeof(double), typeof(CircularProgressBar), 8.0, propertyChanged: OnPropertyChanged);

        public double Thickness
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CircularProgressBar bar)
            {
                bar.Invalidate();
            }
        }

        public CircularProgressBar()
        {
            Drawable = new CircularProgressBarDrawable(this);
            BackgroundColor = Colors.Transparent;
        }

        private class CircularProgressBarDrawable : IDrawable
        {
            private readonly CircularProgressBar _bar;

            public CircularProgressBarDrawable(CircularProgressBar bar)
            {
                _bar = bar;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var thickness = (float)_bar.Thickness;
                var center = new PointF(dirtyRect.Center.X, dirtyRect.Center.Y);
                var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2 - thickness / 2;

                // Draw track
                canvas.StrokeColor = _bar.TrackColor;
                canvas.StrokeSize = thickness;
                canvas.DrawCircle(center, radius);

                // Draw progress
                var clampedProgress = Math.Max(0, Math.Min(1, _bar.Progress));
                if (clampedProgress > 0)
                {
                    var colorToUse = _bar.ProgressColor;
                    
                    if (_bar.Progress > 1.15)
                    {
                        colorToUse = Color.FromArgb("#FF3B30"); // Red
                    }
                    else if (_bar.Progress > 1.0)
                    {
                        colorToUse = Color.FromArgb("#FF9500"); // Orange
                    }

                    canvas.StrokeColor = colorToUse;
                    canvas.StrokeSize = thickness;
                    canvas.StrokeLineCap = LineCap.Round;
                    
                    var endAngle = 90 - (clampedProgress * 360);
                    // DrawArc takes bounding box
                    var rect = new RectF(center.X - radius, center.Y - radius, radius * 2, radius * 2);
                    canvas.DrawArc(rect, 90, (float)endAngle, true, false);
                }
            }
        }
    }
}
