using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MonoGame.Framework.WpfInterop.Input;

namespace FifaFootballGame.Service
{
    public class GameClient : NetworkSystem
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public async Task ConnectAsync(string ip, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);
            _stream = _client.GetStream();
        }

        public async Task SendInputAsync(PlayerInputPacket input)
        {
            if (_client == null || !_client.Connected)
                return;

            string json = JsonSerializer.Serialize(input) + "\n";
            byte[] data = Encoding.UTF8.GetBytes(json);

            await _stream.WriteAsync(data, 0, data.Length);
        }
    }
}