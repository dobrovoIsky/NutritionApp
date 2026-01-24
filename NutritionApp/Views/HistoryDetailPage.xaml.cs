namespace NutritionApp.Views;

[QueryProperty(nameof(PlanText), "PlanText")]
public partial class HistoryDetailPage : ContentPage
{
    private string _planText;
    public string PlanText
    {
        get => _planText;
        set
        {
            _planText = value;
            PlanTextLabel.Text = _planText; // Встановлюємо текст при отриманні
        }
    }

    public HistoryDetailPage()
    {
        InitializeComponent();
    }
}