using FifaFootballGame.ViewModels;
using FifaFootballGame.Service;
using FifaFootballGame.Models;
using System.Diagnostics;
using System.IO;
using System.Windows;
using FifaFootballGame.MultiGamePlayer;
namespace FifaFootballGame
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ApplicationMain();
            OpenDonateSite();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //GameWindow window = new GameWindow();
            //window.Show();
        }

        private void Button_Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenDonateSite()
        {
            PythonExeRunner pythonExeRunner = new PythonExeRunner();
            pythonExeRunner.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //MultigameWindow multigameWindow = new MultigameWindow();
            //multigameWindow.Show();
        }
    }
}