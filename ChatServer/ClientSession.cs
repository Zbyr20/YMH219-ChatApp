using System;
using ChatCommon;

namespace ChatServer
{
    /// <summary>Sunucuya bağlı tek bir istemcinin bilgileri ve bağlantısı.</summary>
    public class ClientSession
    {
        private static int _nextId = 1;

        public int Id { get; private set; }
        public string UserName { get; set; }          // Join mesajı gelene kadar null
        public DateTime ConnectedAt { get; private set; }
        public MessageConnection Connection { get; private set; }
        public int MessageCount { get; set; }

        public ClientSession(MessageConnection connection)
        {
            Id = _nextId++;
            Connection = connection;
            ConnectedAt = DateTime.Now;
        }

        public bool HasJoined
        {
            get { return UserName != null; }
        }

        public string Address
        {
            get { return Connection.RemoteEndPoint != null ? Connection.RemoteEndPoint.ToString() : "?"; }
        }
    }
}
