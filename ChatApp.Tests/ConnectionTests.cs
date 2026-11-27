using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatCommon;

namespace ChatApp.Tests
{
    /// <summary>MessageConnection: gerçek TCP soketleriyle (127.0.0.1) okuma/yazma.</summary>
    public static class ConnectionTests
    {
        /// <summary>Birbirine bağlı iki TcpClient (a = istemci tarafı, b = sunucu tarafı).</summary>
        public static async Task<Tuple<TcpClient, TcpClient>> CreateTcpPairAsync()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                var a = new TcpClient();
                Task<TcpClient> accept = listener.AcceptTcpClientAsync();
                await a.ConnectAsync(IPAddress.Loopback, port);
                TcpClient b = await accept;
                return Tuple.Create(a, b);
            }
            finally
            {
                listener.Stop();
            }
        }

        public static async Task SendAndReceive()
        {
            var pair = await CreateTcpPairAsync();
            using (var a = new MessageConnection(pair.Item1))
            using (var b = new MessageConnection(pair.Item2))
            {
                await a.SendAsync(new ChatMessage(MessageType.Text, "ali", "", "çok satırlı\nmesaj | ayraçlı"));
                await a.SendAsync(new ChatMessage(MessageType.Leave, "ali", "", ""));

                ChatMessage m1 = await b.ReadAsync();
                ChatMessage m2 = await b.ReadAsync();
                Assert.Equal("çok satırlı\nmesaj | ayraçlı", m1.Text, "ilk mesaj");
                Assert.Equal(MessageType.Leave, m2.Type, "ikinci mesaj");
                Assert.True(b.RemoteEndPoint != null, "RemoteEndPoint dolu olmalı");
            }
        }

        public static async Task ReadReturnsNullWhenPeerCloses()
        {
            var pair = await CreateTcpPairAsync();
            var a = new MessageConnection(pair.Item1);
            using (var b = new MessageConnection(pair.Item2))
            {
                a.Dispose();
                Assert.Equal(null, await b.ReadAsync(), "karşı taraf kapattıysa null");
            }
        }

        public static async Task ConcurrentSendsDoNotInterleave()
        {
            var pair = await CreateTcpPairAsync();
            using (var a = new MessageConnection(pair.Item1))
            using (var b = new MessageConnection(pair.Item2))
            {
                const int count = 200;
                string payload = new string('x', 3000);   // tek yazmada bitmeyecek kadar uzun
                var sends = new List<Task>();
                for (int i = 0; i < count; i++)
                {
                    string text = i + ":" + payload;
                    sends.Add(Task.Run(() => a.SendAsync(new ChatMessage(MessageType.Text, "ali", "", text))));
                }

                var seen = new HashSet<int>();
                for (int i = 0; i < count; i++)
                {
                    ChatMessage m = await b.ReadAsync();
                    string[] parts = m.Text.Split(':');
                    Assert.Equal(payload, parts[1], "mesaj " + parts[0] + " bozulmadan gelmeli");
                    seen.Add(int.Parse(parts[0]));
                }
                await Task.WhenAll(sends);
                Assert.Equal(count, seen.Count, "her mesaj bir kez");
            }
        }

        /// <summary>Raw TCP üzerinden protokol dışı veri yazar (kötü niyetli istemci).</summary>
        private static Task WriteRawAsync(TcpClient client, string text)
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(text);
            return client.GetStream().WriteAsync(bytes, 0, bytes.Length);
        }

        public static async Task AcceptsCrLfLineEndings()
        {
            var pair = await CreateTcpPairAsync();
            using (pair.Item1)
            using (var b = new MessageConnection(pair.Item2))
            {
                await WriteRawAsync(pair.Item1, "Text|ali||0|merhaba\r\nText|ali||0|ikinci\n");
                Assert.Equal("merhaba", (await b.ReadAsync()).Text, "\\r\\n ile biten satır");
                Assert.Equal("ikinci", (await b.ReadAsync()).Text, "\\n ile biten satır");
            }
        }

        public static async Task MultiByteCharactersSplitAcrossPackets()
        {
            var pair = await CreateTcpPairAsync();
            using (pair.Item1)
            using (var b = new MessageConnection(pair.Item2))
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes("Text|ali||0|ğüşİöç🚀\n");
                Task<ChatMessage> read = b.ReadAsync();
                NetworkStream s = pair.Item1.GetStream();
                foreach (byte x in bytes)   // her baytı ayrı paket olarak yolla
                {
                    await s.WriteAsync(new[] { x }, 0, 1);
                    await s.FlushAsync();
                }
                Assert.Equal("ğüşİöç🚀", (await read).Text, "UTF-8 karakterleri paketlere bölünse de doğru çözülmeli");
            }
        }

        public static async Task TooLongLineIsRejected()
        {
            // Hata öncesi: StreamReader.ReadLineAsync satır sonu gelene kadar her şeyi belleğe
            // alıyordu; bu test sonsuza kadar bekliyordu (zaman aşımı).
            var pair = await CreateTcpPairAsync();
            using (pair.Item1)
            using (var b = new MessageConnection(pair.Item2, 1000))
            {
                Task<ChatMessage> read = b.ReadAsync();
                await WriteRawAsync(pair.Item1, "Text|ali||0|" + new string('a', 5000));   // satır sonu YOK, bağlantı açık
                var ex = await Assert.ThrowsAsync<FormatException>(() => read, "çok uzun satır");
                Assert.True(ex.Message.Contains("1000"), "hata mesajı sınırı söylemeli: " + ex.Message);
            }
        }

        public static async Task LineExactlyAtLimitIsAccepted()
        {
            var pair = await CreateTcpPairAsync();
            using (pair.Item1)
            using (var b = new MessageConnection(pair.Item2, 1000))
            {
                string prefix = "Text|ali||0|";
                string text = new string('a', 1000 - prefix.Length);
                await WriteRawAsync(pair.Item1, prefix + text + "\n" + prefix + "sonraki\n");
                Assert.Equal(text, (await b.ReadAsync()).Text, "tam 1000 karakterlik satır");
                Assert.Equal("sonraki", (await b.ReadAsync()).Text, "tampondaki sonraki satır kaybolmamalı");
            }
        }

        public static async Task DefaultLimitIsProtocolLimit()
        {
            var pair = await CreateTcpPairAsync();
            using (pair.Item1)
            using (var b = new MessageConnection(pair.Item2))
            {
                Task<ChatMessage> read = b.ReadAsync();
                await WriteRawAsync(pair.Item1, new string('a', ChatProtocol.MaxLineLength + 1));
                await Assert.ThrowsAsync<FormatException>(() => read, "varsayılan sınır ChatProtocol.MaxLineLength");
            }
        }

        public static async Task SendAfterDisposeThrows()
        {
            var pair = await CreateTcpPairAsync();
            var a = new MessageConnection(pair.Item1);
            pair.Item2.Dispose();
            a.Dispose();
            a.Dispose();   // iki kez Dispose sorun çıkarmamalı
            Assert.True(!a.IsConnected, "Dispose sonrası bağlı değil");
            await Assert.ThrowsAsync<ObjectDisposedException>(() => a.SendAsync(new ChatMessage(MessageType.Text, "a", "", "x")), "Dispose sonrası SendAsync");
        }
    }
}
