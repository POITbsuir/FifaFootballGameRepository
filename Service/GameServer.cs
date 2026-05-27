using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace FifaFootballGame.Service
{
    public class GameServer : NetworkSystem
    {
        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;

        public PlayerInputPacket LastInput { get; private set; } = new PlayerInputPacket();

        public bool IsClientConnected => _client != null && _client.Connected;
        public bool ConnectionMessageShown { get; set; }

        public Task StartServerAsync(int port)
        {
            Port = port;
            IP = IPAddress.Any;

            _listener = new TcpListener(IP, Port);
            _listener.Start();

            _ = Task.Run(AcceptLoop);

            return Task.CompletedTask;
        }
        //слушатель
        private async Task AcceptLoop()
        {
            _client = await _listener.AcceptTcpClientAsync();
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
                    var input = JsonSerializer.Deserialize<PlayerInputPacket>(json);

                    if (input != null)
                        LastInput = input;
                }
                catch { }
            }
        }

        public async Task SendStateAsync(GameStatePacket state)
        {
            if (_writer == null || !IsClientConnected)
                return;

            string json = JsonSerializer.Serialize(state);
            await _writer.WriteLineAsync(json);
        }
    }
}