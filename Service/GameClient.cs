using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace FifaFootballGame.Service
{
    public class GameClient : NetworkSystem
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;

        public GameStatePacket LastState { get; private set; }

        public async Task ConnectAsync(string ip, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);

            _stream = _client.GetStream();

            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8)
            {
                AutoFlush = true
            };

            _ = Task.Run(ReadLoop);
        }

        private async Task ReadLoop()
        {
            while (_client != null && _client.Connected)
            {
                string json = await _reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                try
                {
                    var state = JsonSerializer.Deserialize<GameStatePacket>(json);

                    if (state != null)
                        LastState = state;
                }
                catch { }
            }
        }

        public async Task SendInputAsync(PlayerInputPacket input)
        {
            if (_writer == null || _client == null || !_client.Connected)
                return;

            string json = JsonSerializer.Serialize(input);
            await _writer.WriteLineAsync(json);
        }
    }
}