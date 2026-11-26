using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using ChatCommon;

namespace ChatServer
{
    public partial class ServerForm : Form
    {
        private readonly ChatServerCore _server = new ChatServerCore();
        private readonly List<ChatMessage> _history = new List<ChatMessage>();

        public ServerForm()
        {
            InitializeComponent();

            numPort.Value = ChatProtocol.DefaultPort;

            _server.Log += (s, text) => RunOnUi(() => AddLog(text));
            _server.ClientsChanged += (s, e) => RunOnUi(RefreshClientList);
            _server.RunningChanged += (s, e) => RunOnUi(UpdateServerState);
            _server.MessageRelayed += (s, msg) => RunOnUi(() => AddChat(msg));

            UpdateServerState();
        }

        // ------------------------------------------------------------ sunucu kontrolü

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            int port = (int)numPort.Value;
            try
            {
                // Kabul döngüsü sunucu durana kadar burada "bekler"; arayüz donmaz.
                await _server.StartAsync(port);
            }
            catch (SocketException ex)
            {
                AddLog("Sunucu başlatılamadı: " + ex.Message);
                MessageBox.Show(
                    "Sunucu " + port + " numaralı portta başlatılamadı.\n" +
                    "Port başka bir uygulama tarafından kullanılıyor olabilir.\n\nAyrıntı: " + ex.Message,
                    "Sunucu Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateServerState();
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            _server.Stop();
        }

        private async void BtnKick_Click(object sender, EventArgs e)
        {
            if (lvClients.SelectedItems.Count == 0) return;
            var session = lvClients.SelectedItems[0].Tag as ClientSession;
            if (session != null) await _server.KickAsync(session);
        }

        private async void BtnAnnounce_Click(object sender, EventArgs e)
        {
            string text = txtAnnouncement.Text.Trim();
            if (text.Length == 0) return;
            await _server.SendAnnouncementAsync(text);
            txtAnnouncement.Clear();
        }

        private void TxtAnnouncement_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnAnnounce.PerformClick();
            }
        }

        private void BtnSaveHistory_Click(object sender, EventArgs e)
        {
            if (_history.Count == 0)
            {
                MessageBox.Show("Kaydedilecek mesaj yok.", "Geçmiş", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            saveFileDialog.FileName = "sohbet-gecmisi-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
            if (saveFileDialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                File.WriteAllLines(saveFileDialog.FileName, _history.Select(m => m.ToDisplayString()), new UTF8Encoding(true));
                AddLog("Sohbet geçmişi kaydedildi: " + saveFileDialog.FileName);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("Dosya kaydedilemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            lstLog.Items.Clear();
        }

        private void LvClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnKick.Enabled = _server.IsRunning && lvClients.SelectedItems.Count > 0;
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _server.Stop();
        }

        // ------------------------------------------------------------ arayüz güncelleme

        private void UpdateServerState()
        {
            bool running = _server.IsRunning;
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            numPort.Enabled = !running;
            btnAnnounce.Enabled = running;
            btnKick.Enabled = running && lvClients.SelectedItems.Count > 0;

            lblStatus.Text = running ? "ÇALIŞIYOR" : "DURDU";
            lblStatus.BackColor = running ? Color.SeaGreen : Color.Firebrick;
            Text = running ? "YMH219 Chat Sunucu - Port " + _server.Port : "YMH219 Chat Sunucu";
        }

        private void RefreshClientList()
        {
            lvClients.BeginUpdate();
            lvClients.Items.Clear();
            foreach (ClientSession c in _server.Clients)
            {
                var item = new ListViewItem(c.Id.ToString());
                item.SubItems.Add(c.HasJoined ? c.UserName : "(giriş bekleniyor)");
                item.SubItems.Add(c.Address);
                item.SubItems.Add(c.ConnectedAt.ToString("HH:mm:ss"));
                item.SubItems.Add(c.MessageCount.ToString());
                item.Tag = c;
                lvClients.Items.Add(item);
            }
            lvClients.EndUpdate();

            int joined = _server.Clients.Count(c => c.HasJoined);
            lblClientCount.Text = "Bağlı istemci: " + joined;
            btnKick.Enabled = _server.IsRunning && lvClients.SelectedItems.Count > 0;
        }

        private void AddLog(string text)
        {
            lstLog.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text);
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        private void AddChat(ChatMessage msg)
        {
            _history.Add(msg);
            lstChat.Items.Add(msg.ToDisplayString());
            lstChat.TopIndex = lstChat.Items.Count - 1;
        }

        /// <summary>Başka bir iş parçacığından gelirse kodu UI iş parçacığına aktarır.</summary>
        private void RunOnUi(Action action)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }
    }
}
