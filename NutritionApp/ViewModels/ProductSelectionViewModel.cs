using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Data;
using NutritionApp.Models;

namespace NutritionApp.ViewModels
{
    public class ProductSelectionViewModel : INotifyPropertyChanged
    {
        private const string FavoriteProductsKey = "FavoriteProducts";
        private const int PageSize = 30; // Завантажуємо по 30 елементів
        
        private string _searchText = "";
        private string _selectedCategory = "Всі";
        private List<ProductItem> _allProducts;
        private List<ProductItem> _filteredList = new();
        private int _loadedCount = 0;
        private bool _isLoadingMore = false;
        
        public ObservableCollection<ProductItem> FilteredProducts { get; } = new();
        public ObservableCollection<string> Categories { get; } = new();
        
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }
        
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }
        
        public int SelectedCount => _allProducts?.Count(p => p.IsSelected) ?? 0;
        
        public ICommand ToggleProductCommand { get; }
        public ICommand AddCustomProductCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand LoadMoreCommand { get; }
        
        public Action<List<string>> OnProductsSelected { get; set; }
        
        public ProductSelectionViewModel()
        {
            ToggleProductCommand = new Command<ProductItem>(ToggleProduct);
            AddCustomProductCommand = new Command(async () => await AddCustomProductAsync());
            ConfirmCommand = new Command(ConfirmSelection);
            SelectCategoryCommand = new Command<string>(SelectCategory);
            ClearAllCommand = new Command(ClearAll);
            LoadMoreCommand = new Command(LoadMore);
        }
        
        public void Initialize(List<string> preselectedProducts)
        {
            // Завантажуємо продукти тільки при ініціалізації
            _allProducts = ProductsData.GetAllProducts();
            
            // Категорії
            if (Categories.Count == 0)
            {
                Categories.Add("Всі");
                foreach (var cat in ProductsData.GetCategories())
                {
                    Categories.Add(cat);
                }
            }
            
            // Завантажуємо улюблені
            LoadFavoriteProducts();
            
            // Застосовуємо попередній вибір
            if (preselectedProducts != null && preselectedProducts.Count > 0)
            {
                foreach (var product in _allProducts)
                {
                    product.IsSelected = preselectedProducts.Contains(product.Name);
                }
            }
            
            ApplyFilter();
            OnPropertyChanged(nameof(SelectedCount));
        }
        
        private void LoadFavoriteProducts()
        {
            try
            {
                var favoritesJson = Preferences.Get(FavoriteProductsKey, "");
                if (!string.IsNullOrEmpty(favoritesJson))
                {
                    var favorites = System.Text.Json.JsonSerializer.Deserialize<List<string>>(favoritesJson);
                    if (favorites != null)
                    {
                        foreach (var product in _allProducts)
                        {
                            product.IsSelected = favorites.Contains(product.Name);
                        }
                    }
                }
            }
            catch { }
        }
        
        private void SaveFavoriteProducts()
        {
            var favorites = _allProducts.Where(p => p.IsSelected).Select(p => p.Name).ToList();
            Task.Run(() => 
            {
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(favorites);
                    Preferences.Set(FavoriteProductsKey, json);
                }
                catch { }
            });
        }
        
        private void ApplyFilter()
        {
            var filtered = _allProducts.AsEnumerable();
            
            if (_selectedCategory != "Всі")
            {
                filtered = filtered.Where(p => p.Category == _selectedCategory);
            }
            
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var searchLower = _searchText.ToLower();
                filtered = filtered.Where(p => p.Name.ToLower().Contains(searchLower));
            }
            
            // Вибрані першими
            _filteredList = filtered.OrderByDescending(p => p.IsSelected).ThenBy(p => p.Name).ToList();
            
            // Скидаємо та завантажуємо першу порцію
            _loadedCount = 0;
            FilteredProducts.Clear();
            LoadMore();
        }
        
        private void LoadMore()
        {
            if (_isLoadingMore || _loadedCount >= _filteredList.Count) return;
            
            _isLoadingMore = true;
            
            var toLoad = _filteredList.Skip(_loadedCount).Take(PageSize);
            foreach (var product in toLoad)
            {
                FilteredProducts.Add(product);
            }
            _loadedCount += PageSize;
            
            _isLoadingMore = false;
        }
        
        private void ToggleProduct(ProductItem product)
        {
            if (product == null) return;
            
            product.IsSelected = !product.IsSelected;
            OnPropertyChanged(nameof(SelectedCount));
            SaveFavoriteProducts();
        }
        
        private async Task AddCustomProductAsync()
        {
            var customName = await Application.Current.MainPage.DisplayPromptAsync(
                "Додати продукт",
                "Введіть назву продукту:",
                "Додати",
                "Скасувати",
                placeholder: "Наприклад: Тофу",
                maxLength: 50);
            
            if (!string.IsNullOrWhiteSpace(customName))
            {
                if (_allProducts.Any(p => p.Name.ToLower() == customName.ToLower()))
                {
                    await Application.Current.MainPage.DisplayAlert("Увага", "Такий продукт вже є!", "OK");
                    return;
                }
                
                var newProduct = new ProductItem(customName.Trim(), "Інше", "✨") { IsSelected = true };
                _allProducts.Add(newProduct);
                ApplyFilter();
                OnPropertyChanged(nameof(SelectedCount));
                SaveFavoriteProducts();
            }
        }
        
        private void ConfirmSelection()
        {
            var selectedProducts = _allProducts.Where(p => p.IsSelected).Select(p => p.Name).ToList();
            OnProductsSelected?.Invoke(selectedProducts);
        }
        
        private void SelectCategory(string category)
        {
            SelectedCategory = category;
        }
        
        private void ClearAll()
        {
            foreach (var product in _allProducts)
            {
                product.IsSelected = false;
            }
            ApplyFilter();
            OnPropertyChanged(nameof(SelectedCount));
            SaveFavoriteProducts();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
