namespace Interface_pattaya
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            statusLabel = new Label();
            lastCheckLabel = new Label();
            lastFoundLabel = new Label();
            lastSuccessLabel = new Label();
            connectionStatusLabel = new Label();
            startStopButton = new Button();
            settingsButton = new Button();
            exportButton = new Button();
            dateLabel = new Label();
            dateTimePicker = new DateTimePicker();
            searchLabel = new Label();
            searchTextBox = new TextBox();
            searchButton = new Button();
            refreshButton = new Button();
            totalPanel = new Panel();
            totalLabel = new Label();
            totalCountLabel = new Label();
            successPanel = new Panel();
            successLabel = new Label();
            successCountLabel = new Label();
            failedPanel = new Panel();
            failedLabel = new Label();
            failedCountLabel = new Label();
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            groupBox4 = new GroupBox();
            dataGridView = new DataGridView();
            totalPanel.SuspendLayout();
            successPanel.SuspendLayout();
            failedPanel.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            statusLabel.Location = new Point(18, 23);
            statusLabel.Margin = new Padding(4, 0, 4, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(68, 14);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "Status: ...";
            // 
            // lastCheckLabel
            // 
            lastCheckLabel.AutoSize = true;
            lastCheckLabel.Location = new Point(18, 46);
            lastCheckLabel.Margin = new Padding(4, 0, 4, 0);
            lastCheckLabel.Name = "lastCheckLabel";
            lastCheckLabel.Size = new Size(75, 15);
            lastCheckLabel.TabIndex = 1;
            lastCheckLabel.Text = "Last Check: -";
            // 
            // lastFoundLabel
            // 
            lastFoundLabel.AutoSize = true;
            lastFoundLabel.Location = new Point(18, 67);
            lastFoundLabel.Margin = new Padding(4, 0, 4, 0);
            lastFoundLabel.Name = "lastFoundLabel";
            lastFoundLabel.Size = new Size(76, 15);
            lastFoundLabel.TabIndex = 2;
            lastFoundLabel.Text = "Last Found: -";
            // 
            // lastSuccessLabel
            // 
            lastSuccessLabel.AutoSize = true;
            lastSuccessLabel.Location = new Point(467, 46);
            lastSuccessLabel.Margin = new Padding(4, 0, 4, 0);
            lastSuccessLabel.Name = "lastSuccessLabel";
            lastSuccessLabel.Size = new Size(83, 15);
            lastSuccessLabel.TabIndex = 3;
            lastSuccessLabel.Text = "Last Success: -";
            // 
            // connectionStatusLabel
            // 
            connectionStatusLabel.AutoSize = true;
            connectionStatusLabel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            connectionStatusLabel.ForeColor = Color.Gray;
            connectionStatusLabel.Location = new Point(18, 88);
            connectionStatusLabel.Margin = new Padding(4, 0, 4, 0);
            connectionStatusLabel.Name = "connectionStatusLabel";
            connectionStatusLabel.Size = new Size(139, 13);
            connectionStatusLabel.TabIndex = 4;
            connectionStatusLabel.Text = "Database: Connecting...";
            // 
            // startStopButton
            // 
            startStopButton.BackColor = Color.FromArgb(52, 152, 219);
            startStopButton.FlatStyle = FlatStyle.Flat;
            startStopButton.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            startStopButton.ForeColor = Color.White;
            startStopButton.Location = new Point(18, 23);
            startStopButton.Margin = new Padding(4, 3, 4, 3);
            startStopButton.Name = "startStopButton";
            startStopButton.Size = new Size(139, 37);
            startStopButton.TabIndex = 0;
            startStopButton.Text = "▶ Start";
            startStopButton.UseVisualStyleBackColor = false;
            startStopButton.Click += StartStopButton_Click;
            // 
            // settingsButton
            // 
            settingsButton.Location = new Point(164, 23);
            settingsButton.Margin = new Padding(4, 3, 4, 3);
            settingsButton.Name = "settingsButton";
            settingsButton.Size = new Size(140, 37);
            settingsButton.TabIndex = 3;
            settingsButton.Text = "⚙️ Settings";
            settingsButton.UseVisualStyleBackColor = true;
            settingsButton.Click += SettingsButton_Click;
            // 
            // exportButton
            // 
            exportButton.BackColor = Color.FromArgb(155, 89, 182);
            exportButton.FlatStyle = FlatStyle.Flat;
            exportButton.ForeColor = Color.White;
            exportButton.Location = new Point(315, 23);
            exportButton.Margin = new Padding(4, 3, 4, 3);
            exportButton.Name = "exportButton";
            exportButton.Size = new Size(136, 37);
            exportButton.TabIndex = 6;
            exportButton.Text = "📥 Export";
            exportButton.UseVisualStyleBackColor = false;
            exportButton.Click += ExportButton_Click;
            // 
            // dateLabel
            // 
            dateLabel.AutoSize = true;
            dateLabel.Location = new Point(315, 31);
            dateLabel.Margin = new Padding(4, 0, 4, 0);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new Size(34, 15);
            dateLabel.TabIndex = 2;
            dateLabel.Text = "Date:";
            // 
            // dateTimePicker
            // 
            dateTimePicker.Format = DateTimePickerFormat.Short;
            dateTimePicker.Location = new Point(362, 28);
            dateTimePicker.Margin = new Padding(4, 3, 4, 3);
            dateTimePicker.Name = "dateTimePicker";
            dateTimePicker.Size = new Size(139, 23);
            dateTimePicker.TabIndex = 3;
            // 
            // searchLabel
            // 
            searchLabel.AutoSize = true;
            searchLabel.Location = new Point(18, 31);
            searchLabel.Margin = new Padding(4, 0, 4, 0);
            searchLabel.Name = "searchLabel";
            searchLabel.Size = new Size(88, 15);
            searchLabel.TabIndex = 0;
            searchLabel.Text = "Order No / HN:";
            // 
            // searchTextBox
            // 
            searchTextBox.Location = new Point(117, 28);
            searchTextBox.Margin = new Padding(4, 3, 4, 3);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new Size(174, 23);
            searchTextBox.TabIndex = 1;
            // 
            // searchButton
            // 
            searchButton.BackColor = Color.FromArgb(46, 204, 113);
            searchButton.FlatStyle = FlatStyle.Flat;
            searchButton.ForeColor = Color.White;
            searchButton.Location = new Point(525, 24);
            searchButton.Margin = new Padding(4, 3, 4, 3);
            searchButton.Name = "searchButton";
            searchButton.Size = new Size(105, 30);
            searchButton.TabIndex = 4;
            searchButton.Text = "🔍 Search";
            searchButton.UseVisualStyleBackColor = false;
            searchButton.Click += SearchButton_Click;
            // 
            // refreshButton
            // 
            refreshButton.Location = new Point(642, 24);
            refreshButton.Margin = new Padding(4, 3, 4, 3);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(105, 30);
            refreshButton.TabIndex = 5;
            refreshButton.Text = "🔄 Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += RefreshButton_Click;
            // 
            // totalPanel
            // 
            totalPanel.BackColor = Color.White;
            totalPanel.BorderStyle = BorderStyle.FixedSingle;
            totalPanel.Controls.Add(totalLabel);
            totalPanel.Controls.Add(totalCountLabel);
            totalPanel.Cursor = Cursors.Hand;
            totalPanel.Location = new Point(23, 25);
            totalPanel.Margin = new Padding(4, 3, 4, 3);
            totalPanel.Name = "totalPanel";
            totalPanel.Size = new Size(210, 69);
            totalPanel.TabIndex = 0;
            totalPanel.Click += TotalPanel_Click;
            // 
            // totalLabel
            // 
            totalLabel.Font = new Font("Tahoma", 8F);
            totalLabel.ForeColor = Color.Gray;
            totalLabel.Location = new Point(12, 12);
            totalLabel.Margin = new Padding(4, 0, 4, 0);
            totalLabel.Name = "totalLabel";
            totalLabel.Size = new Size(187, 18);
            totalLabel.TabIndex = 0;
            totalLabel.Text = "จำนวนรายการทั้งหมด";
            // 
            // totalCountLabel
            // 
            totalCountLabel.Font = new Font("Tahoma", 20F, FontStyle.Bold);
            totalCountLabel.ForeColor = Color.Black;
            totalCountLabel.Location = new Point(12, 30);
            totalCountLabel.Margin = new Padding(4, 0, 4, 0);
            totalCountLabel.Name = "totalCountLabel";
            totalCountLabel.Size = new Size(187, 35);
            totalCountLabel.TabIndex = 1;
            totalCountLabel.Text = "0";
            totalCountLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // successPanel
            // 
            successPanel.BackColor = Color.White;
            successPanel.BorderStyle = BorderStyle.FixedSingle;
            successPanel.Controls.Add(successLabel);
            successPanel.Controls.Add(successCountLabel);
            successPanel.Cursor = Cursors.Hand;
            successPanel.Location = new Point(251, 25);
            successPanel.Margin = new Padding(4, 3, 4, 3);
            successPanel.Name = "successPanel";
            successPanel.Size = new Size(210, 69);
            successPanel.TabIndex = 1;
            successPanel.Click += SuccessPanel_Click;
            // 
            // successLabel
            // 
            successLabel.Font = new Font("Tahoma", 8F);
            successLabel.ForeColor = Color.Gray;
            successLabel.Location = new Point(12, 12);
            successLabel.Margin = new Padding(4, 0, 4, 0);
            successLabel.Name = "successLabel";
            successLabel.Size = new Size(187, 18);
            successLabel.TabIndex = 0;
            successLabel.Text = "รายการส่งสำเร็จ";
            // 
            // successCountLabel
            // 
            successCountLabel.Font = new Font("Tahoma", 20F, FontStyle.Bold);
            successCountLabel.ForeColor = Color.Green;
            successCountLabel.Location = new Point(12, 30);
            successCountLabel.Margin = new Padding(4, 0, 4, 0);
            successCountLabel.Name = "successCountLabel";
            successCountLabel.Size = new Size(187, 35);
            successCountLabel.TabIndex = 1;
            successCountLabel.Text = "0";
            successCountLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // failedPanel
            // 
            failedPanel.BackColor = Color.White;
            failedPanel.BorderStyle = BorderStyle.FixedSingle;
            failedPanel.Controls.Add(failedLabel);
            failedPanel.Controls.Add(failedCountLabel);
            failedPanel.Cursor = Cursors.Hand;
            failedPanel.Location = new Point(478, 25);
            failedPanel.Margin = new Padding(4, 3, 4, 3);
            failedPanel.Name = "failedPanel";
            failedPanel.Size = new Size(210, 69);
            failedPanel.TabIndex = 2;
            failedPanel.Click += FailedPanel_Click;
            // 
            // failedLabel
            // 
            failedLabel.Font = new Font("Tahoma", 8F);
            failedLabel.ForeColor = Color.Gray;
            failedLabel.Location = new Point(12, 12);
            failedLabel.Margin = new Padding(4, 0, 4, 0);
            failedLabel.Name = "failedLabel";
            failedLabel.Size = new Size(187, 18);
            failedLabel.TabIndex = 0;
            failedLabel.Text = "รายการล้มเหลว";
            // 
            // failedCountLabel
            // 
            failedCountLabel.Font = new Font("Tahoma", 20F, FontStyle.Bold);
            failedCountLabel.ForeColor = Color.Red;
            failedCountLabel.Location = new Point(12, 30);
            failedCountLabel.Margin = new Padding(4, 0, 4, 0);
            failedCountLabel.Name = "failedCountLabel";
            failedCountLabel.Size = new Size(187, 35);
            failedCountLabel.TabIndex = 1;
            failedCountLabel.Text = "0";
            failedCountLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(statusLabel);
            groupBox1.Controls.Add(lastCheckLabel);
            groupBox1.Controls.Add(lastFoundLabel);
            groupBox1.Controls.Add(lastSuccessLabel);
            groupBox1.Controls.Add(connectionStatusLabel);
            groupBox1.Location = new Point(18, 14);
            groupBox1.Margin = new Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 3, 4, 3);
            groupBox1.Size = new Size(818, 115);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "📊 Status Information";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(startStopButton);
            groupBox2.Controls.Add(settingsButton);
            groupBox2.Controls.Add(exportButton);
            groupBox2.Location = new Point(18, 136);
            groupBox2.Margin = new Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 3, 4, 3);
            groupBox2.Size = new Size(818, 72);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "🎮 Service Controls";
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.Controls.Add(searchLabel);
            groupBox3.Controls.Add(searchTextBox);
            groupBox3.Controls.Add(dateLabel);
            groupBox3.Controls.Add(dateTimePicker);
            groupBox3.Controls.Add(searchButton);
            groupBox3.Controls.Add(refreshButton);
            groupBox3.Location = new Point(18, 215);
            groupBox3.Margin = new Padding(4, 3, 4, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 3, 4, 3);
            groupBox3.Size = new Size(818, 69);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "🔍 Search & Filter";
            // 
            // groupBox4
            // 
            groupBox4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox4.Controls.Add(totalPanel);
            groupBox4.Controls.Add(successPanel);
            groupBox4.Controls.Add(failedPanel);
            groupBox4.Location = new Point(18, 291);
            groupBox4.Margin = new Padding(4, 3, 4, 3);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(4, 3, 4, 3);
            groupBox4.Size = new Size(818, 110);
            groupBox4.TabIndex = 3;
            groupBox4.TabStop = false;
            groupBox4.Text = "📈 Status Summary";
            // 
            // dataGridView
            // 
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Location = new Point(14, 414);
            dataGridView.Margin = new Padding(4, 3, 4, 3);
            dataGridView.Name = "dataGridView";
            dataGridView.ReadOnly = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowHeadersWidth = 51;
            dataGridView.RowTemplate.Height = 24;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.Size = new Size(822, 277);
            dataGridView.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.FromArgb(240, 240, 240);
            ClientSize = new Size(849, 705);
            Controls.Add(dataGridView);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MaximumSize = new Size(865, 744);
            MinimumSize = new Size(865, 744);
            Name = "Form1";
            StartPosition = FormStartPosition.Manual;
            Text = "Interface Pataya - Drug Dispense Monitor";
            totalPanel.ResumeLayout(false);
            successPanel.ResumeLayout(false);
            failedPanel.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            ResumeLayout(false);

        }

        #endregion

        // Status Zone
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label lastCheckLabel;
        private System.Windows.Forms.Label lastFoundLabel;
        private System.Windows.Forms.Label lastSuccessLabel;
        private System.Windows.Forms.Label connectionStatusLabel;

        // Controls Zone
        private System.Windows.Forms.Button startStopButton;
        private System.Windows.Forms.Button settingsButton;
        private System.Windows.Forms.Button exportButton;

        // Search & Filter Zone
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Label dateLabel;
        private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.Button refreshButton;

        // Summary Zone
        private System.Windows.Forms.Panel totalPanel;
        private System.Windows.Forms.Label totalLabel;
        private System.Windows.Forms.Label totalCountLabel;
        private System.Windows.Forms.Panel successPanel;
        private System.Windows.Forms.Label successLabel;
        private System.Windows.Forms.Label successCountLabel;
        private System.Windows.Forms.Panel failedPanel;
        private System.Windows.Forms.Label failedLabel;
        private System.Windows.Forms.Label failedCountLabel;



        // GroupBoxes
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;

        // Data Zone
        private System.Windows.Forms.DataGridView dataGridView;

    }
}
