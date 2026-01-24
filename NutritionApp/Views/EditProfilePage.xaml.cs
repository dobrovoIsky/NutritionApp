using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class EditProfilePage : ContentPage
{
    public EditProfilePage(EditProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}