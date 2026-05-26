using FifaFootballGame.Service;
using System.Windows;
using System.Windows.Input;

namespace FifaFootballGame.MultiGamePlayer
{
    public partial class MultigameWindow : Window
    {
        public MultigameWindow()
        {
            InitializeComponent();
        }

        private async void CreateServerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int port = int.Parse(PortTextBox.Text);

                GameServer server = new GameServer();
                await server.StartServerAsync(port);

                GameWindow gameWindow = new GameWindow(server);
                gameWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch 
            {
                throw new Exception();
            }
            finally
            {
                Close();
            }

        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IpTextBox.Text == "" || PortTextBox.Text == "")
                    MessageBox.Show("Поля не должны быть пустыми...");
                string ip = IpTextBox.Text;
                int port = int.Parse(PortTextBox.Text);

                GameClient client = new GameClient();
                await client.ConnectAsync(ip, port);

                GameWindow gameWindow = new GameWindow(client);
                gameWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch
            {
                throw new Exception();
            }
            finally
            { 
                Close(); 
            }
        }
        private void IpTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9.]$");
        }

        private void PortTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]$");
        }
    }
}