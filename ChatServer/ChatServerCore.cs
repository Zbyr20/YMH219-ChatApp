using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatCommon;

namespace ChatServer
{
    /// <summary>
    /// TCP sohbet sunucusu. Arayüzden bağımsızdır: form sadece olaylarını dinler.
    ///
    /// Not: StartAsync arayüz (UI) iş parçacığından çağrıldığında, await'lerden sonraki kod
    /// yine UI iş parçacığında devam eder (SynchronizationContext). Bu yüzden istemci listesi
    /// tek bir iş parçacığından değiştirilir ve kilit (lock) gerekmez; ağ beklemeleri ise
    /// arayüzü dondurmaz.
    /// </summary>
    public class ChatServerCore
    {
        private readonly List<ClientSession> _clients = new List<ClientSession>();
        private TcpListener _listener;

        public bool IsRunning { get; private set; }
        public int Port { get; private set; }

        public IReadOnlyList<ClientSession> Clients { get { return _clients; } }

        /// <summary>Sunucu günlüğüne yazılacak her olay (zaman damgası form tarafında eklenir).</summary>
        public event EventHandler<string> Log;
        /// <summary>İstemci listesi değiştiğinde (katılma/ayrılma).</summary>
        public event EventHandler ClientsChanged;
        /// <summary>Sohbete gönderilen her mesaj (geçmiş için).</summary>
        public event EventHandler<ChatMessage> MessageRelayed;
        /// <summary>Sunucu başladığında ya da durduğunda.</summary>
        public event EventHandler RunningChanged;

        /// <summary>Dinlemeye başlar ve bağlantı kabul döngüsünü çalıştırır.</summary>
        public async Task StartAsync(int port)
        {
            if (IsRunning) return;

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();   // port kullanımdaysa SocketException fırlatır (form yakalar)
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;   // port 0 verilirse işletim sisteminin seçtiği port
            IsRunning = true;
            Raise(RunningChanged);
            WriteLog("Sunucu başlatıldı. Port: " + port);

            await AcceptLoopAsync();
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;

            try { _listener.Stop(); }
            catch (SocketException) { }

            foreach (ClientSession c in _clients.ToList())
                c.Connection.Dispose();   // okuma döngüleri kendiliğinden sonlanır
            _clients.Clear();

            WriteLog("Sunucu durduruldu.");
            Raise(ClientsChanged);
            Raise(RunningChanged);
        }

        private async Task AcceptLoopAsync()
        {
            while (IsRunning)
            {
                TcpClient tcp;
                try
                {
                    tcp = await _listener.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException) { break; }    // Stop() çağrıldı
                catch (SocketException) { break; }
                catch (InvalidOperationException) { break; }

                var session = new ClientSession(new MessageConnection(tcp));
                _clients.Add(session);
                WriteLog("Yeni bağlantı: " + session.Address);

                // Her istemci kendi okuma döngüsünde çalışır; burada beklemiyoruz.
                Task ignored = HandleClientAsync(session);
            }
        }

        private async Task HandleClientAsync(ClientSession session)
        {
            string reason = "bağlantı kapandı";
            try
            {
                while (IsRunning)
                {
                    ChatMessage msg = await session.Connection.ReadAsync();
                    if (msg == null) break;                       // istemci kapattı

                    bool keepOpen = await ProcessMessageAsync(session, msg);
                    if (!keepOpen) { reason = session.HasJoined ? "ayrıldı" : "bağlantısı reddedildi"; break; }
                }
            }
            catch (FormatException ex)
            {
                reason = "geçersiz mesaj (" + ex.Message + ")";
            }
            catch (IOException)
            {
                reason = "bağlantı koptu";
            }
            catch (ObjectDisposedException)
            {
                reason = "bağlantı kapatıldı";
            }
            finally
            {
                await RemoveClientAsync(session, reason);
            }
        }

        /// <summary>Gelen mesajı işler. Bağlantı kapatılacaksa false döner.</summary>
        private async Task<bool> ProcessMessageAsync(ClientSession session, ChatMessage msg)
        {
            if (!session.HasJoined)
            {
                if (msg.Type != MessageType.Join)
                {
                    await SafeSendAsync(session, new ChatMessage(MessageType.Error, "Sunucu", "", "Önce kullanıcı adıyla katılmalısınız."));
                    return false;
                }
                return await HandleJoinAsync(session, msg.Text.Trim());
            }

            switch (msg.Type)
            {
                case MessageType.Text:
                    if (string.IsNullOrWhiteSpace(msg.Text)) return true;
                    session.MessageCount++;
                    // Gönderen adını istemciye güvenmeden sunucu belirler.
                    var text = new ChatMessage(MessageType.Text, session.UserName, "", Truncate(msg.Text));
                    await BroadcastAsync(text);
                    Raise(MessageRelayed, text);
                    Raise(ClientsChanged);
                    return true;

                case MessageType.Private:
                    if (string.IsNullOrWhiteSpace(msg.Text)) return true;   // boş özel mesajı iletme
                    session.MessageCount++;
                    await HandlePrivateAsync(session, msg);
                    Raise(ClientsChanged);
                    return true;

                case MessageType.Leave:
                    return false;

                default:
                    WriteLog(session.UserName + " beklenmeyen mesaj türü gönderdi: " + msg.Type);
                    return true;
            }
        }

        private async Task<bool> HandleJoinAsync(ClientSession session, string name)
        {
            string error = ChatProtocol.ValidateUserName(name);
            if (error == null && _clients.Any(c => c.HasJoined && string.Equals(c.UserName, name, StringComparison.OrdinalIgnoreCase)))
                error = "'" + name + "' kullanıcı adı zaten kullanımda.";

            if (error != null)
            {
                WriteLog(session.Address + " reddedildi: " + error);
                await SafeSendAsync(session, new ChatMessage(MessageType.Error, "Sunucu", "", error));
                return false;
            }

            session.UserName = name;
            await SafeSendAsync(session, new ChatMessage(MessageType.JoinAck, "Sunucu", name, "Hoş geldin " + name + "!"));

            var joined = new ChatMessage(MessageType.System, "Sunucu", "", name + " sohbete katıldı.");
            await BroadcastAsync(joined);
            Raise(MessageRelayed, joined);
            await BroadcastUserListAsync();

            WriteLog(name + " katıldı (" + session.Address + ")");
            Raise(ClientsChanged);
            return true;
        }

        private async Task HandlePrivateAsync(ClientSession sender, ChatMessage msg)
        {
            ClientSession target = _clients.FirstOrDefault(c => c.HasJoined &&
                string.Equals(c.UserName, msg.To, StringComparison.OrdinalIgnoreCase));

            if (target == null)
            {
                await SafeSendAsync(sender, new ChatMessage(MessageType.Error, "Sunucu", sender.UserName,
                    "'" + msg.To + "' adlı kullanıcı çevrimiçi değil."));
                return;
            }

            var pm = new ChatMessage(MessageType.Private, sender.UserName, target.UserName, Truncate(msg.Text));
            await SafeSendAsync(target, pm);
            if (target != sender) await SafeSendAsync(sender, pm);   // gönderene de kopya
            WriteLog("Özel mesaj: " + sender.UserName + " → " + target.UserName);
        }

        private async Task RemoveClientAsync(ClientSession session, string reason)
        {
            if (!_clients.Remove(session)) return;   // zaten kaldırılmış (Stop)
            session.Connection.Dispose();

            if (session.HasJoined)
            {
                WriteLog(session.UserName + " " + reason + ".");
                var left = new ChatMessage(MessageType.System, "Sunucu", "", session.UserName + " sohbetten ayrıldı.");
                await BroadcastAsync(left);
                Raise(MessageRelayed, left);
                await BroadcastUserListAsync();
            }
            else
            {
                WriteLog(session.Address + " " + reason + ".");
            }
            Raise(ClientsChanged);
        }

        /// <summary>Sunucu yöneticisinin tüm istemcilere duyuru göndermesi.</summary>
        public async Task SendAnnouncementAsync(string text)
        {
            if (!IsRunning || string.IsNullOrWhiteSpace(text)) return;
            var msg = new ChatMessage(MessageType.System, "Sunucu", "", "DUYURU: " + Truncate(text));
            await BroadcastAsync(msg);
            Raise(MessageRelayed, msg);
        }

        /// <summary>Bir istemciyi sunucudan çıkarır.</summary>
        public async Task KickAsync(ClientSession session)
        {
            await SafeSendAsync(session, new ChatMessage(MessageType.Error, "Sunucu", "", "Sunucu tarafından bağlantınız kesildi."));
            session.Connection.Dispose();   // okuma döngüsü hata alır ve RemoveClientAsync çalışır
        }

        private Task BroadcastUserListAsync()
        {
            string names = string.Join(",", _clients.Where(c => c.HasJoined).Select(c => c.UserName));
            return BroadcastAsync(new ChatMessage(MessageType.UserList, "Sunucu", "", names));
        }

        private async Task BroadcastAsync(ChatMessage msg)
        {
            foreach (ClientSession c in _clients.Where(c => c.HasJoined).ToList())
                await SafeSendAsync(c, msg);
        }

        /// <summary>Gönderim hatası tek istemciyi etkiler; sunucuyu çökertmez.</summary>
        private async Task SafeSendAsync(ClientSession session, ChatMessage msg)
        {
            try
            {
                await session.Connection.SendAsync(msg);
            }
            catch (IOException) { session.Connection.Dispose(); }
            catch (ObjectDisposedException) { }
            catch (SocketException) { session.Connection.Dispose(); }
        }

        private static string Truncate(string text)
        {
            return text.Length <= ChatProtocol.MaxMessageLength ? text : text.Substring(0, ChatProtocol.MaxMessageLength);
        }

        private void WriteLog(string text)
        {
            EventHandler<string> h = Log;
            if (h != null) h(this, text);
        }

        private void Raise(EventHandler h)
        {
            if (h != null) h(this, EventArgs.Empty);
        }

        private void Raise(EventHandler<ChatMessage> h, ChatMessage m)
        {
            if (h != null) h(this, m);
        }
    }
}
