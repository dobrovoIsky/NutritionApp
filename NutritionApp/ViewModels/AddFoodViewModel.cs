using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class AddFoodViewModel : INotifyPropertyChanged
    {
        private string _mealType;
        public string MealType
        {
            get => _mealType;
            set { _mealType = value; OnPropertyChanged(); }
        }

        private int? _editEntryId;
        public int? EditEntryId
        {
            get => _editEntryId;
            set { _editEntryId = value; OnPropertyChanged(); }
        }
        private readonly ApiService _apiService;
        private List<FoodDatabaseItem> _allProducts = new();
        
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }
        
        private string _calories;
        public string Calories { get => _calories; set { _calories = value; OnPropertyChanged(); } }
        
        private string _protein;
        public string Protein { get => _protein; set { _protein = value; OnPropertyChanged(); } }
        
        private string _fat;
        public string Fat { get => _fat; set { _fat = value; OnPropertyChanged(); } }
        
        private string _carbs;
        public string Carbs { get => _carbs; set { _carbs = value; OnPropertyChanged(); } }
        
        private string _weight;
        public string Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                OnPropertyChanged();
                RecalculateNutrients();
            }
        }

        private FoodDatabaseItem _selectedProduct;
        public FoodDatabaseItem SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                if (_selectedProduct != null)
                {
                    Name = _selectedProduct.Name;
                    ShowDropdown = false;
                    RecalculateNutrients();
                }
            }
        }

        private bool _showDropdown;
        public bool ShowDropdown
        {
            get => _showDropdown;
            set { _showDropdown = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FoodDatabaseItem> FilteredProducts { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private bool _isAnalyzingImage;
        public bool IsAnalyzingImage
        {
            get => _isAnalyzingImage;
            set { _isAnalyzingImage = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand AnalyzeFoodImageCommand { get; }

        public AddFoodViewModel(ApiService apiService)
        {
            _apiService = apiService;
            SaveCommand = new Command(async () => await SaveFoodAsync());
            AnalyzeFoodImageCommand = new Command(async () => await AnalyzeFoodImageAsync());
            
            // Load products in background
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            var products = await _apiService.GetFoodDatabaseItemsAsync();
            if (products != null && products.Any())
            {
                _allProducts = products;
            }
        }

        public void InitializeForEdit(FoodEntry entry)
        {
            EditEntryId = entry.Id;
            MealType = entry.MealType;
            Name = entry.Name;

            double weightMultiplier = entry.Weight > 0 ? entry.Weight / 100.0 : 1;
            _selectedProduct = new FoodDatabaseItem 
            {
                Name = entry.Name,
                CaloriesPer100g = Math.Round(entry.Calories / weightMultiplier, 1),
                ProteinPer100g = Math.Round(entry.Protein / weightMultiplier, 1),
                FatPer100g = Math.Round(entry.Fat / weightMultiplier, 1),
                CarbsPer100g = Math.Round(entry.Carbs / weightMultiplier, 1)
            };
            OnPropertyChanged(nameof(SelectedProduct));

            Calories = entry.Calories.ToString();
            Protein = entry.Protein.ToString();
            Fat = entry.Fat.ToString();
            Carbs = entry.Carbs.ToString();
            Weight = entry.Weight.ToString();
            // Don't show dropdown when editing
            ShowDropdown = false;
        }

        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(Name) || SelectedProduct?.Name == Name)
            {
                ShowDropdown = false;
                return;
            }

            var lowerQuery = Name.ToLowerInvariant();
            var matches = _allProducts.Where(p => p.Name.ToLowerInvariant().Contains(lowerQuery)).Take(10).ToList();
            
            FilteredProducts.Clear();
            foreach (var match in matches)
            {
                FilteredProducts.Add(match);
            }
            
            ShowDropdown = FilteredProducts.Any();
        }

        private void RecalculateNutrients()
        {
            if (SelectedProduct == null) return;

            if (double.TryParse(Weight, out double weight) && weight > 0)
            {
                double multiplier = weight / 100.0;
                Calories = Math.Round(SelectedProduct.CaloriesPer100g * multiplier, 1).ToString();
                Protein = Math.Round(SelectedProduct.ProteinPer100g * multiplier, 1).ToString();
                Fat = Math.Round(SelectedProduct.FatPer100g * multiplier, 1).ToString();
                Carbs = Math.Round(SelectedProduct.CarbsPer100g * multiplier, 1).ToString();
            }
            else
            {
                // Default to 100g values if weight is empty/invalid
                Calories = SelectedProduct.CaloriesPer100g.ToString();
                Protein = SelectedProduct.ProteinPer100g.ToString();
                Fat = SelectedProduct.FatPer100g.ToString();
                Carbs = SelectedProduct.CarbsPer100g.ToString();
            }
        }

        private async Task SaveFoodAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Calories))
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Введіть назву та калорії", "ОК");
                return;
            }

            if (IsLoading) return;
            IsLoading = true;

            try
            {
                int userId = Preferences.Get("UserId", 0);
                if (userId == 0) return;

                var entry = new FoodEntry
                {
                    UserId = userId,
                    Name = Name,
                    Calories = double.TryParse(Calories, out var cal) ? cal : 0,
                    Protein = double.TryParse(Protein, out var pro) ? pro : 0,
                    Fat = double.TryParse(Fat, out var f) ? f : 0,
                    Carbs = double.TryParse(Carbs, out var c) ? c : 0,
                    Weight = double.TryParse(Weight, out var w) ? w : 0,
                    MealType = this.MealType ?? string.Empty,
                    LoggedAt = DateTime.UtcNow
                };

                // Debug display alert removed for cleanliness
                FoodEntry saved;
                if (_editEntryId.HasValue)
                {
                    saved = await _apiService.UpdateFoodEntryAsync(_editEntryId.Value, entry);
                }
                else
                {
                    saved = await _apiService.LogFoodAsync(entry);
                }

                if (saved != null)
                {
                    await Shell.Current.Navigation.PopAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося зберегти дані", "ОК");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AnalyzeFoodImageAsync()
        {
            if (IsAnalyzingImage) return;

            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    // Request camera permission
                    var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.Camera>();
                    }

                    if (status != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert("Помилка", "Доступ до камери заборонено.", "ОК");
                        return;
                    }

                    var photo = await MediaPicker.Default.CapturePhotoAsync();

                    if (photo != null)
                    {
                        IsAnalyzingImage = true;
                        
                        using var stream = await photo.OpenReadAsync();
                        using var originalMs = new MemoryStream();
                        await stream.CopyToAsync(originalMs);
                        var imageBytes = originalMs.ToArray();

#if ANDROID || IOS || MACCATALYST || WINDOWS
                        try
                        {
                            using var stream2 = new MemoryStream(imageBytes);
                            Microsoft.Maui.Graphics.IImage image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream2);
                            if (image != null)
                            {
                                Microsoft.Maui.Graphics.IImage downsizedImage = image.Downsize(800, 800, true);
                                using var resizedMs = new MemoryStream();
                                downsizedImage.Save(resizedMs, Microsoft.Maui.Graphics.ImageFormat.Jpeg);
                                imageBytes = resizedMs.ToArray();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to downsize image: {ex}");
                        }
#endif

                        var result = await _apiService.AnalyzeFoodImageAsync(imageBytes);

                        if (result != null)
                        {
                            Weight = "100"; // Default weight from gemini prompt
                            SelectedProduct = result;
                            
                            // Let the user know
                            await Application.Current.MainPage.DisplayAlert("Успіх", $"Розпізнано: {result.Name}", "Клас!");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Упс", "Не вдалося розпізнати їжу на фото. Спробуйте ще раз.", "ОК");
                        }
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Ваш пристрій не підтримує зйомку фото.", "ОК");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "ОК");
            }
            finally
            {
                IsAnalyzingImage = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
