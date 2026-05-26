using System.Windows;
using FifaFootballGame.Models.GamePresentWindowFolder.GameFolder;
using FifaFootballGame.Service;

namespace FifaFootballGame
{
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            InitializeComponent();

            var game = new Game
            {
                Height = 600,
                Width = 900
            };

            GameHost.Children.Add(game);
        }

        public GameWindow(GameServer server)
        {
            InitializeComponent();

            var game = new Game(server)
            {
                Height = 600,
                Width = 900
            };

            GameHost.Children.Add(game);
        }

        public GameWindow(GameClient client)
        {
            InitializeComponent();

            var game = new Game(client)
            {
                Height = 600,
                Width = 900
            };

            GameHost.Children.Add(game);
        }
    }
}