using Plugin.LocalNotification;
using System.Diagnostics;

namespace NutritionApp.Services
{
    public class WaterReminderService
    {
        private const string REMINDER_ENABLED_KEY = "WaterReminderEnabled";
        private const string REMINDER_INTERVAL_KEY = "WaterReminderInterval";
        private const int BASE_NOTIFICATION_ID = 1000;

        // ====== ТЕСТОВИЙ РЕЖИМ ======
        // Встанови true для тесту (сповіщення кожну хвилину)
        // Встанови false для продакшену (використовує вибраний інтервал)
        private const bool TEST_MODE = false;  // <-- ВИМКНЕНО
        private const int TEST_INTERVAL_MINUTES = 1;
        // ============================

        public bool IsEnabled
        {
            get => Preferences.Get(REMINDER_ENABLED_KEY, false);
            set => Preferences.Set(REMINDER_ENABLED_KEY, value);
        }

        public int IntervalMinutes
        {
            get => Preferences.Get(REMINDER_INTERVAL_KEY, 60);
            set => Preferences.Set(REMINDER_INTERVAL_KEY, value);
        }

        // Фактичний інтервал (тестовий або реальний)
        private int ActualInterval => TEST_MODE ? TEST_INTERVAL_MINUTES : IntervalMinutes;

        public async Task<bool> RequestPermissionAsync()
        {
            try
            {
                var result = await LocalNotificationCenter.Current.RequestNotificationPermission();
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RequestPermission error: {ex.Message}");
                return true;
            }
        }

        public void StartRemindersInBackground()
        {
            if (!IsEnabled)
                return;

            Task.Run(async () =>
            {
                try
                {
                    await ScheduleNotificationsAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"WaterReminder: Error scheduling - {ex.Message}");
                }
            });
        }

        private async Task ScheduleNotificationsAsync()
        {
            LocalNotificationCenter.Current.CancelAll();

            var now = DateTime.Now;

            // Сповіщення з 8:00 до 22:00
            var startHour = 8;
            var endHour = 22;

            var notificationId = BASE_NOTIFICATION_ID;
            var messages = new[]
            {
                "Час випити склянку води!",
                "Не забудь про воду!",
                "Пий воду регулярно!",
                "Склянка води = енергія!",
                "Гідратація - це важливо!",
                "Зроби перерву, випий води!",
                "Вода - джерело життя!"
            };

            var random = new Random();
            var notificationsToSchedule = new List<NotificationRequest>();

            // Плануємо на 2 дні вперед
            for (int day = 0; day < 2; day++)
            {
                var dayStart = now.Date.AddDays(day).AddHours(startHour);
                var dayEnd = now.Date.AddDays(day).AddHours(endHour);

                var notificationTime = dayStart;

                // Якщо сьогодні - починаємо з наступного інтервалу
                if (day == 0 && now.Hour >= startHour && now.Hour < endHour)
                {
                    var minutesSinceStart = (now.Hour - startHour) * 60 + now.Minute;
                    var nextInterval = ((minutesSinceStart / ActualInterval) + 1) * ActualInterval;
                    notificationTime = now.Date.AddHours(startHour).AddMinutes(nextInterval);
                }
                else if (day == 0 && now.Hour >= endHour)
                {
                    continue; // Пропускаємо сьогодні
                }

                while (notificationTime < dayEnd && notificationTime > now)
                {
                    var message = messages[random.Next(messages.Length)];

                    notificationsToSchedule.Add(new NotificationRequest
                    {
                        NotificationId = notificationId++,
                        Title = "Нагадування про воду",
                        Description = message,
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = notificationTime
                        }
                    });

                    Debug.WriteLine($"WaterReminder: Prepared for {notificationTime:HH:mm dd.MM}");
                    notificationTime = notificationTime.AddMinutes(ActualInterval);
                }
            }

            foreach (var notification in notificationsToSchedule)
            {
                await LocalNotificationCenter.Current.Show(notification);
                await Task.Delay(30);
            }

            Debug.WriteLine($"WaterReminder: Scheduled {notificationsToSchedule.Count} notifications (interval: {ActualInterval} min)");
        }

        public void StopReminders()
        {
            LocalNotificationCenter.Current.CancelAll();
            Debug.WriteLine("WaterReminder: Stopped");
        }

        public void RestartRemindersInBackground()
        {
            StopReminders();
            if (IsEnabled)
            {
                StartRemindersInBackground();
            }
        }
    }
}