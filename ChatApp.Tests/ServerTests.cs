using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatClient;
using ChatCommon;
using ChatServer;

namespace ChatApp.Tests
{
    /// <summary>
    /// ChatServerCore ve ChatClientCore uçtan uca: gerçek sunucu 127.0.0.1'de rastgele bir
    /// portta başlatılır, istemciler TCP ile bağlanır.
    /// </summary>
    public static class ServerTests
    {
        // ------------------------------------------------------------------ yardımcılar

        private sealed class TestServer : IDisposable
        {
            public readonly ChatServerCore Core = new ChatServerCore();
            public readonly List<string> Log = new List<string>();
            public Task Running;

            public TestServer()
            {
                Core.Log += (s, text) => Log.Add(text);
                Running = Core.StartAsync(0);   // 0: işletim sistemi boş bir port seçer
            }

            public void Dispose() { Core.Stop(); }
        }

        /// <summary>Protokolü doğrudan konuşan "ham" istemci (sahte/kötü niyetli istemci testleri için).</summary>
        private static async Task<MessageConnection> ConnectRawAsync(int port)
        {
            var tcp = new TcpClient();
            await tcp.ConnectAsync(IPAddress.Loopback, port);
            return new MessageConnection(tcp);
        }

        private static async Task<MessageConnection> JoinRawAsync(int port, string name)
        {
            MessageConnection c = await ConnectRawAsync(port);
            await c.SendAsync(new ChatMessage(MessageType.Join, name, "", name));
            ChatMessage reply = await c.ReadAsync();
            Assert.Equal(MessageType.JoinAck, reply.Type, name + " katılabilmeli");
            return c;
        }

        /// <summary>İstenen türde mesaj gelene kadar okur (arada gelen UserList/System vb. atlanır).</summary>
        private static async Task<ChatMessage> ReadUntilAsync(MessageConnection c, MessageType type, Func<ChatMessage, bool> match = null)
        {
            while (true)
            {
                ChatMessage m = await c.ReadAsync();
                Assert.True(m != null, type + " beklenirken bağlantı kapandı");
                if (m.Type == type && (match == null || match(m))) return m;
            }
        }

        private static async Task WaitUntilAsync(Func<bool> condition, string what)
        {
            for (int i = 0; i < 200 && !condition(); i++)
                await Task.Delay(10);
            Assert.True(condition(), what);
        }

        // ------------------------------------------------------------------ testler

        public static async Task PortZeroReportsActualPort()
        {
            using (var server = new TestServer())
            {
                await Task.Yield();
                Assert.True(server.Core.IsRunning, "sunucu çalışıyor");
                Assert.True(server.Core.Port > 0, "Port gerçek dinlenen port olmalı, gelen: " + server.Core.Port);
            }
        }

        public static async Task JoinAndBroadcast()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali"))
            using (MessageConnection veli = await JoinRawAsync(server.Core.Port, "veli"))
            {
                ChatMessage list = await ReadUntilAsync(ali, MessageType.UserList, m => m.Text.Contains("veli"));
                Assert.Equal("ali,veli", list.Text, "kullanıcı listesi");

                await veli.SendAsync(new ChatMessage(MessageType.Text, "veli", "", "selam"));
                ChatMessage got = await ReadUntilAsync(ali, MessageType.Text);
                Assert.Equal("veli", got.From, "gönderen");
                Assert.Equal("selam", got.Text, "metin");

                ChatMessage echo = await ReadUntilAsync(veli, MessageType.Text);
                Assert.Equal("selam", echo.Text, "gönderen de kendi mesajını alır");

                await WaitUntilAsync(() => server.Core.Clients.Single(c => c.UserName == "veli").MessageCount == 1, "mesaj sayısı 1");
            }
        }

        public static async Task SenderNameCannotBeSpoofed()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali"))
            using (MessageConnection mallory = await JoinRawAsync(server.Core.Port, "mallory"))
            {
                await mallory.SendAsync(new ChatMessage(MessageType.Text, "ali", "", "ben aliyim"));
                ChatMessage got = await ReadUntilAsync(ali, MessageType.Text);
                Assert.Equal("mallory", got.From, "gönderen adını sunucu belirlemeli");
            }
        }

        public static async Task DuplicateNameRejectedCaseInsensitive()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "Ali"))
            using (MessageConnection second = await ConnectRawAsync(server.Core.Port))
            {
                await second.SendAsync(new ChatMessage(MessageType.Join, "", "", "ALI"));
                ChatMessage reply = await second.ReadAsync();
                Assert.Equal(MessageType.Error, reply.Type, "aynı ad reddedilmeli");
                Assert.True(reply.Text.Contains("kullanımda"), "hata mesajı: " + reply.Text);
                Assert.Equal(null, await second.ReadAsync(), "reddedilen bağlantı kapanmalı");
            }
        }

        public static async Task MessageBeforeJoinIsRejected()
        {
            using (var server = new TestServer())
            using (MessageConnection c = await ConnectRawAsync(server.Core.Port))
            {
                await c.SendAsync(new ChatMessage(MessageType.Text, "x", "", "merhaba"));
                Assert.Equal(MessageType.Error, (await c.ReadAsync()).Type, "Join'den önce mesaj");
                Assert.Equal(null, await c.ReadAsync(), "bağlantı kapanmalı");
                await WaitUntilAsync(() => server.Core.Clients.Count == 0, "istemci listeden çıkmalı");
            }
        }

        public static async Task PrivateMessageReachesOnlyTarget()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali"))
            using (MessageConnection veli = await JoinRawAsync(server.Core.Port, "veli"))
            using (MessageConnection ayse = await JoinRawAsync(server.Core.Port, "ayse"))
            {
                await ali.SendAsync(new ChatMessage(MessageType.Private, "ali", "VELI", "gizli"));
                ChatMessage pm = await ReadUntilAsync(veli, MessageType.Private);
                Assert.Equal("ali", pm.From, "From");
                Assert.Equal("veli", pm.To, "To (sunucudaki gerçek ad)");
                Assert.Equal("gizli", (await ReadUntilAsync(ali, MessageType.Private)).Text, "gönderene kopya");

                // ayse özel mesajı görmemeli: ardından gelen genel mesaj ilk Text/Private olmalı
                await ali.SendAsync(new ChatMessage(MessageType.Text, "ali", "", "herkese"));
                ChatMessage next;
                do { next = await ayse.ReadAsync(); } while (next.Type == MessageType.UserList || next.Type == MessageType.System);
                Assert.Equal(MessageType.Text, next.Type, "ayse özel mesajı almamalı");
            }
        }

        public static async Task PrivateToOfflineUserReturnsError()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali"))
            {
                await ali.SendAsync(new ChatMessage(MessageType.Private, "ali", "yok", "merhaba"));
                ChatMessage err = await ReadUntilAsync(ali, MessageType.Error);
                Assert.True(err.Text.Contains("çevrimiçi değil"), "hata: " + err.Text);
            }
        }

        public static async Task EmptyMessagesAreNotRelayed()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali"))
            using (MessageConnection veli = await JoinRawAsync(server.Core.Port, "veli"))
            {
                await ali.SendAsync(new ChatMessage(MessageType.Text, "ali", "", "   "));
                await ali.SendAsync(new ChatMessage(MessageType.Private, "ali", "veli", "  \n "));   // hata öncesi iletiliyordu
                await ali.SendAsync(new ChatMessage(MessageType.Text, "ali", "", "dolu"));

                ChatMessage next;
                do { next = await veli.ReadAsync(); } while (next.Type == MessageType.UserList || next.Type == MessageType.System);
                Assert.Equal(MessageType.Text, next.Type, "ilk gelen boş özel mesaj olmamalı");
                Assert.Equal("dolu", next.Text, "ilk gelen dolu mesaj");
                Assert.Equal(1, server.Core.Clients.Single(c => c.UserName == "ali").MessageCount, "boş mesajlar sayılmamalı");
            }
        }

        public static async Task LongTextIsTruncated()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali"))
            {
                await ali.SendAsync(new ChatMessage(MessageType.Text, "ali", "", new string('a', 5000)));
                ChatMessage got = await ReadUntilAsync(ali, MessageType.Text);
                Assert.Equal(ChatProtocol.MaxMessageLength, got.Text.Length, "metin MaxMessageLength'e kırpılmalı");
            }
        }

        public static async Task OversizedLineDisconnectsOnlyThatClient()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali"))
            {
                var evil = new TcpClient();
                await evil.ConnectAsync(IPAddress.Loopback, server.Core.Port);
                await WaitUntilAsync(() => server.Core.Clients.Count == 2, "kötü istemci bağlandı");

                // Satır sonu olmadan sınırdan fazla veri: eskiden sunucu bunu sonsuza kadar belleğe alırdı.
                byte[] junk = Encoding.ASCII.GetBytes(new string('A', ChatProtocol.MaxLineLength + 10));
                Task write = evil.GetStream().WriteAsync(junk, 0, junk.Length);

                await WaitUntilAsync(() => server.Core.Clients.Count == 1, "kötü istemci atılmalı");
                Assert.True(server.Log.Any(l => l.Contains("çok uzun")), "günlükte neden yazmalı: " + string.Join(" / ", server.Log));

                // Diğer istemci çalışmaya devam eder
                await ali.SendAsync(new ChatMessage(MessageType.Text, "ali", "", "hâlâ buradayım"));
                Assert.Equal("hâlâ buradayım", (await ReadUntilAsync(ali, MessageType.Text)).Text, "sunucu çalışmaya devam etmeli");
                evil.Dispose();
                try { await write; } catch (Exception) { }
            }
        }

        public static async Task LeaveNotifiesOthers()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali"))
            {
                MessageConnection veli = await JoinRawAsync(server.Core.Port, "veli");
                await veli.SendAsync(new ChatMessage(MessageType.Leave, "veli", "", ""));

                ChatMessage left = await ReadUntilAsync(ali, MessageType.System, m => m.Text.Contains("ayrıldı"));
                Assert.Equal("veli sohbetten ayrıldı.", left.Text, "ayrılma bildirimi");
                Assert.Equal("ali", (await ReadUntilAsync(ali, MessageType.UserList)).Text, "güncel liste");
                veli.Dispose();
            }
        }

        public static async Task KickAndAnnouncement()
        {
            using (var server = new TestServer())
            using (MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali"))
            using (MessageConnection veli = await JoinRawAsync(server.Core.Port, "veli"))
            {
                await server.Core.SendAnnouncementAsync("ders bitti");
                Assert.Equal("DUYURU: ders bitti", (await ReadUntilAsync(veli, MessageType.System, m => m.Text.StartsWith("DUYURU"))).Text, "duyuru");

                await server.Core.KickAsync(server.Core.Clients.Single(c => c.UserName == "veli"));
                Assert.True((await ReadUntilAsync(veli, MessageType.Error)).Text.Contains("kesildi"), "atılan istemci nedenini görmeli");
                await ReadUntilAsync(ali, MessageType.System, m => m.Text == "veli sohbetten ayrıldı.");
            }
        }

        public static async Task StopClosesAllClients()
        {
            var server = new TestServer();
            MessageConnection ali = await JoinRawAsync(server.Core.Port, "ali");
            server.Core.Stop();
            await server.Running;   // kabul döngüsü bitmeli
            Assert.True(!server.Core.IsRunning, "durdu");
            ChatMessage m;
            do { m = await ali.ReadAsync(); } while (m != null);   // bağlantı kapanana kadar oku
            ali.Dispose();
        }

        // ------------------------------------------------------------------ ChatClientCore

        public static async Task ClientCoreEndToEnd()
        {
            using (var server = new TestServer())
            {
                var ali = new ChatClientCore();
                var veli = new ChatClientCore();
                var aliMessages = new List<ChatMessage>();
                string[] aliUsers = null;
                string veliDisconnectReason = null;
                ali.MessageReceived += (s, m) => aliMessages.Add(m);
                ali.UserListReceived += (s, u) => aliUsers = u;
                veli.Disconnected += (s, r) => veliDisconnectReason = r;

                await ali.ConnectAsync("127.0.0.1", server.Core.Port, "ali");
                await veli.ConnectAsync("127.0.0.1", server.Core.Port, "veli");
                Assert.True(ali.IsConnected && veli.IsConnected, "ikisi de bağlı");

                await veli.SendTextAsync("selam ali");
                await veli.SendPrivateAsync("ali", "gizli");
                await WaitUntilAsync(() => aliMessages.Any(m => m.Type == MessageType.Private), "ali özel mesajı almalı");
                Assert.True(aliMessages.Any(m => m.Type == MessageType.Text && m.From == "veli" && m.Text == "selam ali"), "genel mesaj");
                Assert.True(aliUsers != null && aliUsers.SequenceEqual(new[] { "ali", "veli" }), "kullanıcı listesi");

                var third = new ChatClientCore();
                var ex = await Assert.ThrowsAsync<ChatConnectionException>(() => third.ConnectAsync("127.0.0.1", server.Core.Port, "VELI"), "aynı ad");
                Assert.True(ex.Message.Contains("kullanımda"), "hata: " + ex.Message);

                server.Core.Stop();
                await WaitUntilAsync(() => veliDisconnectReason != null, "sunucu durunca Disconnected olayı");
                Assert.True(!veli.IsConnected, "veli artık bağlı değil");
                await Assert.ThrowsAsync<ChatConnectionException>(() => veli.SendTextAsync("x"), "bağlı değilken gönderme");
            }
        }

        public static async Task ClientCoreServerNotRunning()
        {
            // Boş bir port bul, sonra kapat: orada kimse dinlemiyor.
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();

            var client = new ChatClientCore();
            var ex = await Assert.ThrowsAsync<ChatConnectionException>(() => client.ConnectAsync("127.0.0.1", port, "ali"), "sunucu kapalı");
            Assert.True(ex.Message.Contains("bağlanılamadı"), "anlaşılır hata: " + ex.Message);
            Assert.True(!client.IsConnected, "bağlı değil");

            await Assert.ThrowsAsync<ChatConnectionException>(() => client.ConnectAsync("127.0.0.1", port, "a,b"), "geçersiz ad");
            await Assert.ThrowsAsync<ChatConnectionException>(() => client.ConnectAsync(" ", port, "ali"), "boş adres");
        }
    }
}
