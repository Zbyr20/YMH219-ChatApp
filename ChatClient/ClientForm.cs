using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ChatCommon;

namespace ChatClient
{
    public partial class ClientForm : Form
    {
        private const string Everyone = "(Herkese)";

        private readonly ChatClientCore _client = new ChatClientCore();
        private readonly List<string> _history = new List<string>();
        private bool _busy;   // bağlanma sürerken butonları kilitlemek için

        public ClientForm()
        {
            InitializeComponent();

            numPort.Value = ChatProtocol.DefaultPort;
            txtUser.MaxLength = ChatProtocol.MaxUserNameLength;
            txtMessage.MaxLength = ChatProtocol.MaxMessageLength;

            _client.MessageReceived += (s, msg) => RunOnUi(() => ShowMessage(msg));
            _client.UserListReceived += (s, users) => RunOnUi(() => UpdateUserList(users));
            _client.Disconnected += (s, reason) => RunOnUi(() => OnDisconnected(reason));

            UpdateUi();
        }

        // ------------------------------------------------------------ bağlantı

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            if (_busy || _client.IsConnected) return;   // çift tıklama / tekrar bağlanma koruması

            _busy = true;
            UpdateUi();
            SetState("Bağlanıyor...", Color.DarkOrange);

            try
            {
                await _client.ConnectAsync(txtHost.Text, (int)numPort.Value, txtUser.Text.Trim());
                SetState("Bağlı: " + _client.UserName + " @ " + txtHost.Text.Trim() + ":" + numPort.Value, Color.SeaGreen);
                Text = "YMH219 Chat - " + _client.UserName;
            }
            catch (ChatConnectionException ex)
            {
                SetState("Bağlı değil", Color.Firebrick);
                AppendLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] HATA: " + ex.Message, Color.Firebrick, false);
                MessageBox.Show(ex.Message, "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _busy = false;
                UpdateUi();
                // Odak, mesaj kutusu etkinleştirildikten SONRA verilmeli; aksi halde
                // odak "Bağlan" butonunda kalır ve yazılan boşluklar butona basar.
                if (_client.IsConnected) txtMessage.Focus();
            }
        }

        private async void BtnDisconnect_Click(object sender, EventArgs e)
        {
            await _client.DisconnectAsync();
        }

        private void OnDisconnected(string reason)
        {
            AppendLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] * " + reason, Color.Gray, true);
            SetState("Bağlı değil — " + reason, Color.Firebrick);
            lstUsers.Items.Clear();
            Text = "YMH219 Chat İstemci";
            UpdateUi();
        }

        // ------------------------------------------------------------ mesaj gönderme

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            string text = txtMessage.Text.Trim();
            if (text.Length == 0 || !_client.IsConnected) return;

            string target = SelectedTarget();
            try
            {
                if (target == null) await _client.SendTextAsync(text);
                else await _client.SendPrivateAsync(target, text);

                txtMessage.Clear();
            }
            catch (ChatConnectionException ex)
            {
                MessageBox.Show(ex.Message, "Gönderim Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            txtMessage.Focus();
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter = gönder, Shift+Enter = yeni satır
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                btnSend.PerformClick();
            }
        }

        /// <summary>Seçili özel mesaj alıcısı; herkese gönderilecekse null.</summary>
        private string SelectedTarget()
        {
            var selected = lstUsers.SelectedItem as string;
            if (selected == null || selected == Everyone || selected == _client.UserName) return null;
            return selected;
        }

        private void LstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            string target = SelectedTarget();
            lblTarget.Text = target == null ? "Alıcı: Herkes" : "Alıcı: " + target + " (özel mesaj)";
            btnSend.Text = target == null ? "Gönder" : "Özel Gönder";
        }

        // ------------------------------------------------------------ görüntüleme

        private void ShowMessage(ChatMessage msg)
        {
            string time = "[" + msg.Timestamp.ToString("HH:mm:ss") + "] ";
            switch (msg.Type)
            {
                case MessageType.Text:
                    bool mine = msg.From == _client.UserName;
                    AppendLine(time + msg.From + ": " + msg.Text, mine ? Color.RoyalBlue : Color.Black, mine);
                    break;
                case MessageType.Private:
                    string who = msg.From == _client.UserName ? "siz → " + msg.To : msg.From + " → siz";
                    AppendLine(time + "(özel) " + who + ": " + msg.Text, Color.DarkMagenta, true);
                    break;
                case MessageType.System:
                    AppendLine(time + "* " + msg.Text, Color.DarkGreen, false);
                    break;
                case MessageType.Error:
                    AppendLine(time + "HATA: " + msg.Text, Color.Firebrick, true);
                    break;
            }
        }

        private void AppendLine(string text, Color color, bool bold)
        {
            _history.Add(text);

            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            rtbChat.SelectionColor = color;
            rtbChat.SelectionFont = new Font(rtbChat.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            rtbChat.AppendText(text + Environment.NewLine);
            rtbChat.SelectionColor = rtbChat.ForeColor;
            rtbChat.ScrollToCaret();
        }

        private void UpdateUserList(string[] users)
        {
            string previous = lstUsers.SelectedItem as string;

            lstUsers.BeginUpdate();
            lstUsers.Items.Clear();
            lstUsers.Items.Add(Everyone);
            foreach (string u in users)
                if (u != _client.UserName) lstUsers.Items.Add(u);
            lstUsers.EndUpdate();

            int idx = previous != null ? lstUsers.Items.IndexOf(previous) : -1;
            lstUsers.SelectedIndex = idx >= 0 ? idx : 0;
            grpUsers.Text = "Çevrimiçi (" + users.Length + ")";
        }

        private void SetState(string text, Color color)
        {
            lblState.Text = text;
            lblState.BackColor = color;
        }

        private void UpdateUi()
        {
            bool connected = _client.IsConnected;
            btnConnect.Enabled = !connected && !_busy;
            btnDisconnect.Enabled = connected;
            txtHost.Enabled = !connected && !_busy;
            numPort.Enabled = !connected && !_busy;
            txtUser.Enabled = !connected && !_busy;
            txtMessage.Enabled = connected;
            btnSend.Enabled = connected;
            lstUsers.Enabled = connected;
            if (!connected)
            {
                lblTarget.Text = "Alıcı: Herkes";
                btnSend.Text = "Gönder";
                grpUsers.Text = "Çevrimiçi";
            }
        }

        private void BtnSaveHistory_Click(object sender, EventArgs e)
        {
            if (_history.Count == 0)
            {
                MessageBox.Show("Kaydedilecek mesaj yok.", "Geçmiş", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            saveFileDialog.FileName = "sohbet-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
            if (saveFileDialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                File.WriteAllLines(saveFileDialog.FileName, _history, new UTF8Encoding(true));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("Dosya kaydedilemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_client.IsConnected) await _client.DisconnectAsync();
        }

        private void RunOnUi(Action action)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }
    }
}
