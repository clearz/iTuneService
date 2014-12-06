using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Web;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace iTuneServiceManager
{
    public partial class MainForm : Form
    {
		private readonly Logger _logger = Logger.GetLogger(writeToConsole: true);

	    bool passwordsMatching = false;

        public bool PasswordsMatching
        {
            get { return passwordsMatching; }
        }
        bool iTunesExeFound = false;

        public bool ITunesExeFound
        {
            get { return iTunesExeFound; }
        }

        public MainForm()
        {
            _logger.Log( "MainForm.InitializeComponent()" );
            InitializeComponent();
            try
            {
                computerNameBox.Text = Environment.GetEnvironmentVariable("COMPUTERNAME") ?? ".";

                _logger.Log("computerNameBox.Text: " + computerNameBox.Text);
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
            // validate the credentials
        }

        public void Form1_Load(object sender, EventArgs e) // TO DO - Call this from UninstallWin's Close method
        {
            string programFilesDir;
            if ((programFilesDir = Environment.GetEnvironmentVariable("ProgramFiles(x86)")) == null)
                programFilesDir = Environment.GetEnvironmentVariable("ProgramFiles");

            _logger.Log("programFilesDir: " + programFilesDir);
            var query = new SelectQuery("Win32_UserAccount");
            var searcher = new ManagementObjectSearcher(query);
			if(!(sender is UninstallWin))
				foreach (ManagementObject envVar in searcher.Get())
					usernameBox.Items.Add(envVar["Name"]);

            usernameBox.SelectedItem = Environment.UserName;

            _logger.Log("Environment.UserName: " + Environment.UserName);
            if (File.Exists(programFilesDir + @"\iTunes\iTunes.exe"))
            {
                _logger.Log("iTunes Directory: " + programFilesDir + @"\iTunes\iTunes.exe");
                iTunesPathBox.Text = programFilesDir + @"\iTunes\iTunes.exe";
                if (ServiceInstalledAlready() && e != null)
                {
                    pictureBox1.Visible = passwordBox1.Enabled = passwordBox2.Enabled = usernameBox.Enabled = selectITunesExeBtn.Enabled = installBtn.Enabled = false;
                    UninstallBtn.Enabled = true;
                    return;
                }
                openITunes.Enabled = true;
                openFileDialog1.InitialDirectory = programFilesDir + @"\iTunes\iTunes.exe";
                iTunesExeFound = true;
                installBtn.Enabled = passwordsMatching;
                pictureBox1.Visible = passwordsMatching;
            }
            else _logger.Log( "iTunes Directory: Not Found" );

            if (!iTunesExeFound)
            {
                if(programFilesDir != null)
                    openFileDialog1.InitialDirectory = programFilesDir;
            }
        }

        private bool ServiceInstalledAlready()
        {
	        if (!ServiceManager.IsServiceInstalled) return false;
            try
            {
                if (ServiceManager.ServiceStatus == ServiceControllerStatus.Stopped)
                {
                    startBtn.Text = "Start";
                }
                else if (ServiceManager.ServiceStatus == ServiceControllerStatus.Running)
                {
                    startBtn.Text = "Stop";
                }
                _logger.Log("ServiceManager.Status: " + ServiceManager.ServiceStatus);
                _logger.Log("startBtn.Text: " + startBtn.Text);
                startBtn.Enabled = true;

                _logger.Log("returning true");
                return true;
            }
            catch(Exception e)
            {
                _logger.Log(e);
                _logger.Log("returning false");
                return false;
            }
        }


        private void installBtn_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            var box = new InstallWin() {Visible = true, Owner = this};
            IntPtr hmenu = Win32.GetSystemMenu(box.Handle, 0);
            int cnt = Win32.GetMenuItemCount(hmenu);

            // remove 'close' action
	        Win32.RemoveMenu(hmenu, cnt - 1, Win32.MF_DISABLED | Win32.MF_BYPOSITION);

            // remove extra menu line
	        Win32.RemoveMenu(hmenu, cnt - 2, Win32.MF_DISABLED | Win32.MF_BYPOSITION);

	        Win32.DrawMenuBar(box.Handle);
        }

        private void selectITunesExeBtn_Click(object sender, EventArgs e)
        {
            DialogResult r = openFileDialog1.ShowDialog();
            if (r == DialogResult.OK)
            {
                iTunesPathBox.Text = openFileDialog1.FileName;
                installBtn.Enabled = passwordsMatching;
                pictureBox1.Visible = passwordsMatching;
                openITunes.Enabled = true;
            }
        }


        private void passwordBox1_KeyUp(object sender, KeyEventArgs e)
        {
            passwordsMatching = (passwordBox1.Text != "" && passwordBox2.Text != "" && passwordBox1.Text == passwordBox2.Text);
            if (passwordsMatching)
            {
                installBtn.Enabled = iTunesExeFound && iTunesPathBox.Text != "";
                pictureBox1.Visible = iTunesExeFound && iTunesPathBox.Text != "";
                InfoLbl.Text = Label.Get["vpass"];
            }
            else
                pictureBox1.Visible = installBtn.Enabled = false;

        }

        private void UninstallBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to uninstall the iTuneServer service?", "Uninstall Service", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Visible = false;
                var box = new UninstallWin(this) {Owner = this, Visible = true};
	            IntPtr hmenu = Win32.GetSystemMenu(box.Handle, 0);
                int cnt = Win32.GetMenuItemCount(hmenu);

                // remove 'close' action
	            Win32.RemoveMenu(hmenu, cnt - 1, Win32.MF_DISABLED | Win32.MF_BYPOSITION);

                // remove extra menu line
	            Win32.RemoveMenu(hmenu, cnt - 2, Win32.MF_DISABLED | Win32.MF_BYPOSITION);

	            Win32.DrawMenuBar(box.Handle);
            }
        }
        private void startBtn_Click(object sender, EventArgs e)
        {
                if (startBtn.Text == "Start")
                {
                    try
                    {
                        ServiceManager.StartService("iTuneServer Service");
                        InfoLbl.Text = "The iTuneServer Service is now running";
                        openITunes.Enabled = false;
                        startBtn.Text = "Stop";
                    }
                    catch (Exception ex)
					{
						_logger.Log(ex);
                        if (MessageBox.Show(this, ex.Message, "Error Starting Service", MessageBoxButtons.RetryCancel,MessageBoxIcon.Error) == DialogResult.Retry)
                         startBtn_Click(sender, e);

                    }
                }
                else
                {
                    try
                    {
                        ServiceManager.StopService("iTuneServer Service");
                        InfoLbl.Text = "The iTuneServer Service has stopped running";
                        openITunes.Enabled = true;
                        startBtn.Text = "Start";
                    }
                    catch (Exception ex)
                    {
						_logger.Log(ex);
                        if (MessageBox.Show(this, ex.Message, "Error Stopping Service", MessageBoxButtons.RetryCancel,MessageBoxIcon.Error) == DialogResult.Retry)
                            startBtn_Click(sender, e);

                    }
                }
        }

        private void computerNameBox_Leave(object sender, EventArgs e)
        {

        }


        private void selectITunesExeBtn_MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = Label.Get["select"];
        }

        private void ITunes_MouseLeave(object sender, EventArgs e)
        {
            InfoLbl.Text = Label.Get["default"];
        }

        private void computerNameBox_MouseEnter(object sender, EventArgs e)
		{
            InfoLbl.Text = Label.Get["computer"];
        }

        private void comboBox1_MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = Label.Get["user"];
        }

        private void passwordBox1_MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = Label.Get["pass"];
        }

        private void passwordBox2_MouseEnter(object sender, EventArgs e)
		{
            InfoLbl.Text = Label.Get["rpass"];
        }

        private void installBtn_MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = Label.Get["install"];
        }

        private void UninstallBtn_MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = Label.Get["uninstall"];
        }

        private void startBtn_MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = startBtn.Text == "Start" ? Label.Get["start"] : Label.Get["stop"];
        }

        private void stopBtn_MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = Label.Get["stop"];
        }

        private void openITunes_MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = Label.Get["open"];
        }

        private void openITunes_Click(object sender, EventArgs e)
        {
            if (iTunesPathBox.Text == "") selectITunesExeBtn_Click(sender, e);
            if (iTunesPathBox.Text == "") return;
            var p = new Process();
            p.StartInfo.FileName = iTunesPathBox.Text;
            p.Start();
        }
    }
}
