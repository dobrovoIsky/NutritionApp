using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutritionApp.Models
{
    public class MealPlan : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Plan { get; set; }
        public DateTime Date { get; set; }
        
        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}