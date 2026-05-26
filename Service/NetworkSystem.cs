using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace FifaFootballGame.Service
{
    public class NetworkSystem : INotifyPropertyChanged
    {
        protected int _port;
        protected DateTime _timeout; //время ожидания запроса, чтобы если врямя вышло, то выводило ссобщение
        protected IPAddress _iPAddress;
        protected IPEndPoint _iPEndPoint;

        public int Port
        { 
            get => _port; 
            set 
            { 
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        public IPAddress IP
        {
            get => _iPAddress;
            set
            {
                _iPAddress = value;
                OnPropertyChanged("IP");
            }
        }
        public NetworkSystem()
        {
        }
        //метод для получения конечной точки подключения
        public IPEndPoint GetEndpoint()
        {
            return new IPEndPoint(_iPAddress, _port);
        }


        //реализация интерфейса

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
