using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatCommon;

namespace ChatClient
{
    /// <summary>
    /// Sohbet istemcisinin ağ tarafı. Bağlanma, mesaj gönderme ve gelen mesajları
    /// dinleme işlerini yapar; sonuçları olaylarla forma bildirir.
    /// </summary>
    public class ChatClientCore
    {
        private MessageConnection _connection;

        public string UserName { get; private set; }
        public bool IsConnected { get; private set; }

        public event EventHandler<ChatMessage> MessageReceived;
        public event EventHandler<string[]> UserListReceived;
        /// <summary>Bağlantı kapandığında; parametre kapanma nedenidir.</summary>
        public event EventHandler<string> Disconnected;

        /// <summary>
        /// Sunucuya bağlanır ve kullanıcı adıyla katılır.
        /// Başarısızlıkta anlaşılır bir mesajla ChatConnectionException fırlatır.
        /// </summary>
        public async Task ConnectAsync(string host, int port, string userName)
        {
            if (IsConnected) throw new InvalidOperationException("Zaten bağlısınız.");

            string nameError = ChatProtocol.ValidateUserName(userName);
            if (nameError != null) throw new ChatConnectionException(nameError);
            if (string.IsNullOrWhiteSpace(host)) throw new ChatConnectionException("Sunucu adresi boş olamaz.");

            var tcp = new TcpClient();
            try
            {
                Task connectTask = tcp.ConnectAsync(host.Trim(), port);
                // 5 saniyede bağlanamazsa vazgeç (arayüz uzun süre beklemesin)
                if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                {
                    tcp.Close();
                    throw new ChatConnectionException("Sunucuya bağlanma zaman aşımına uğradı (" + host + ":" + port + ").");
                }
                await connectTask;   // hata varsa burada fırlar
            }
            catch (SocketException ex)
            {
                tcp.Close();
                throw new ChatConnectionException("Sunucuya bağlanılamadı (" + host + ":" + port + "). " +
                                                  "Sunucunun çalıştığından emin olun.\nAyrıntı: " + ex.Message, ex);
            }

            _connection = new MessageConnection(tcp);

            try
            {
                await _connection.SendAsync(new ChatMessage(MessageType.Join, userName, "", userName));

                // Sunucunun yanıtını bekle: JoinAck ya da Error
                ChatMessage reply = await _connection.ReadAsync();
                if (reply == null)
                    throw new ChatConnectionException("Sunucu bağlantıyı kapattı.");
                if (reply.Type == MessageType.Error)
                    throw new ChatConnectionException(reply.Text);
                if (reply.Type != MessageType.JoinAck)
                    throw new ChatConnectionException("Sunucudan beklenmeyen yanıt: " + reply.Type);

                UserName = userName;
                IsConnected = true;
                OnMessage(new ChatMessage(MessageType.System, "Sunucu", "", reply.Text, reply.Timestamp));
            }
            catch (Exception ex) when (ex is IOException || ex is FormatException)
            {
                _connection.Dispose();
                throw new ChatConnectionException("Sunucuyla iletişim kurulamadı: " + ex.Message, ex);
            }
            catch (ChatConnectionException)
            {
                _connection.Dispose();
                throw;
            }

            // Dinleme döngüsünü başlat (beklemeden)
            Task ignored = ReceiveLoopAsync(_connection);
        }

        private async Task ReceiveLoopAsync(MessageConnection connection)
        {
            string reason = "Sunucu bağlantıyı kapattı.";
            try
            {
                while (IsConnected)
                {
                    ChatMessage msg = await connection.ReadAsync();
                    if (msg == null) break;

                    if (msg.Type == MessageType.UserList)
                    {
                        string[] users = msg.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        EventHandler<string[]> h = UserListReceived;
                        if (h != null) h(this, users);
                    }
                    else
                    {
                        OnMessage(msg);
                    }
                }
            }
            catch (IOException) { reason = "Sunucu ile bağlantı koptu."; }
            catch (ObjectDisposedException) { reason = "Bağlantı kesildi."; }
            catch (FormatException ex) { reason = "Sunucudan bozuk veri geldi: " + ex.Message; }
            finally
            {
                // Bu döngü eski bir bağlantıya aitse (yeniden bağlanıldıysa) yeni bağlantıya dokunma.
                if (connection == _connection) CloseConnection(reason);
            }
        }

        public Task SendTextAsync(string text)
        {
            return SendAsync(new ChatMessage(MessageType.Text, UserName, "", text));
        }

        public Task SendPrivateAsync(string toUser, string text)
        {
            return SendAsync(new ChatMessage(MessageType.Private, UserName, toUser, text));
        }

        private async Task SendAsync(ChatMessage msg)
        {
            if (!IsConnected) throw new ChatConnectionException("Sunucuya bağlı değilsiniz.");
            if (string.IsNullOrWhiteSpace(msg.Text)) return;

            try
            {
                await _connection.SendAsync(msg);
            }
            catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException || ex is SocketException)
            {
                CloseConnection("Mesaj gönderilemedi, bağlantı koptu.");
                throw new ChatConnectionException("Mesaj gönderilemedi: bağlantı koptu.", ex);
            }
        }

        /// <summary>Kullanıcının "Bağlantıyı Kes" demesi.</summary>
        public async Task DisconnectAsync()
        {
            if (!IsConnected) return;
            try
            {
                await _connection.SendAsync(new ChatMessage(MessageType.Leave, UserName, "", ""));
            }
            catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException || ex is SocketException)
            {
                // zaten kopmuş olabilir; önemli değil
            }
            CloseConnection("Bağlantıyı siz kestiniz.");
        }

        private void CloseConnection(string reason)
        {
            if (!IsConnected && _connection == null) return;

            bool wasConnected = IsConnected;
            IsConnected = false;
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }

            if (wasConnected)
            {
                EventHandler<string> h = Disconnected;
                if (h != null) h(this, reason);
            }
        }

        private void OnMessage(ChatMessage msg)
        {
            EventHandler<ChatMessage> h = MessageReceived;
            if (h != null) h(this, msg);
        }
    }

    /// <summary>Kullanıcıya gösterilebilecek bağlantı hataları.</summary>
    public class ChatConnectionException : Exception
    {
        public ChatConnectionException(string message) : base(message) { }
        public ChatConnectionException(string message, Exception inner) : base(message, inner) { }
    }
}
