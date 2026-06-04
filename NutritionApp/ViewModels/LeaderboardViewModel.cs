using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class LeaderboardItem
    {
        public LeaderboardUserDto User { get; set; }
        public int Rank { get; set; }
        public string AvatarChar => !string.IsNullOrEmpty(User?.Username) ? User.Username.Substring(0, 1).ToUpper() : "?";
        public bool HasAvatar => !string.IsNullOrEmpty(User?.AvatarBase64);
    }

    public class LeaderboardViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;

        public ObservableCollection<LeaderboardItem> LeaderboardUsers { get; set; } = new ObservableCollection<LeaderboardItem>();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadLeaderboardCommand { get; }

        public LeaderboardViewModel()
        {
            _apiService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<ApiService>();
            LoadLeaderboardCommand = new Command(async () => await LoadLeaderboardAsync());
        }

        private async Task LoadLeaderboardAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                var users = await _apiService.GetLeaderboardAsync();
                LeaderboardUsers.Clear();

                int rank = 1;
                foreach (var u in users)
                {
                    LeaderboardUsers.Add(new LeaderboardItem { User = u, Rank = rank });
                    rank++;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
