using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Data;
using NutritionApp.Models;

namespace NutritionApp.ViewModels
{
    public class EquipmentSelectionViewModel : INotifyPropertyChanged
    {
        private const string FavoriteEquipmentKey = "FavoriteEquipment";
        
        private string _searchText = "";
        private string _selectedCategory = "Всі";
        private List<EquipmentItem> _allEquipment;
        
        public ObservableCollection<EquipmentItem> FilteredEquipment { get; } = new();
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
        
        public int SelectedCount => _allEquipment?.Count(e => e.IsSelected) ?? 0;
        
        public ICommand ToggleEquipmentCommand { get; }
        public ICommand AddCustomEquipmentCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand ClearAllCommand { get; }
        
        public Action<List<string>> OnEquipmentSelected { get; set; }
        
        public EquipmentSelectionViewModel()
        {
            ToggleEquipmentCommand = new Command<EquipmentItem>(ToggleEquipment);
            AddCustomEquipmentCommand = new Command(async () => await AddCustomEquipmentAsync());
            ConfirmCommand = new Command(ConfirmSelection);
            SelectCategoryCommand = new Command<string>(SelectCategory);
            ClearAllCommand = new Command(ClearAll);
        }
        
        public void Initialize(List<string> preselectedEquipment)
        {
            _allEquipment = EquipmentData.GetAllEquipment();
            
            if (Categories.Count == 0)
            {
                Categories.Add("Всі");
                foreach (var cat in EquipmentData.GetCategories())
                {
                    Categories.Add(cat);
                }
            }
            
            LoadFavoriteEquipment();
            
            if (preselectedEquipment != null && preselectedEquipment.Count > 0)
            {
                foreach (var equipment in _allEquipment)
                {
                    equipment.IsSelected = preselectedEquipment.Contains(equipment.Name);
                }
            }
            
            ApplyFilter();
            OnPropertyChanged(nameof(SelectedCount));
        }
        
        private void LoadFavoriteEquipment()
        {
            try
            {
                var favoritesJson = Preferences.Get(FavoriteEquipmentKey, "");
                if (!string.IsNullOrEmpty(favoritesJson))
                {
                    var favorites = System.Text.Json.JsonSerializer.Deserialize<List<string>>(favoritesJson);
                    if (favorites != null)
                    {
                        foreach (var equipment in _allEquipment)
                        {
                            equipment.IsSelected = favorites.Contains(equipment.Name);
                        }
                    }
                }
            }
            catch { }
        }
        
        private void SaveFavoriteEquipment()
        {
            var favorites = _allEquipment.Where(e => e.IsSelected).Select(e => e.Name).ToList();
            Task.Run(() => 
            {
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(favorites);
                    Preferences.Set(FavoriteEquipmentKey, json);
                }
                catch { }
            });
        }
        
        private void ApplyFilter()
        {
            FilteredEquipment.Clear();
            
            var filtered = _allEquipment.AsEnumerable();
            
            if (_selectedCategory != "Всі")
            {
                filtered = filtered.Where(e => e.Category == _selectedCategory);
            }
            
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var searchLower = _searchText.ToLower();
                filtered = filtered.Where(e => e.Name.ToLower().Contains(searchLower));
            }
            
            filtered = filtered.OrderByDescending(e => e.IsSelected).ThenBy(e => e.Name);
            
            foreach (var equipment in filtered)
            {
                FilteredEquipment.Add(equipment);
            }
        }
        
        private void ToggleEquipment(EquipmentItem equipment)
        {
            if (equipment == null) return;
            
            equipment.IsSelected = !equipment.IsSelected;
            OnPropertyChanged(nameof(SelectedCount));
            SaveFavoriteEquipment();
        }
        
        private async Task AddCustomEquipmentAsync()
        {
            var customName = await Shell.Current.DisplayPromptAsync(
                "Додати обладнання",
                "Введіть назву:",
                "Додати",
                "Скасувати",
                placeholder: "Наприклад: Кільця гімнастичні",
                maxLength: 50);
            
            if (!string.IsNullOrWhiteSpace(customName))
            {
                if (_allEquipment.Any(e => e.Name.ToLower() == customName.ToLower()))
                {
                    await Shell.Current.DisplayAlert("Увага", "Таке обладнання вже є!", "OK");
                    return;
                }
                
                var newEquipment = new EquipmentItem(customName.Trim(), "Інше", "✨") { IsSelected = true };
                _allEquipment.Add(newEquipment);
                ApplyFilter();
                OnPropertyChanged(nameof(SelectedCount));
                SaveFavoriteEquipment();
            }
        }
        
        private void ConfirmSelection()
        {
            var selectedEquipment = _allEquipment.Where(e => e.IsSelected).Select(e => e.Name).ToList();
            OnEquipmentSelected?.Invoke(selectedEquipment);
        }
        
        private void SelectCategory(string category)
        {
            SelectedCategory = category;
        }
        
        private void ClearAll()
        {
            foreach (var equipment in _allEquipment)
            {
                equipment.IsSelected = false;
            }
            ApplyFilter();
            OnPropertyChanged(nameof(SelectedCount));
            SaveFavoriteEquipment();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
