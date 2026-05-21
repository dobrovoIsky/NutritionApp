using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutritionApp.Models
{
    public class ProductItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        
        public string Name { get; set; }
        public string Category { get; set; }
        public string Emoji { get; set; }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public ProductItem(string name, string category, string emoji = "🍽️")
        {
            Name = name;
            Category = category;
            Emoji = emoji;
            IsSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
