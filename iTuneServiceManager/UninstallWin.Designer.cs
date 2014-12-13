namespace iTuneServiceManager
{
    partial class UninstallWin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UninstallWin));
            this.UninstalliTuneServiceLbl = new System.Windows.Forms.Label();
            this.StoppingiTuneServiceLbl = new System.Windows.Forms.Label();
            this.UninstalliTuneServiceTick = new System.Windows.Forms.Label();
            this.StoppingiTuneServiceTick = new System.Windows.Forms.Label();
            this.SuccessLbl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // UninstalliTuneServiceLbl
            // 
            this.UninstalliTuneServiceLbl.AutoSize = true;
            this.UninstalliTuneServiceLbl.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UninstalliTuneServiceLbl.ForeColor = System.Drawing.SystemColors.Highlight;
            this.UninstalliTuneServiceLbl.Location = new System.Drawing.Point(44, 65);
            this.UninstalliTuneServiceLbl.Name = "UninstalliTuneServiceLbl";
            this.UninstalliTuneServiceLbl.Size = new System.Drawing.Size(219, 17);
            this.UninstalliTuneServiceLbl.TabIndex = 2;
            this.UninstalliTuneServiceLbl.Text = "●  Uninstalling iTuneServer Service";
            // 
            // StoppingiTuneServiceLbl
            // 
            this.StoppingiTuneServiceLbl.AutoSize = true;
            this.StoppingiTuneServiceLbl.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StoppingiTuneServiceLbl.ForeColor = System.Drawing.SystemColors.Highlight;
            this.StoppingiTuneServiceLbl.Location = new System.Drawing.Point(44, 30);
            this.StoppingiTuneServiceLbl.Name = "StoppingiTuneServiceLbl";
            this.StoppingiTuneServiceLbl.Size = new System.Drawing.Size(205, 17);
            this.StoppingiTuneServiceLbl.TabIndex = 3;
            this.StoppingiTuneServiceLbl.Text = "●  Stopping iTuneServer Service";
            // 
            // UninstalliTuneServiceTick
            // 
            this.UninstalliTuneServiceTick.AutoSize = true;
            this.UninstalliTuneServiceTick.Font = new System.Drawing.Font("Century Gothic", 15.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UninstalliTuneServiceTick.ForeColor = System.Drawing.SystemColors.Highlight;
            this.UninstalliTuneServiceTick.Location = new System.Drawing.Point(337, 58);
            this.UninstalliTuneServiceTick.Name = "UninstalliTuneServiceTick";
            this.UninstalliTuneServiceTick.Size = new System.Drawing.Size(33, 25);
            this.UninstalliTuneServiceTick.TabIndex = 6;
            this.UninstalliTuneServiceTick.Text = "✔";
            this.UninstalliTuneServiceTick.Visible = false;
            // 
            // StoppingiTuneServiceTick
            // 
            this.StoppingiTuneServiceTick.AutoSize = true;
            this.StoppingiTuneServiceTick.Font = new System.Drawing.Font("Century Gothic", 15.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StoppingiTuneServiceTick.ForeColor = System.Drawing.SystemColors.Highlight;
            this.StoppingiTuneServiceTick.Location = new System.Drawing.Point(337, 23);
            this.StoppingiTuneServiceTick.Name = "StoppingiTuneServiceTick";
            this.StoppingiTuneServiceTick.Size = new System.Drawing.Size(33, 25);
            this.StoppingiTuneServiceTick.TabIndex = 7;
            this.StoppingiTuneServiceTick.Text = "✔";
            this.StoppingiTuneServiceTick.Visible = false;
            // 
            // SuccessLbl
            // 
            this.SuccessLbl.Font = new System.Drawing.Font("Century Gothic", 14F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SuccessLbl.ForeColor = System.Drawing.SystemColors.Highlight;
            this.SuccessLbl.Location = new System.Drawing.Point(186, 100);
            this.SuccessLbl.Name = "SuccessLbl";
            this.SuccessLbl.Size = new System.Drawing.Size(184, 23);
            this.SuccessLbl.TabIndex = 8;
            this.SuccessLbl.Text = "SUCCESS!";
            this.SuccessLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.SuccessLbl.Visible = false;
            // 
            // UninstallWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 137);
            this.Controls.Add(this.SuccessLbl);
            this.Controls.Add(this.StoppingiTuneServiceTick);
            this.Controls.Add(this.UninstalliTuneServiceTick);
            this.Controls.Add(this.StoppingiTuneServiceLbl);
            this.Controls.Add(this.UninstalliTuneServiceLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UninstallWin";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Uninstalling...";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label UninstalliTuneServiceTick;
        public System.Windows.Forms.Label UninstalliTuneServiceLbl;
        public System.Windows.Forms.Label StoppingiTuneServiceLbl;
        public System.Windows.Forms.Label StoppingiTuneServiceTick;
        public System.Windows.Forms.Label SuccessLbl;
    }
}