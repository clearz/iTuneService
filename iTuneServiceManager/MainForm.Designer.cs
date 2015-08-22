namespace iTuneServiceManager
{
    partial class MainForm
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
                this._trayIcon.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.GroupBox groupBox2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.usernameBox = new System.Windows.Forms.ComboBox();
            this.iTunesPathBox = new System.Windows.Forms.TextBox();
            this.computerNameBox = new System.Windows.Forms.TextBox();
            this.selectITunesExeBtn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.passwordBox2 = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.passwordBox1 = new System.Windows.Forms.TextBox();
            this.installBtn = new System.Windows.Forms.Button();
            this.UninstallBtn = new System.Windows.Forms.Button();
            this.startBtn = new System.Windows.Forms.Button();
            this.startContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startITunesInInteractiveModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.InfoLbl = new System.Windows.Forms.Label();
            this.openITunes = new System.Windows.Forms.Button();
            this.emptyRecycleBinButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            groupBox2.SuspendLayout();
            this.startContextMenu.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this.label1);
            groupBox1.Controls.Add(this.label2);
            groupBox1.Controls.Add(this.usernameBox);
            groupBox1.Controls.Add(this.iTunesPathBox);
            groupBox1.Controls.Add(this.computerNameBox);
            groupBox1.Controls.Add(this.selectITunesExeBtn);
            groupBox1.Controls.Add(this.label5);
            groupBox1.Controls.Add(this.label3);
            groupBox1.Controls.Add(this.label4);
            groupBox1.Controls.Add(this.passwordBox2);
            groupBox1.Controls.Add(this.pictureBox1);
            groupBox1.Controls.Add(this.passwordBox1);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(517, 154);
            groupBox1.TabIndex = 18;
            groupBox1.TabStop = false;
            groupBox1.Text = "Service Config";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "iTunes Executable (.exe)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Run iTunes as (Account)";
            // 
            // usernameBox
            // 
            this.usernameBox.DropDownWidth = 180;
            this.usernameBox.FormattingEnabled = true;
            this.usernameBox.Location = new System.Drawing.Point(152, 69);
            this.usernameBox.Name = "usernameBox";
            this.usernameBox.Size = new System.Drawing.Size(121, 21);
            this.usernameBox.TabIndex = 2;
            this.usernameBox.SelectedIndexChanged += new System.EventHandler(this.OnUsernameBoxSelectedIndexChanged);
            this.usernameBox.MouseEnter += new System.EventHandler(this.OnUsernameBoxMouseEnter);
            this.usernameBox.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // iTunesPathBox
            // 
            this.iTunesPathBox.Location = new System.Drawing.Point(152, 17);
            this.iTunesPathBox.Name = "iTunesPathBox";
            this.iTunesPathBox.ReadOnly = true;
            this.iTunesPathBox.Size = new System.Drawing.Size(268, 20);
            this.iTunesPathBox.TabIndex = 0;
            this.iTunesPathBox.TabStop = false;
            // 
            // computerNameBox
            // 
            this.computerNameBox.Location = new System.Drawing.Point(152, 43);
            this.computerNameBox.Name = "computerNameBox";
            this.computerNameBox.Size = new System.Drawing.Size(121, 20);
            this.computerNameBox.TabIndex = 2;
            this.computerNameBox.Leave += new System.EventHandler(this.OnControlsMouseLeave);
            this.computerNameBox.MouseEnter += new System.EventHandler(this.OnComputerNameBoxMouseEnter);
            this.computerNameBox.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // selectITunesExeBtn
            // 
            this.selectITunesExeBtn.Location = new System.Drawing.Point(426, 15);
            this.selectITunesExeBtn.Name = "selectITunesExeBtn";
            this.selectITunesExeBtn.Size = new System.Drawing.Size(75, 23);
            this.selectITunesExeBtn.TabIndex = 1;
            this.selectITunesExeBtn.Text = "Select";
            this.selectITunesExeBtn.UseVisualStyleBackColor = true;
            this.selectITunesExeBtn.Click += new System.EventHandler(this.OnSelectITunesExeBtnClick);
            this.selectITunesExeBtn.MouseEnter += new System.EventHandler(this.OnSelectITunesExeBtnMouseEnter);
            this.selectITunesExeBtn.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Domain or Computer Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Account Password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 125);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Retype Password";
            // 
            // passwordBox2
            // 
            this.passwordBox2.Location = new System.Drawing.Point(152, 122);
            this.passwordBox2.Name = "passwordBox2";
            this.passwordBox2.PasswordChar = '●';
            this.passwordBox2.Size = new System.Drawing.Size(121, 20);
            this.passwordBox2.TabIndex = 4;
            this.passwordBox2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnPasswordBoxesKeyUp);
            this.passwordBox2.MouseEnter += new System.EventHandler(this.OnPasswordBox2MouseEnter);
            this.passwordBox2.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::iTuneServiceManager.Properties.Resources.tick;
            this.pictureBox1.Location = new System.Drawing.Point(291, 67);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(79, 75);
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // passwordBox1
            // 
            this.passwordBox1.Location = new System.Drawing.Point(152, 96);
            this.passwordBox1.Name = "passwordBox1";
            this.passwordBox1.PasswordChar = '●';
            this.passwordBox1.Size = new System.Drawing.Size(121, 20);
            this.passwordBox1.TabIndex = 3;
            this.passwordBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnPasswordBoxesKeyUp);
            this.passwordBox1.MouseEnter += new System.EventHandler(this.OnPasswordBox1MouseEnter);
            this.passwordBox1.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(this.installBtn);
            groupBox2.Controls.Add(this.UninstallBtn);
            groupBox2.Controls.Add(this.startBtn);
            groupBox2.Location = new System.Drawing.Point(12, 176);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(288, 100);
            groupBox2.TabIndex = 19;
            groupBox2.TabStop = false;
            groupBox2.Text = "Service Control";
            // 
            // installBtn
            // 
            this.installBtn.Enabled = false;
            this.installBtn.Location = new System.Drawing.Point(15, 24);
            this.installBtn.Name = "installBtn";
            this.installBtn.Size = new System.Drawing.Size(121, 30);
            this.installBtn.TabIndex = 5;
            this.installBtn.Text = "Install Service";
            this.installBtn.UseVisualStyleBackColor = true;
            this.installBtn.Click += new System.EventHandler(this.OnInstallBtnClick);
            this.installBtn.MouseEnter += new System.EventHandler(this.OnInstallBtnMouseEnter);
            this.installBtn.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // UninstallBtn
            // 
            this.UninstallBtn.Enabled = false;
            this.UninstallBtn.Location = new System.Drawing.Point(15, 57);
            this.UninstallBtn.Name = "UninstallBtn";
            this.UninstallBtn.Size = new System.Drawing.Size(121, 30);
            this.UninstallBtn.TabIndex = 6;
            this.UninstallBtn.Text = "Uninstall Service";
            this.UninstallBtn.UseVisualStyleBackColor = true;
            this.UninstallBtn.Click += new System.EventHandler(this.OnUninstallBtnClick);
            this.UninstallBtn.MouseEnter += new System.EventHandler(this.OnUninstallBtnMouseEnter);
            this.UninstallBtn.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // startBtn
            // 
            this.startBtn.BackColor = System.Drawing.Color.Transparent;
            this.startBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.startBtn.ContextMenuStrip = this.startContextMenu;
            this.startBtn.Enabled = false;
            this.startBtn.Location = new System.Drawing.Point(182, 39);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(75, 30);
            this.startBtn.TabIndex = 7;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = false;
            this.startBtn.Click += new System.EventHandler(this.OnStartBtnClick);
            this.startBtn.MouseEnter += new System.EventHandler(this.OnStartBtnMouseEnter);
            this.startBtn.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // startContextMenu
            // 
            this.startContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startServiceToolStripMenuItem,
            this.stopServiceToolStripMenuItem,
            this.startITunesInInteractiveModeToolStripMenuItem});
            this.startContextMenu.Name = "startContextMenu";
            this.startContextMenu.Size = new System.Drawing.Size(242, 70);
            // 
            // startServiceToolStripMenuItem
            // 
            this.startServiceToolStripMenuItem.Name = "startServiceToolStripMenuItem";
            this.startServiceToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
            this.startServiceToolStripMenuItem.Text = "Start Service";
            // 
            // stopServiceToolStripMenuItem
            // 
            this.stopServiceToolStripMenuItem.Name = "stopServiceToolStripMenuItem";
            this.stopServiceToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
            this.stopServiceToolStripMenuItem.Text = "Stop Service";
            // 
            // startITunesInInteractiveModeToolStripMenuItem
            // 
            this.startITunesInInteractiveModeToolStripMenuItem.Name = "startITunesInInteractiveModeToolStripMenuItem";
            this.startITunesInInteractiveModeToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
            this.startITunesInInteractiveModeToolStripMenuItem.Text = "Start iTunes in Interactive Mode";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "exe";
            this.openFileDialog1.FileName = "iTunes.exe";
            this.openFileDialog1.Filter = "iTunes | iTunes.exe";
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // InfoLbl
            // 
            this.InfoLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.InfoLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InfoLbl.ForeColor = System.Drawing.SystemColors.Highlight;
            this.InfoLbl.Location = new System.Drawing.Point(9, 294);
            this.InfoLbl.Name = "InfoLbl";
            this.InfoLbl.Size = new System.Drawing.Size(520, 16);
            this.InfoLbl.TabIndex = 16;
            this.InfoLbl.Text = "iTuneServer Windows Service manager - github.com/clearz/iTuneService";
            this.InfoLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // openITunes
            // 
            this.openITunes.Enabled = false;
            this.openITunes.Location = new System.Drawing.Point(33, 24);
            this.openITunes.Name = "openITunes";
            this.openITunes.Size = new System.Drawing.Size(156, 30);
            this.openITunes.TabIndex = 8;
            this.openITunes.Text = "Run iTunes Interactively";
            this.openITunes.UseVisualStyleBackColor = true;
            this.openITunes.Click += new System.EventHandler(this.OnOpenITunesClick);
            this.openITunes.MouseEnter += new System.EventHandler(this.OnOpenITunesMouseEnter);
            this.openITunes.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // emptyRecycleBinButton
            // 
            this.emptyRecycleBinButton.Enabled = false;
            this.emptyRecycleBinButton.Image = global::iTuneServiceManager.Properties.Resources.RecycleBin;
            this.emptyRecycleBinButton.Location = new System.Drawing.Point(33, 57);
            this.emptyRecycleBinButton.Name = "emptyRecycleBinButton";
            this.emptyRecycleBinButton.Size = new System.Drawing.Size(156, 30);
            this.emptyRecycleBinButton.TabIndex = 17;
            this.emptyRecycleBinButton.Text = "Empty Recycle Bin";
            this.emptyRecycleBinButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.emptyRecycleBinButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.emptyRecycleBinButton.UseVisualStyleBackColor = true;
            this.emptyRecycleBinButton.Click += new System.EventHandler(this.OnEmptyRecycleBinButtonClick);
            this.emptyRecycleBinButton.MouseEnter += new System.EventHandler(this.OnEmptyRecycleBinButtonMouseEnter);
            this.emptyRecycleBinButton.MouseLeave += new System.EventHandler(this.OnControlsMouseLeave);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.openITunes);
            this.groupBox3.Controls.Add(this.emptyRecycleBinButton);
            this.groupBox3.Location = new System.Drawing.Point(306, 176);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(223, 100);
            this.groupBox3.TabIndex = 20;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Utility";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(541, 319);
            this.Controls.Add(this.InfoLbl);
            this.Controls.Add(groupBox1);
            this.Controls.Add(groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "iTuneServer Service Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.OnLoad);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            groupBox2.ResumeLayout(false);
            this.startContextMenu.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.Button UninstallBtn;
        public System.Windows.Forms.Button selectITunesExeBtn;
        public System.Windows.Forms.TextBox passwordBox2;
        public System.Windows.Forms.TextBox passwordBox1;
        public System.Windows.Forms.Button installBtn;
        public System.Windows.Forms.ComboBox usernameBox;
        public System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.Button startBtn;
        public System.Windows.Forms.TextBox computerNameBox;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox iTunesPathBox;
        private System.Windows.Forms.Label InfoLbl;
        public System.Windows.Forms.Button openITunes;
        private System.Windows.Forms.ContextMenuStrip startContextMenu;
        private System.Windows.Forms.ToolStripMenuItem startServiceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startITunesInInteractiveModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopServiceToolStripMenuItem;
        public System.Windows.Forms.Button emptyRecycleBinButton;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}

