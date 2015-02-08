using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using Common;
using iTuneServiceManager.Localization;

namespace iTuneServiceManager
{
    public partial class MainForm : Form
    {
		private readonly Logger _logger = Logger.Instance;

        private bool _setupComplete = false;

        public bool iTunesFound
        {
            get { return !string.IsNullOrWhiteSpace(iTunesPathBox.Text) && File.Exists(iTunesPathBox.Text); }
        }
        public bool PasswordsMatch
        {
            get { return passwordBox1.Text != "" && passwordBox2.Text != "" && passwordBox1.Text == passwordBox2.Text; }
        }
        private DomainAuthCredentials CurrentCredentials
        {
            get
            {
                if (!PasswordsMatch || string.IsNullOrWhiteSpace(usernameBox.Text)) return null;

                return new DomainAuthCredentials(
                    (string.IsNullOrWhiteSpace(computerNameBox.Text) ? "." : computerNameBox.Text.Trim()) + "\\" + usernameBox.Text.Trim(),
                    passwordBox1.Text);
            }
        }

        public MainForm()
        {
            _logger.Log("MainForm.InitializeComponent()");
            InitializeComponent();
            ServiceManager.StateChanged += OnServiceManagerStateChanged;
        }

        public void OnLoad(object sender, EventArgs e)
        {
            string programFilesDir;
            if ((programFilesDir = Environment.GetEnvironmentVariable("ProgramFiles(x86)")) == null) programFilesDir = Environment.GetEnvironmentVariable("ProgramFiles");

            var savedCreds = Credentials.RetrieveCredential();
            var startDomain = Environment.MachineName;
            var startUser = Environment.UserName;
            var startPassword = string.Empty;

            // See who's running the current service and look up his credentials if possible
            var currentUser = Service.GetServiceUsername();
            if (currentUser != null)
            {
                startDomain = currentUser.Domain == "." ? Environment.MachineName : currentUser.Domain;
                startUser = currentUser.Username;
                if (savedCreds != null && currentUser.UserEquals(savedCreds.Item1))
                {
                    startPassword = savedCreds.Item2;
                }
                else
                {
                    if (savedCreds != null) Credentials.RemoveCredential();
                    startPassword = string.Empty;
                }
            }

            _logger.Log("programFilesDir: " + programFilesDir);
            
            // Get list of installed users for our combobox
            var localCreds = DomainAuthCredentials.GetLocalUsers();
            
            // Add list to combobox and select start user if there
            usernameBox.Items.AddRange(localCreds.ToArray());
            var userToSelect = localCreds.FirstOrDefault(c => c.UserEquals(startDomain + "\\" + startUser));
            if (userToSelect != null)
            {
                usernameBox.SelectedItem = userToSelect;
            }
            else
            {
                computerNameBox.Text = startDomain;
                usernameBox.Text = startUser;
            }
            passwordBox1.Text = startPassword;
            passwordBox2.Text = startPassword;

            _logger.Log("computerNameBox.Text: " + computerNameBox.Text);
            _logger.Log("Environment.UserName: " + Environment.UserName);

            // Initialize the state as setup until we can check
            ServiceManager.CurrentState = ServiceManager.State.Setup;

            // Use currently set iTunes path or try to detect
            var currentiTunesPath = ServiceManager.GetServiceiTunesPath();
            if (currentiTunesPath != null)
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(currentiTunesPath);
                iTunesPathBox.Text = currentiTunesPath;
            }
            else
            {
                if (File.Exists(programFilesDir + @"\iTunes\iTunes.exe"))
                {
                    _logger.Log("iTunes Directory: " + programFilesDir + @"\iTunes\iTunes.exe");
                    iTunesPathBox.Text = programFilesDir + @"\iTunes\iTunes.exe";
                    openFileDialog1.InitialDirectory = programFilesDir + @"\iTunes\iTunes.exe";
                }
                else
                {
                    if (programFilesDir != null) openFileDialog1.InitialDirectory = programFilesDir;
                    _logger.Log("iTunes Directory: Not Found");
                }
            }

            // Check to see if service is installed / running
            if (Service.IsServiceInstalled)
            {
                try
                {
                    var serviceStatus = Service.ServiceStatus;
                    _logger.Log("Service Status: " + serviceStatus);

                    ServiceManager.CurrentState = serviceStatus == ServiceControllerStatus.Stopped
                                                      ? ServiceManager.State.ServiceStopped
                                                      : ServiceManager.State.ServiceRunning;
                }
                catch (Exception ex)
                {
                    _logger.Log("Error while getting service status.");
                    _logger.Log(ex);
                }
            }

            // Manually trigger actions that fire on app events
            CheckSetupComplete();
            OnServiceManagerStateChanged(null, ServiceManager.CurrentState);
        }

        public void CheckSetupComplete()
        {
            var isComplete = iTunesFound && PasswordsMatch;

            if (isComplete && (ServiceManager.CurrentState == ServiceManager.State.Setup))
            {
                ServiceManager.CurrentState = ServiceManager.State.SetupComplete;
            }
            else if (!isComplete && (ServiceManager.CurrentState == ServiceManager.State.SetupComplete))
            {
                ServiceManager.CurrentState = ServiceManager.State.Setup;
            }

            _setupComplete = isComplete;
        }

        private void OnServiceManagerStateChanged(object sender, ServiceManager.State state)
        {
            if (InvokeRequired)
                Invoke(new Action(() => HandleStateChange(state)));
            else
                HandleStateChange(state);
        }
        private void HandleStateChange(ServiceManager.State state)
        {
            switch (state)
            {
                case ServiceManager.State.Setup:
                case ServiceManager.State.SetupComplete:
                    selectITunesExeBtn.Enabled = true;
                    computerNameBox.Enabled = true;
                    usernameBox.Enabled = true;
                    passwordBox1.Enabled = true;
                    passwordBox2.Enabled = true;
                    pictureBox1.Visible = (state == ServiceManager.State.SetupComplete);
                    installBtn.Enabled = (state == ServiceManager.State.SetupComplete);
                    UninstallBtn.Enabled = false;
                    startBtn.Enabled = false;
                    startBtn.Text = MainFormStrings.ACTION_START;
                    openITunes.Enabled = false;
                    emptyRecycleBinButton.Enabled = false;
                    if (state == ServiceManager.State.Setup) CheckSetupComplete();
                    break;
                case ServiceManager.State.Installing:
                case ServiceManager.State.Uninstalling:
                    selectITunesExeBtn.Enabled = false;
                    computerNameBox.Enabled = false;
                    usernameBox.Enabled = false;
                    passwordBox1.Enabled = false;
                    passwordBox2.Enabled = false;
                    pictureBox1.Visible = (state == ServiceManager.State.Installing);
                    installBtn.Enabled = false;
                    UninstallBtn.Enabled = false;
                    startBtn.Enabled = false;
                    startBtn.Text = MainFormStrings.ACTION_START;
                    openITunes.Enabled = false;
                    emptyRecycleBinButton.Enabled = false;
                    break;
                case ServiceManager.State.ServiceRunning:
                    selectITunesExeBtn.Enabled = false;
                    computerNameBox.Enabled = false;
                    usernameBox.Enabled = false;
                    passwordBox1.Enabled = false;
                    passwordBox2.Enabled = false;
                    pictureBox1.Visible = false;
                    installBtn.Enabled = false;
                    UninstallBtn.Enabled = true;
                    startBtn.Enabled = true;
                    startBtn.Text = MainFormStrings.ACTION_STOP;
                    openITunes.Enabled = false;
                    emptyRecycleBinButton.Enabled = true;
                    break;
                case ServiceManager.State.ServiceStopped:
                    selectITunesExeBtn.Enabled = false;
                    computerNameBox.Enabled = false;
                    usernameBox.Enabled = false;
                    passwordBox1.Enabled = false;
                    passwordBox2.Enabled = false;
                    pictureBox1.Visible = false;
                    installBtn.Enabled = false;
                    UninstallBtn.Enabled = true;
                    startBtn.Enabled = true;
                    startBtn.Text = MainFormStrings.ACTION_START;
                    openITunes.Enabled = true;
                    emptyRecycleBinButton.Enabled = true;
                    break;
                default:
                    if (Debugger.IsAttached) Debugger.Break();
                    break;
            }
        }

        private void OnSelectITunesExeBtnClick(object sender, EventArgs e)
        {
            var r = openFileDialog1.ShowDialog();
            if (r != DialogResult.OK) return;

            iTunesPathBox.Text = openFileDialog1.FileName;
            CheckSetupComplete();
        }

        private void OnUsernameBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (usernameBox.SelectedIndex >= 0)
            {
                var selected = (DomainAuthCredentials)usernameBox.Items[usernameBox.SelectedIndex];
                computerNameBox.Text = selected.Domain;
                BeginInvoke(new Action(() => usernameBox.Text = selected.Username)); // http://stackoverflow.com/a/1052762
            }
        }
        private void OnPasswordBoxesKeyUp(object sender, KeyEventArgs e)
        {
            CheckSetupComplete();

            if (PasswordsMatch)
            {
                InfoLbl.Text = MainFormStrings.INFO_LABEL_VPASS;
            }
        }

        private void OnInstallBtnClick(object sender, EventArgs e)
        {
            Visible = false;

            var box = new InstallWin(this, CurrentCredentials) { Visible = true, Owner = this };

            HideTitleBarItems(box.Handle);
        }
        private void OnUninstallBtnClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to uninstall the iTuneServer service?", "Uninstall Service", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Visible = false;

                var box = new UninstallWin(this) {Owner = this, Visible = true};

                HideTitleBarItems(box.Handle);
            }
        }
        private static void HideTitleBarItems(IntPtr windowHandle)
        {
            IntPtr hmenu = Win32.GetSystemMenu(windowHandle, 0);
            int cnt = Win32.GetMenuItemCount(hmenu);

            // remove 'close' action
            Win32.RemoveMenu(hmenu, cnt - 1, Win32.MF_DISABLED | Win32.MF_BYPOSITION);

            // remove extra menu line
            Win32.RemoveMenu(hmenu, cnt - 2, Win32.MF_DISABLED | Win32.MF_BYPOSITION);

            Win32.DrawMenuBar(windowHandle);
        }
        
        private void OnStartBtnClick(object sender, EventArgs e)
        {
            startBtn.Enabled = false;

            var shouldStart = (startBtn.Text == MainFormStrings.ACTION_START);

            while (true)
            {
                try
                {
                    if (shouldStart)
                    {
                        ServiceManager.StartService();
                        InfoLbl.Text = "The iTuneServer Service is now running";
                    }
                    else
                    {
                        ServiceManager.StopService();
                        InfoLbl.Text = "The iTuneServer Service has stopped running";
                    }
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Log(ex);
                    var answer = MessageBox.Show(this,
                                                 ex.Message,
                                                 string.Format("Error {0} Service", shouldStart ? "Starting" : "Stopping"),
                                                 MessageBoxButtons.RetryCancel,
                                                 MessageBoxIcon.Error);
                    if (answer == DialogResult.Cancel) break;
                }
            }
        }
        private void OnOpenITunesClick(object sender, EventArgs e)
        {
            if (!_setupComplete)
            {
                MessageBox.Show(this,
                                "There is not enough information to run iTunes. Please uninstall and reinstall the service.",
                                "Not Enough Information",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            var p = new Process();
            
            var creds = CurrentCredentials;
            p.StartInfo.Domain = creds.Domain;
            p.StartInfo.UserName = creds.Username;
            p.StartInfo.Password = creds.GetPasswordAsSecureString();

            p.StartInfo.FileName = iTunesPathBox.Text;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(iTunesPathBox.Text);
            p.StartInfo.ErrorDialog = true;
            p.StartInfo.LoadUserProfile = true;

            p.Start();
        }

        #region Mouse Over Text Handling

        private void OnControlsMouseLeave(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_DEFAULT;
        }

        private void OnSelectITunesExeBtnMouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_SELECT;
        }
        private void OnComputerNameBoxMouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_COMPUTER;
        }
        private void OnUsernameBoxMouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_USER;
        }
        private void OnPasswordBox1MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_PASS;
        }
        private void OnPasswordBox2MouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_RPASS;
        }
        private void OnInstallBtnMouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_INSTALL;
        }
        private void OnUninstallBtnMouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_UNINSTALL;
        }
        private void OnStartBtnMouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = startBtn.Text == MainFormStrings.ACTION_START
                               ? MainFormStrings.INFO_LABEL_START
                               : MainFormStrings.INFO_LABEL_STOP;
        }
        private void OnOpenITunesMouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_OPEN;
        }
        private void OnEmptyRecycleBinButtonMouseEnter(object sender, EventArgs e)
        {
            InfoLbl.Text = MainFormStrings.INFO_LABEL_EMPTY_RECYCLE_BIN;
        }

        #endregion

        private void OnEmptyRecycleBinButtonClick(object sender, EventArgs e)
        {
            if (!_setupComplete)
            {
                MessageBox.Show(this,
                                "All information must be entered to be able to empty the given user's recycle bin.",
                                "Not Enough Information",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            var p = new Process();

            var creds = CurrentCredentials;
            p.StartInfo.Domain = creds.Domain;
            p.StartInfo.UserName = creds.Username;
            p.StartInfo.Password = creds.GetPasswordAsSecureString();

            p.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmptyRecycleBin.exe");
            p.StartInfo.Arguments = EmptyRecycleBin.Program.EmptyCommand;
            p.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.ErrorDialog = true;
            p.StartInfo.LoadUserProfile = true;

            p.Start();

            p.WaitForExit(20000);
            if (p.HasExited)
            {
                if (p.ExitCode != 0)
                {
                    MessageBox.Show(this,
                                    string.Format("Emptying the recycle bin gave a funny return code ({0:x8}) and may not have worked correctly.", p.ExitCode),
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(this,
                                    "Successfully emptied the recycle bin!",
                                    "Success",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);

                }
            }
            else
            {
                try { p.Kill(); }
                catch (Exception ex)
                {
                    _logger.Log(ex);
                }
                MessageBox.Show(this,
                                "Timed out waiting for the recycle bin to empty.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

            }
        }
    }
}
