using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace NutritionApp.Controls
{
    public class RoundedProgressBar : GraphicsView
    {
        public static readonly BindableProperty ProgressProperty =
            BindableProperty.Create(nameof(Progress), typeof(double), typeof(RoundedProgressBar), 0.0, propertyChanged: OnPropertyChanged);

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public static readonly BindableProperty TrackColorProperty =
            BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(RoundedProgressBar), Color.FromArgb("#1A1A1A"), propertyChanged: OnPropertyChanged);

        public Color TrackColor
        {
            get => (Color)GetValue(TrackColorProperty);
            set => SetValue(TrackColorProperty, value);
        }

        public static readonly BindableProperty ProgressColorProperty =
            BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(RoundedProgressBar), Color.FromArgb("#CCFF00"), propertyChanged: OnPropertyChanged);

        public Color ProgressColor
        {
            get => (Color)GetValue(ProgressColorProperty);
            set => SetValue(ProgressColorProperty, value);
        }

        private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RoundedProgressBar bar)
            {
                bar.Invalidate();
            }
        }

        public RoundedProgressBar()
        {
            Drawable = new RoundedProgressBarDrawable(this);
            BackgroundColor = Colors.Transparent;
        }

        private class RoundedProgressBarDrawable : IDrawable
        {
            private readonly RoundedProgressBar _bar;

            public RoundedProgressBarDrawable(RoundedProgressBar bar)
            {
                _bar = bar;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var height = dirtyRect.Height;
                var width = dirtyRect.Width;
                var cornerRadius = height / 2;

                // Draw track
                canvas.FillColor = _bar.TrackColor;
                canvas.FillRoundedRectangle(0, 0, width, height, cornerRadius);

                // Draw progress fill
                var fillWidth = (float)(width * _bar.Progress);
                // Clamp width to bounds
                fillWidth = Math.Max(fillWidth, height); // Minimum width is height so corners look round
                fillWidth = Math.Min(fillWidth, width);  // Max width is container width

                if (fillWidth > 0)
                {
                    // Default color
                    var colorToUse = _bar.ProgressColor;
                    
                    // Warning/Danger colors for overages
                    if (_bar.Progress > 1.15)
                    {
                        colorToUse = Color.FromArgb("#FF3B30"); // Red
                    }
                    else if (_bar.Progress > 1.0)
                    {
                        colorToUse = Color.FromArgb("#FF9500"); // Orange
                    }

                    canvas.FillColor = colorToUse;
                    canvas.FillRoundedRectangle(0, 0, fillWidth, height, cornerRadius);
                }
            }
        }
    }
}
