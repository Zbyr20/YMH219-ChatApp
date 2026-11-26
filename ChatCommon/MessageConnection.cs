using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatCommon
{
    /// <summary>
    /// Bir TcpClient'ı satır tabanlı mesajlaşma için sarmalar.
    /// Hem sunucu (her istemci için) hem istemci tarafında aynı sınıf kullanılır.
    /// </summary>
    public class MessageConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        private bool _disposed;

        public IPEndPoint RemoteEndPoint { get; private set; }

        public MessageConnection(TcpClient client)
        {
            if (client == null) throw new ArgumentNullException("client");
            _client = client;
            _client.NoDelay = true;

            NetworkStream stream = client.GetStream();
            var utf8 = new UTF8Encoding(false);
            _reader = new StreamReader(stream, utf8);
            _writer = new StreamWriter(stream, utf8) { AutoFlush = true, NewLine = "\n" };

            RemoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        }

        public bool IsConnected
        {
            get { return !_disposed && _client.Connected; }
        }

        /// <summary>
        /// Bir sonraki mesajı bekler. Karşı taraf bağlantıyı kapattıysa null döner.
        /// </summary>
        public async Task<ChatMessage> ReadAsync()
        {
            string line = await _reader.ReadLineAsync();
            if (line == null) return null;
            return ChatMessage.Decode(line);
        }

        /// <summary>
        /// Mesaj gönderir. Aynı anda iki görev yazmaya çalışırsa satırlar karışmasın diye
        /// SemaphoreSlim ile sıraya sokulur.
        /// </summary>
        public async Task SendAsync(ChatMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (_disposed) throw new ObjectDisposedException("MessageConnection");

            await _writeLock.WaitAsync();
            try
            {
                await _writer.WriteLineAsync(message.Encode());
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _client.Close(); }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }
    }
}
