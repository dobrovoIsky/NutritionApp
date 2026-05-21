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
                var progress = Math.Max(0, Math.Min(1, _bar.Progress));
                if (progress > 0)
                {
                    var fillWidth = (float)(width * progress);
                    // Ensure minimum width equals the height so corners look correct
                    fillWidth = Math.Max(fillWidth, height);
                    fillWidth = Math.Min(fillWidth, width);

                    canvas.FillColor = _bar.ProgressColor;
                    canvas.FillRoundedRectangle(0, 0, fillWidth, height, cornerRadius);
                }
            }
        }
    }
}
