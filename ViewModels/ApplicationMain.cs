using FifaFootballGame.MultiGamePlayer;
using FifaFootballGame.Service;
using System.Diagnostics;
using System.Windows;

namespace FifaFootballGame.ViewModels
{
    public class ApplicationMain
    {
        
        public ApplicationMain() 
        {
            
        }
        //вход в одиночную игру
        private AppCommands _buttonPlay;
        public AppCommands ButtonPlay
        {
            get
            {
                return _buttonPlay ?? new AppCommands(obj =>
                {
                    GameWindow game = new GameWindow();
                    game.Show();
                });
            }
        }
        //команда настроек (поддрежка логики многопользовательской игры)
        private AppCommands _buttonSettings;
        public AppCommands ButtonSettings
        {
            get
            {
                return _buttonSettings ?? new AppCommands(obj =>
                {
                    MessageBox.Show("Настройки");
                });
            }
        }
        //команда регистрации пользователя
        private AppCommands _buttonRegister;
        public AppCommands ButtonRegister
        {
            get
            {
                return _buttonRegister ?? new AppCommands(obj =>
                {
                    MultigameWindow multigameWindow = new MultigameWindow();
                    multigameWindow.Show(); 
                });
            }
        }

        private AppCommands _buttonDonate;
        public AppCommands ButtonDonate
        {
            get
            {
                return _buttonDonate ?? new AppCommands(obj =>
                {
                    OpenDonationSite();
                });
            }
        }

        private void OpenDonationSite()
        {
            try
            {
                // Путь к вашему HTML файлу (локальный)
                string localSitePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DonationSite", "index.html");

                // Или URL вашего хостинга (если зальёте куда-то)
                string donationUrl = "https://your-donation-site.com"; // Замените на реальный URL

                // Открываем в браузере по умолчанию
                Process.Start(new ProcessStartInfo
                {
                    FileName = localSitePath,
                    UseShellExecute = true // Важно для открытия в браузере
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть сайт: {ex.Message}", "Ошибка");
            }
        }
    }
}
