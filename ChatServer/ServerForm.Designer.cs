namespace ChatServer
{
    partial class ServerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.grpServer = new System.Windows.Forms.GroupBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.grpClients = new System.Windows.Forms.GroupBox();
            this.lvClients = new System.Windows.Forms.ListView();
            this.colId = new System.Windows.Forms.ColumnHeader();
            this.colUser = new System.Windows.Forms.ColumnHeader();
            this.colAddress = new System.Windows.Forms.ColumnHeader();
            this.colTime = new System.Windows.Forms.ColumnHeader();
            this.colCount = new System.Windows.Forms.ColumnHeader();
            this.btnKick = new System.Windows.Forms.Button();
            this.grpChat = new System.Windows.Forms.GroupBox();
            this.lstChat = new System.Windows.Forms.ListBox();
            this.txtAnnouncement = new System.Windows.Forms.TextBox();
            this.btnAnnounce = new System.Windows.Forms.Button();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.btnSaveHistory = new System.Windows.Forms.Button();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblClientCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.grpServer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.grpClients.SuspendLayout();
            this.grpChat.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // grpServer
            //
            this.grpServer.Controls.Add(this.lblPort);
            this.grpServer.Controls.Add(this.numPort);
            this.grpServer.Controls.Add(this.lblStatus);
            this.grpServer.Controls.Add(this.btnStart);
            this.grpServer.Controls.Add(this.btnStop);
            this.grpServer.Location = new System.Drawing.Point(12, 12);
            this.grpServer.Name = "grpServer";
            this.grpServer.Size = new System.Drawing.Size(330, 108);
            this.grpServer.TabIndex = 0;
            this.grpServer.TabStop = false;
            this.grpServer.Text = "Sunucu";
            //
            // lblPort
            //
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(15, 30);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(32, 15);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "Port:";
            //
            // numPort
            //
            this.numPort.Location = new System.Drawing.Point(55, 27);
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(90, 23);
            this.numPort.TabIndex = 1;
            this.numPort.Value = new decimal(new int[] {
            5050,
            0,
            0,
            0});
            //
            // lblStatus
            //
            this.lblStatus.BackColor = System.Drawing.Color.Firebrick;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Location = new System.Drawing.Point(160, 26);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(155, 25);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "DURDU";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnStart
            //
            this.btnStart.Location = new System.Drawing.Point(15, 62);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(145, 32);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Sunucuyu Başlat";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            //
            // btnStop
            //
            this.btnStop.Location = new System.Drawing.Point(170, 62);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(145, 32);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Sunucuyu Durdur";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            //
            // grpClients
            //
            this.grpClients.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpClients.Controls.Add(this.lvClients);
            this.grpClients.Controls.Add(this.btnKick);
            this.grpClients.Location = new System.Drawing.Point(12, 128);
            this.grpClients.Name = "grpClients";
            this.grpClients.Size = new System.Drawing.Size(330, 402);
            this.grpClients.TabIndex = 1;
            this.grpClients.TabStop = false;
            this.grpClients.Text = "Bağlı İstemciler";
            //
            // lvClients
            //
            this.lvClients.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvClients.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colId,
            this.colUser,
            this.colAddress,
            this.colTime,
            this.colCount});
            this.lvClients.FullRowSelect = true;
            this.lvClients.GridLines = true;
            this.lvClients.HideSelection = false;
            this.lvClients.Location = new System.Drawing.Point(10, 22);
            this.lvClients.MultiSelect = false;
            this.lvClients.Name = "lvClients";
            this.lvClients.Size = new System.Drawing.Size(310, 330);
            this.lvClients.TabIndex = 0;
            this.lvClients.UseCompatibleStateImageBehavior = false;
            this.lvClients.View = System.Windows.Forms.View.Details;
            this.lvClients.SelectedIndexChanged += new System.EventHandler(this.LvClients_SelectedIndexChanged);
            //
            // colId
            //
            this.colId.Text = "#";
            this.colId.Width = 30;
            //
            // colUser
            //
            this.colUser.Text = "Kullanıcı";
            this.colUser.Width = 85;
            //
            // colAddress
            //
            this.colAddress.Text = "Adres";
            this.colAddress.Width = 105;
            //
            // colTime
            //
            this.colTime.Text = "Bağlandı";
            this.colTime.Width = 60;
            //
            // colCount
            //
            this.colCount.Text = "Msj";
            this.colCount.Width = 35;
            //
            // btnKick
            //
            this.btnKick.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKick.Location = new System.Drawing.Point(10, 360);
            this.btnKick.Name = "btnKick";
            this.btnKick.Size = new System.Drawing.Size(310, 30);
            this.btnKick.TabIndex = 1;
            this.btnKick.Text = "Seçili İstemcinin Bağlantısını Kes";
            this.btnKick.UseVisualStyleBackColor = true;
            this.btnKick.Click += new System.EventHandler(this.BtnKick_Click);
            //
            // grpChat
            //
            this.grpChat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpChat.Controls.Add(this.lstChat);
            this.grpChat.Controls.Add(this.txtAnnouncement);
            this.grpChat.Controls.Add(this.btnAnnounce);
            this.grpChat.Location = new System.Drawing.Point(354, 12);
            this.grpChat.Name = "grpChat";
            this.grpChat.Size = new System.Drawing.Size(534, 300);
            this.grpChat.TabIndex = 2;
            this.grpChat.TabStop = false;
            this.grpChat.Text = "Sohbet Akışı";
            //
            // lstChat
            //
            this.lstChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstChat.HorizontalScrollbar = true;
            this.lstChat.IntegralHeight = false;
            this.lstChat.ItemHeight = 15;
            this.lstChat.Location = new System.Drawing.Point(10, 22);
            this.lstChat.Name = "lstChat";
            this.lstChat.Size = new System.Drawing.Size(514, 232);
            this.lstChat.TabIndex = 0;
            //
            // txtAnnouncement
            //
            this.txtAnnouncement.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAnnouncement.Location = new System.Drawing.Point(10, 264);
            this.txtAnnouncement.Name = "txtAnnouncement";
            this.txtAnnouncement.PlaceholderText = "Tüm istemcilere duyuru...";
            this.txtAnnouncement.Size = new System.Drawing.Size(390, 23);
            this.txtAnnouncement.TabIndex = 1;
            this.txtAnnouncement.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtAnnouncement_KeyDown);
            //
            // btnAnnounce
            //
            this.btnAnnounce.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAnnounce.Location = new System.Drawing.Point(410, 262);
            this.btnAnnounce.Name = "btnAnnounce";
            this.btnAnnounce.Size = new System.Drawing.Size(114, 27);
            this.btnAnnounce.TabIndex = 2;
            this.btnAnnounce.Text = "Duyuru Gönder";
            this.btnAnnounce.UseVisualStyleBackColor = true;
            this.btnAnnounce.Click += new System.EventHandler(this.BtnAnnounce_Click);
            //
            // grpLog
            //
            this.grpLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLog.Controls.Add(this.lstLog);
            this.grpLog.Controls.Add(this.btnSaveHistory);
            this.grpLog.Controls.Add(this.btnClearLog);
            this.grpLog.Location = new System.Drawing.Point(354, 320);
            this.grpLog.Name = "grpLog";
            this.grpLog.Size = new System.Drawing.Size(534, 210);
            this.grpLog.TabIndex = 3;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Sunucu Günlüğü";
            //
            // lstLog
            //
            this.lstLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.lstLog.HorizontalScrollbar = true;
            this.lstLog.IntegralHeight = false;
            this.lstLog.ItemHeight = 14;
            this.lstLog.Location = new System.Drawing.Point(10, 22);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(514, 140);
            this.lstLog.TabIndex = 0;
            //
            // btnSaveHistory
            //
            this.btnSaveHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveHistory.Location = new System.Drawing.Point(10, 170);
            this.btnSaveHistory.Name = "btnSaveHistory";
            this.btnSaveHistory.Size = new System.Drawing.Size(190, 28);
            this.btnSaveHistory.TabIndex = 1;
            this.btnSaveHistory.Text = "Sohbet Geçmişini Kaydet";
            this.btnSaveHistory.UseVisualStyleBackColor = true;
            this.btnSaveHistory.Click += new System.EventHandler(this.BtnSaveHistory_Click);
            //
            // btnClearLog
            //
            this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClearLog.Location = new System.Drawing.Point(210, 170);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(130, 28);
            this.btnClearLog.TabIndex = 2;
            this.btnClearLog.Text = "Günlüğü Temizle";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.BtnClearLog_Click);
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblClientCount});
            this.statusStrip.Location = new System.Drawing.Point(0, 538);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(900, 22);
            this.statusStrip.TabIndex = 4;
            //
            // lblClientCount
            //
            this.lblClientCount.Name = "lblClientCount";
            this.lblClientCount.Size = new System.Drawing.Size(100, 17);
            this.lblClientCount.Text = "Bağlı istemci: 0";
            //
            // saveFileDialog
            //
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.Filter = "Metin dosyası (*.txt)|*.txt";
            //
            // ServerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 560);
            this.Controls.Add(this.grpServer);
            this.Controls.Add(this.grpClients);
            this.Controls.Add(this.grpChat);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.statusStrip);
            this.MinimumSize = new System.Drawing.Size(860, 500);
            this.Name = "ServerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YMH219 Chat Sunucu";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerForm_FormClosing);
            this.grpServer.ResumeLayout(false);
            this.grpServer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.grpClients.ResumeLayout(false);
            this.grpChat.ResumeLayout(false);
            this.grpChat.PerformLayout();
            this.grpLog.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpServer;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.GroupBox grpClients;
        private System.Windows.Forms.ListView lvClients;
        private System.Windows.Forms.ColumnHeader colId;
        private System.Windows.Forms.ColumnHeader colUser;
        private System.Windows.Forms.ColumnHeader colAddress;
        private System.Windows.Forms.ColumnHeader colTime;
        private System.Windows.Forms.ColumnHeader colCount;
        private System.Windows.Forms.Button btnKick;
        private System.Windows.Forms.GroupBox grpChat;
        private System.Windows.Forms.ListBox lstChat;
        private System.Windows.Forms.TextBox txtAnnouncement;
        private System.Windows.Forms.Button btnAnnounce;
        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.Button btnSaveHistory;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblClientCount;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}
