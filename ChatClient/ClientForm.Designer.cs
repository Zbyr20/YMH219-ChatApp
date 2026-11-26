namespace ChatClient
{
    partial class ClientForm
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
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.lblHost = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.lblUser = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.lblState = new System.Windows.Forms.Label();
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.grpUsers = new System.Windows.Forms.GroupBox();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.lblUsersHint = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnSaveHistory = new System.Windows.Forms.Button();
            this.lblTarget = new System.Windows.Forms.Label();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.grpConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.grpUsers.SuspendLayout();
            this.SuspendLayout();
            //
            // grpConnection
            //
            this.grpConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConnection.Controls.Add(this.lblHost);
            this.grpConnection.Controls.Add(this.txtHost);
            this.grpConnection.Controls.Add(this.lblPort);
            this.grpConnection.Controls.Add(this.numPort);
            this.grpConnection.Controls.Add(this.lblUser);
            this.grpConnection.Controls.Add(this.txtUser);
            this.grpConnection.Controls.Add(this.btnConnect);
            this.grpConnection.Controls.Add(this.btnDisconnect);
            this.grpConnection.Controls.Add(this.lblState);
            this.grpConnection.Location = new System.Drawing.Point(12, 12);
            this.grpConnection.Name = "grpConnection";
            this.grpConnection.Size = new System.Drawing.Size(856, 92);
            this.grpConnection.TabIndex = 0;
            this.grpConnection.TabStop = false;
            this.grpConnection.Text = "Bağlantı";
            //
            // lblHost
            //
            this.lblHost.AutoSize = true;
            this.lblHost.Location = new System.Drawing.Point(12, 28);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(51, 15);
            this.lblHost.TabIndex = 0;
            this.lblHost.Text = "Sunucu:";
            //
            // txtHost
            //
            this.txtHost.Location = new System.Drawing.Point(70, 25);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(130, 23);
            this.txtHost.TabIndex = 1;
            this.txtHost.Text = "127.0.0.1";
            //
            // lblPort
            //
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(210, 28);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(32, 15);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "Port:";
            //
            // numPort
            //
            this.numPort.Location = new System.Drawing.Point(248, 25);
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(80, 23);
            this.numPort.TabIndex = 3;
            this.numPort.Value = new decimal(new int[] {
            5050,
            0,
            0,
            0});
            //
            // lblUser
            //
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(340, 28);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(76, 15);
            this.lblUser.TabIndex = 4;
            this.lblUser.Text = "Kullanıcı adı:";
            //
            // txtUser
            //
            this.txtUser.Location = new System.Drawing.Point(422, 25);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(140, 23);
            this.txtUser.TabIndex = 5;
            //
            // btnConnect
            //
            this.btnConnect.Location = new System.Drawing.Point(575, 21);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(130, 30);
            this.btnConnect.TabIndex = 6;
            this.btnConnect.Text = "Bağlan";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            //
            // btnDisconnect
            //
            this.btnDisconnect.Location = new System.Drawing.Point(712, 21);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(130, 30);
            this.btnDisconnect.TabIndex = 7;
            this.btnDisconnect.Text = "Bağlantıyı Kes";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.BtnDisconnect_Click);
            //
            // lblState
            //
            this.lblState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblState.BackColor = System.Drawing.Color.Firebrick;
            this.lblState.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblState.ForeColor = System.Drawing.Color.White;
            this.lblState.Location = new System.Drawing.Point(12, 58);
            this.lblState.Name = "lblState";
            this.lblState.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.lblState.Size = new System.Drawing.Size(830, 24);
            this.lblState.TabIndex = 8;
            this.lblState.Text = "Bağlı değil";
            this.lblState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // rtbChat
            //
            this.rtbChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbChat.BackColor = System.Drawing.Color.White;
            this.rtbChat.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.rtbChat.Location = new System.Drawing.Point(12, 112);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.ReadOnly = true;
            this.rtbChat.Size = new System.Drawing.Size(640, 380);
            this.rtbChat.TabIndex = 1;
            this.rtbChat.TabStop = false;
            this.rtbChat.Text = "";
            //
            // grpUsers
            //
            this.grpUsers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpUsers.Controls.Add(this.lstUsers);
            this.grpUsers.Controls.Add(this.lblUsersHint);
            this.grpUsers.Location = new System.Drawing.Point(664, 106);
            this.grpUsers.Name = "grpUsers";
            this.grpUsers.Size = new System.Drawing.Size(204, 386);
            this.grpUsers.TabIndex = 2;
            this.grpUsers.TabStop = false;
            this.grpUsers.Text = "Çevrimiçi";
            //
            // lstUsers
            //
            this.lstUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstUsers.IntegralHeight = false;
            this.lstUsers.ItemHeight = 15;
            this.lstUsers.Location = new System.Drawing.Point(10, 22);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(184, 304);
            this.lstUsers.TabIndex = 0;
            this.lstUsers.SelectedIndexChanged += new System.EventHandler(this.LstUsers_SelectedIndexChanged);
            //
            // lblUsersHint
            //
            this.lblUsersHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUsersHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblUsersHint.Location = new System.Drawing.Point(10, 330);
            this.lblUsersHint.Name = "lblUsersHint";
            this.lblUsersHint.Size = new System.Drawing.Size(184, 48);
            this.lblUsersHint.TabIndex = 1;
            this.lblUsersHint.Text = "Özel mesaj için bir kullanıcı seçin. Herkese yazmak için \"(Herkese)\" seçin.";
            //
            // txtMessage
            //
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMessage.Location = new System.Drawing.Point(12, 502);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(520, 25);
            this.txtMessage.TabIndex = 3;
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtMessage_KeyDown);
            //
            // btnSend
            //
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(542, 500);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(110, 30);
            this.btnSend.TabIndex = 4;
            this.btnSend.Text = "Gönder";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.BtnSend_Click);
            //
            // btnSaveHistory
            //
            this.btnSaveHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveHistory.Location = new System.Drawing.Point(664, 500);
            this.btnSaveHistory.Name = "btnSaveHistory";
            this.btnSaveHistory.Size = new System.Drawing.Size(204, 30);
            this.btnSaveHistory.TabIndex = 5;
            this.btnSaveHistory.Text = "Geçmişi Kaydet (.txt)";
            this.btnSaveHistory.UseVisualStyleBackColor = true;
            this.btnSaveHistory.Click += new System.EventHandler(this.BtnSaveHistory_Click);
            //
            // lblTarget
            //
            this.lblTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTarget.AutoSize = true;
            this.lblTarget.ForeColor = System.Drawing.Color.DimGray;
            this.lblTarget.Location = new System.Drawing.Point(12, 535);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(83, 15);
            this.lblTarget.TabIndex = 6;
            this.lblTarget.Text = "Alıcı: Herkes";
            //
            // saveFileDialog
            //
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.Filter = "Metin dosyası (*.txt)|*.txt";
            //
            // ClientForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 560);
            this.Controls.Add(this.grpConnection);
            this.Controls.Add(this.rtbChat);
            this.Controls.Add(this.grpUsers);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnSaveHistory);
            this.Controls.Add(this.lblTarget);
            this.MinimumSize = new System.Drawing.Size(896, 480);
            this.Name = "ClientForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YMH219 Chat İstemci";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientForm_FormClosing);
            this.grpConnection.ResumeLayout(false);
            this.grpConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.grpUsers.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.GroupBox grpUsers;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Label lblUsersHint;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnSaveHistory;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}
