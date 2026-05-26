using FifaFootballGame.ViewModels;
using System.Windows;
namespace FifaFootballGame
{
    public partial class PresentWindow : Window
    {
        public PresentWindow()
        {
            InitializeComponent();
            DataContext = new ApplicationPresent();
        }
    }
}
