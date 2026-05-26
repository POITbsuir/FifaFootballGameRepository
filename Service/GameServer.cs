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

        public PlayerInputPacket LastInput { get; private set; } = new PlayerInputPacket();

        public async Task StartServerAsync(int port)
        {
            Port = port;
            IP = IPAddress.Any;

            _listener = new TcpListener(IP, Port);
            _listener.Start();

            _client = await _listener.AcceptTcpClientAsync();
            _stream = _client.GetStream();

            _ = Task.Run(ReadLoop);
        }

        private async Task ReadLoop()
        {
            using var reader = new StreamReader(_stream, Encoding.UTF8);

            while (_client != null && _client.Connected)
            {
                string? json = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                try
                {
                    var input = JsonSerializer.Deserialize<PlayerInputPacket>(json);

                    if (input != null)
                        LastInput = input;
                }
                catch
                {
                    // битый пакет игнорируем
                }
            }
        }
    }
}