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
        
        // Custom product properties
        private bool _isAddingCustomProduct;
        public bool IsAddingCustomProduct
        {
            get => _isAddingCustomProduct;
            set
            {
                _isAddingCustomProduct = value;
                OnPropertyChanged();
            }
        }
        
        private string _newProductName;
        public string NewProductName
        {
            get => _newProductName;
            set
            {
                _newProductName = value;
                OnPropertyChanged();
            }
        }
        
        private string _newProductCategory;
        public string NewProductCategory
        {
            get => _newProductCategory;
            set
            {
                _newProductCategory = value;
                OnPropertyChanged();
            }
        }

        public List<string> CategoriesWithoutAll => Categories.Where(c => c != "Всі").ToList();

        public ICommand ToggleProductCommand { get; }
        public ICommand ShowAddCustomProductCommand { get; }
        public ICommand CancelCustomProductCommand { get; }
        public ICommand SaveCustomProductCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand LoadMoreCommand { get; }
        
        private const string CustomProductsKey = "CustomProducts";
        
        public Action<List<string>> OnProductsSelected { get; set; }
        
        public ProductSelectionViewModel()
        {
            ToggleProductCommand = new Command<ProductItem>(ToggleProduct);
            ShowAddCustomProductCommand = new Command(ShowAddCustomProduct);
            CancelCustomProductCommand = new Command(CancelCustomProduct);
            SaveCustomProductCommand = new Command(SaveCustomProduct);
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
            
            // Завантажуємо власні продукти
            LoadCustomProducts();
            
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
            OnPropertyChanged(nameof(CategoriesWithoutAll));
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
        
        private void ShowAddCustomProduct()
        {
            NewProductName = string.Empty;
            NewProductCategory = CategoriesWithoutAll.FirstOrDefault() ?? "Інше";
            IsAddingCustomProduct = true;
        }

        private void CancelCustomProduct()
        {
            IsAddingCustomProduct = false;
        }

        private void SaveCustomProduct()
        {
            if (string.IsNullOrWhiteSpace(NewProductName))
            {
                Application.Current.MainPage.DisplayAlert("Увага", "Введіть назву продукту", "OK");
                return;
            }

            var customName = NewProductName.Trim();
            if (_allProducts.Any(p => p.Name.ToLower() == customName.ToLower()))
            {
                Application.Current.MainPage.DisplayAlert("Увага", "Такий продукт вже є!", "OK");
                return;
            }

            var category = string.IsNullOrWhiteSpace(NewProductCategory) ? "Інше" : NewProductCategory;
            
            // Generate a simple emoji based on category or default
            var emoji = "✨";
            if (category.Contains("Овочі")) emoji = "🥦";
            else if (category.Contains("Фрукти")) emoji = "🍎";
            else if (category.Contains("М'ясо")) emoji = "🥩";
            else if (category.Contains("Молочні")) emoji = "🧀";

            var newProduct = new ProductItem(customName, category, emoji) { IsSelected = true };
            _allProducts.Add(newProduct);
            
            // Save to preferences
            var customProductsList = _allProducts.Where(p => p.Emoji == "✨" || p.Emoji == "🥦" || p.Emoji == "🍎" || p.Emoji == "🥩" || p.Emoji == "🧀")
                                                 .Select(p => new { Name = p.Name, Category = p.Category, Emoji = p.Emoji })
                                                 .ToList();
                                                 
            // Actually it's better to maintain a separate list of CustomProducts to save
            SaveCustomProductsToPrefs();
            
            IsAddingCustomProduct = false;
            ApplyFilter();
            OnPropertyChanged(nameof(SelectedCount));
            SaveFavoriteProducts();
        }

        private void SaveCustomProductsToPrefs()
        {
            try
            {
                // We'll identify custom products by checking if they are not in the default ProductsData list
                var defaultNames = ProductsData.GetAllProducts().Select(p => p.Name).ToHashSet();
                var customProducts = _allProducts.Where(p => !defaultNames.Contains(p.Name))
                                                 .Select(p => new { Name = p.Name, Category = p.Category, Emoji = p.Emoji })
                                                 .ToList();
                
                var json = System.Text.Json.JsonSerializer.Serialize(customProducts);
                Preferences.Set(CustomProductsKey, json);
            }
            catch { }
        }

        private void LoadCustomProducts()
        {
            try
            {
                var customJson = Preferences.Get(CustomProductsKey, "");
                if (!string.IsNullOrEmpty(customJson))
                {
                    var customProducts = System.Text.Json.JsonSerializer.Deserialize<List<ProductItem>>(customJson);
                    if (customProducts != null)
                    {
                        foreach (var cp in customProducts)
                        {
                            if (!_allProducts.Any(p => p.Name == cp.Name))
                            {
                                _allProducts.Add(cp);
                            }
                        }
                    }
                }
            }
            catch { }
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
