namespace Interface_pattaya
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            tabControl = new TabControl();
            tabDatabase = new TabPage();
            btnTestConnection = new Button();
            lblConnectionStatus = new Label();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtUserId = new TextBox();
            lblUserId = new Label();
            txtDatabase = new TextBox();
            lblDatabase = new Label();
            txtServer = new TextBox();
            lblServer = new Label();
            tabAPI = new TabPage();
            numApiRetryDelay = new NumericUpDown();
            lblApiRetryDelay = new Label();
            numApiRetry = new NumericUpDown();
            lblApiRetry = new Label();
            numApiTimeout = new NumericUpDown();
            lblApiTimeout = new Label();
            txtApiEndpoint = new TextBox();
            lblApiEndpoint = new Label();
            tabLog = new TabPage();
            lblLogRetentionInfo = new Label();
            numLogRetention = new NumericUpDown();
            lblLogRetention = new Label();
            panelButtons = new Panel();
            btnCancel = new Button();
            btnSave = new Button();
            tabControl.SuspendLayout();
            tabDatabase.SuspendLayout();
            tabAPI.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numApiRetryDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numApiRetry).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numApiTimeout).BeginInit();
            tabLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numLogRetention).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabDatabase);
            tabControl.Controls.Add(tabAPI);
            tabControl.Controls.Add(tabLog);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Tahoma", 9F);
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(636, 363);
            tabControl.TabIndex = 0;
            // 
            // tabDatabase
            // 
            tabDatabase.BackColor = Color.White;
            tabDatabase.Controls.Add(btnTestConnection);
            tabDatabase.Controls.Add(lblConnectionStatus);
            tabDatabase.Controls.Add(txtPassword);
            tabDatabase.Controls.Add(lblPassword);
            tabDatabase.Controls.Add(txtUserId);
            tabDatabase.Controls.Add(lblUserId);
            tabDatabase.Controls.Add(txtDatabase);
            tabDatabase.Controls.Add(lblDatabase);
            tabDatabase.Controls.Add(txtServer);
            tabDatabase.Controls.Add(lblServer);
            tabDatabase.Location = new Point(4, 23);
            tabDatabase.Name = "tabDatabase";
            tabDatabase.Padding = new Padding(3);
            tabDatabase.Size = new Size(628, 336);
            tabDatabase.TabIndex = 0;
            tabDatabase.Text = "🗄️ Database";
            // 
            // btnTestConnection
            // 
            btnTestConnection.BackColor = Color.FromArgb(46, 204, 113);
            btnTestConnection.FlatStyle = FlatStyle.Flat;
            btnTestConnection.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            btnTestConnection.ForeColor = Color.White;
            btnTestConnection.Location = new Point(165, 224);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new Size(200, 35);
            btnTestConnection.TabIndex = 9;
            btnTestConnection.Text = "🔌 Test Connection";
            btnTestConnection.UseVisualStyleBackColor = false;
            btnTestConnection.Click += BtnTestConnection_Click;
            // 
            // lblConnectionStatus
            // 
            lblConnectionStatus.Font = new Font("Tahoma", 8.25F);
            lblConnectionStatus.ForeColor = Color.Gray;
            lblConnectionStatus.Location = new Point(165, 281);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new Size(450, 36);
            lblConnectionStatus.TabIndex = 8;
            lblConnectionStatus.Text = "ℹ️ Click 'Test Connection' to verify database settings";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(165, 173);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(450, 22);
            txtPassword.TabIndex = 7;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(30, 176);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(62, 14);
            lblPassword.TabIndex = 6;
            lblPassword.Text = "Password:";
            // 
            // txtUserId
            // 
            txtUserId.Location = new Point(165, 131);
            txtUserId.Name = "txtUserId";
            txtUserId.Size = new Size(450, 22);
            txtUserId.TabIndex = 5;
            // 
            // lblUserId
            // 
            lblUserId.AutoSize = true;
            lblUserId.Location = new Point(30, 134);
            lblUserId.Name = "lblUserId";
            lblUserId.Size = new Size(51, 14);
            lblUserId.TabIndex = 4;
            lblUserId.Text = "User ID:";
            // 
            // txtDatabase
            // 
            txtDatabase.Location = new Point(165, 89);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.Size = new Size(450, 22);
            txtDatabase.TabIndex = 3;
            // 
            // lblDatabase
            // 
            lblDatabase.AutoSize = true;
            lblDatabase.Location = new Point(30, 92);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new Size(61, 14);
            lblDatabase.TabIndex = 2;
            lblDatabase.Text = "Database:";
            // 
            // txtServer
            // 
            txtServer.Location = new Point(165, 47);
            txtServer.Name = "txtServer";
            txtServer.Size = new Size(450, 22);
            txtServer.TabIndex = 1;
            // 
            // lblServer
            // 
            lblServer.AutoSize = true;
            lblServer.Location = new Point(30, 50);
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(46, 14);
            lblServer.TabIndex = 0;
            lblServer.Text = "Server:";
            // 
            // tabAPI
            // 
            tabAPI.BackColor = Color.White;
            tabAPI.Controls.Add(numApiRetryDelay);
            tabAPI.Controls.Add(lblApiRetryDelay);
            tabAPI.Controls.Add(numApiRetry);
            tabAPI.Controls.Add(lblApiRetry);
            tabAPI.Controls.Add(numApiTimeout);
            tabAPI.Controls.Add(lblApiTimeout);
            tabAPI.Controls.Add(txtApiEndpoint);
            tabAPI.Controls.Add(lblApiEndpoint);
            tabAPI.Location = new Point(4, 23);
            tabAPI.Name = "tabAPI";
            tabAPI.Padding = new Padding(3);
            tabAPI.Size = new Size(628, 336);
            tabAPI.TabIndex = 1;
            tabAPI.Text = "🌐 API Settings";
            // 
            // numApiRetryDelay
            // 
            numApiRetryDelay.Location = new Point(200, 200);
            numApiRetryDelay.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            numApiRetryDelay.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numApiRetryDelay.Name = "numApiRetryDelay";
            numApiRetryDelay.Size = new Size(120, 22);
            numApiRetryDelay.TabIndex = 7;
            numApiRetryDelay.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // lblApiRetryDelay
            // 
            lblApiRetryDelay.AutoSize = true;
            lblApiRetryDelay.Location = new Point(30, 202);
            lblApiRetryDelay.Name = "lblApiRetryDelay";
            lblApiRetryDelay.Size = new Size(170, 14);
            lblApiRetryDelay.TabIndex = 6;
            lblApiRetryDelay.Text = "Retry Delay (seconds) [1-60]:";
            // 
            // numApiRetry
            // 
            numApiRetry.Location = new Point(200, 158);
            numApiRetry.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numApiRetry.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numApiRetry.Name = "numApiRetry";
            numApiRetry.Size = new Size(120, 22);
            numApiRetry.TabIndex = 5;
            numApiRetry.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // lblApiRetry
            // 
            lblApiRetry.AutoSize = true;
            lblApiRetry.Location = new Point(30, 160);
            lblApiRetry.Name = "lblApiRetry";
            lblApiRetry.Size = new Size(135, 14);
            lblApiRetry.TabIndex = 4;
            lblApiRetry.Text = "Retry Attempts [1-10]:";
            // 
            // numApiTimeout
            // 
            numApiTimeout.Location = new Point(200, 116);
            numApiTimeout.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numApiTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numApiTimeout.Name = "numApiTimeout";
            numApiTimeout.Size = new Size(120, 22);
            numApiTimeout.TabIndex = 3;
            numApiTimeout.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblApiTimeout
            // 
            lblApiTimeout.AutoSize = true;
            lblApiTimeout.Location = new Point(30, 118);
            lblApiTimeout.Name = "lblApiTimeout";
            lblApiTimeout.Size = new Size(161, 14);
            lblApiTimeout.TabIndex = 2;
            lblApiTimeout.Text = "Timeout (seconds) [5-300]:";
            // 
            // txtApiEndpoint
            // 
            txtApiEndpoint.Location = new Point(33, 70);
            txtApiEndpoint.Name = "txtApiEndpoint";
            txtApiEndpoint.Size = new Size(610, 22);
            txtApiEndpoint.TabIndex = 1;
            // 
            // lblApiEndpoint
            // 
            lblApiEndpoint.AutoSize = true;
            lblApiEndpoint.Location = new Point(30, 36);
            lblApiEndpoint.Name = "lblApiEndpoint";
            lblApiEndpoint.Size = new Size(429, 14);
            lblApiEndpoint.TabIndex = 0;
            lblApiEndpoint.Text = "API Endpoint URL (e.g., https://example.com/api/conHIS/insertPrescription):";
            // 
            // tabLog
            // 
            tabLog.BackColor = Color.White;
            tabLog.Controls.Add(lblLogRetentionInfo);
            tabLog.Controls.Add(numLogRetention);
            tabLog.Controls.Add(lblLogRetention);
            tabLog.Location = new Point(4, 23);
            tabLog.Name = "tabLog";
            tabLog.Size = new Size(628, 336);
            tabLog.TabIndex = 2;
            tabLog.Text = "📋 Logging";
            // 
            // lblLogRetentionInfo
            // 
            lblLogRetentionInfo.ForeColor = Color.Gray;
            lblLogRetentionInfo.Location = new Point(30, 110);
            lblLogRetentionInfo.Name = "lblLogRetentionInfo";
            lblLogRetentionInfo.Size = new Size(578, 60);
            lblLogRetentionInfo.TabIndex = 2;
            lblLogRetentionInfo.Text = "ℹ️ Log files older than the specified number of days will be automatically deleted.\r\nDefault: 30 days";
            // 
            // numLogRetention
            // 
            numLogRetention.Location = new Point(200, 70);
            numLogRetention.Maximum = new decimal(new int[] { 365, 0, 0, 0 });
            numLogRetention.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numLogRetention.Name = "numLogRetention";
            numLogRetention.Size = new Size(120, 22);
            numLogRetention.TabIndex = 1;
            numLogRetention.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblLogRetention
            // 
            lblLogRetention.AutoSize = true;
            lblLogRetention.Location = new Point(30, 72);
            lblLogRetention.Name = "lblLogRetention";
            lblLogRetention.Size = new Size(173, 14);
            lblLogRetention.TabIndex = 0;
            lblLogRetention.Text = "Log Retention (days) [1-365]:";
            // 
            // panelButtons
            // 
            panelButtons.BackColor = Color.FromArgb(240, 240, 240);
            panelButtons.Controls.Add(btnCancel);
            panelButtons.Controls.Add(btnSave);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 363);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(636, 60);
            panelButtons.TabIndex = 1;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(505, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 35);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "❌ Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(52, 152, 219);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(365, 12);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(120, 35);
            btnSave.TabIndex = 0;
            btnSave.Text = "💾 Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(636, 423);
            Controls.Add(tabControl);
            Controls.Add(panelButtons);
            Font = new Font("Tahoma", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings - Interface Pattaya";
            tabControl.ResumeLayout(false);
            tabDatabase.ResumeLayout(false);
            tabDatabase.PerformLayout();
            tabAPI.ResumeLayout(false);
            tabAPI.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numApiRetryDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numApiRetry).EndInit();
            ((System.ComponentModel.ISupportInitialize)numApiTimeout).EndInit();
            tabLog.ResumeLayout(false);
            tabLog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numLogRetention).EndInit();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDatabase;
        private System.Windows.Forms.TabPage tabAPI;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        // Database Tab
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Button btnTestConnection;

        // API Tab
        private System.Windows.Forms.Label lblApiEndpoint;
        private System.Windows.Forms.TextBox txtApiEndpoint;
        private System.Windows.Forms.Label lblApiTimeout;
        private System.Windows.Forms.NumericUpDown numApiTimeout;
        private System.Windows.Forms.Label lblApiRetry;
        private System.Windows.Forms.NumericUpDown numApiRetry;
        private System.Windows.Forms.Label lblApiRetryDelay;
        private System.Windows.Forms.NumericUpDown numApiRetryDelay;

        // Log Tab
        private System.Windows.Forms.Label lblLogRetention;
        private System.Windows.Forms.NumericUpDown numLogRetention;
        private System.Windows.Forms.Label lblLogRetentionInfo;
    }
}