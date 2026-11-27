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
        private readonly int _maxLineLength;
        private bool _disposed;

        // Okuma tamponu: ReadLineAsync yerine satırı kendimiz topluyoruz ki uzunluğu sınırlayabilelim.
        private readonly char[] _readBuffer = new char[4096];
        private int _readPos;
        private int _readLen;
        private readonly StringBuilder _line = new StringBuilder();

        public IPEndPoint RemoteEndPoint { get; private set; }

        public MessageConnection(TcpClient client)
            : this(client, ChatProtocol.MaxLineLength)
        {
        }

        /// <param name="maxLineLength">Okunabilecek en uzun satır (karakter). Aşılırsa ReadAsync FormatException fırlatır.</param>
        public MessageConnection(TcpClient client, int maxLineLength)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (maxLineLength <= 0) throw new ArgumentOutOfRangeException("maxLineLength");
            _client = client;
            _maxLineLength = maxLineLength;
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
        /// Satır çok uzunsa ya da biçimi bozuksa FormatException fırlatır.
        /// </summary>
        public async Task<ChatMessage> ReadAsync()
        {
            string line = await ReadLineAsync();
            if (line == null) return null;
            return ChatMessage.Decode(line);
        }

        /// <summary>
        /// StreamReader.ReadLineAsync gibi bir satır okur, ama en fazla _maxLineLength karakter.
        /// (ReadLineAsync satır sonu gelene kadar her şeyi belleğe alır; kötü niyetli bir istemci
        /// hiç '\n' göndermeden sunucunun belleğini doldurabilirdi.)
        /// </summary>
        private async Task<string> ReadLineAsync()
        {
            _line.Clear();
            while (true)
            {
                if (_readPos == _readLen)
                {
                    _readLen = await _reader.ReadAsync(_readBuffer, 0, _readBuffer.Length);
                    _readPos = 0;
                    if (_readLen == 0)   // akış bitti
                        return _line.Length > 0 ? TrimCarriageReturn(_line.ToString()) : null;
                }

                int newline = Array.IndexOf(_readBuffer, '\n', _readPos, _readLen - _readPos);
                int end = newline >= 0 ? newline : _readLen;
                int count = end - _readPos;

                if (_line.Length + count > _maxLineLength)
                    throw new FormatException("Mesaj satırı çok uzun (en fazla " + _maxLineLength + " karakter).");

                _line.Append(_readBuffer, _readPos, count);
                _readPos = newline >= 0 ? newline + 1 : _readLen;

                if (newline >= 0)
                    return TrimCarriageReturn(_line.ToString());
            }
        }

        private static string TrimCarriageReturn(string line)
        {
            return line.Length > 0 && line[line.Length - 1] == '\r' ? line.Substring(0, line.Length - 1) : line;
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
