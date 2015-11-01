using System;
using System.Diagnostics;
using System.Drawing;
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
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(MainForm));
        private readonly ToolStripMenuItem _serviceControlMenuItem;
        private readonly ToolStripMenuItem _runInteractiveMenuItem;
        private readonly NotifyIcon _trayIcon;
        private readonly Options _options;
        private bool _startupComplete;
        private bool _allowMakeVisible;
        private bool _suppressNextVisible;
        private bool _showMinimizedToTrayNotice;
        private bool _wasVisibleWhenRunInteractive;
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

                var domain = string.IsNullOrWhiteSpace(computerNameBox.Text) ? "." : computerNameBox.Text.Trim();

                string username;

                if (usernameBox.SelectedIndex >= 0)
                {
                    var selected = (DomainAuthCredentials)usernameBox.Items[usernameBox.SelectedIndex];
                    username = selected.Username;
                }
                else
                {
                    username = usernameBox.Text.Trim();
                }

                return new DomainAuthCredentials(domain + "\\" + username, passwordBox1.Text);
            }
        }

        public MainForm(Options commandOptions)
        {
            _options = commandOptions;

            // This will prevent the form from showing when the application is first run
            _allowMakeVisible = !_options.Minimized;

            // See if we should install / uninstall the service
            var shouldInstallService = _options.InstallService && !Service.IsServiceInstalled && _options.HasFullInstallInfo;
            var shouldUninstallService = _options.UninstallService && Service.IsServiceInstalled;

            // This will suppress the next visibility call so the main window won't be visible while installing/uninstalling
            _suppressNextVisible = shouldInstallService || shouldUninstallService;

            // This will show a notice when the app is minimized to the tray the first time
            _showMinimizedToTrayNotice = !_options.Minimized;

            _logger.Info("MainForm.InitializeComponent()");
            InitializeComponent();

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            this._trayIcon = new NotifyIcon();
            this._trayIcon.Text = "iTuneService Manager";
            this._trayIcon.Icon = Properties.Resources.iTunes;
            this._trayIcon.Visible = _options.Minimized;
            this._trayIcon.DoubleClick += ( sender, args ) =>
                                          {
                                              _allowMakeVisible = true;
                                              this.Visible = true;
                                              this.Activate();
                                          };

            // Create menu items that we'll need to keep track of
            _serviceControlMenuItem = new ToolStripMenuItem("Start/Stop Service", null, OnStartBtnClick);
            _runInteractiveMenuItem = new ToolStripMenuItem("Run iTunes Interactively", null, OnOpenITunesClick);

            // Create menu and add it to tray icon
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add(_serviceControlMenuItem);
            trayMenu.Items.Add(_runInteractiveMenuItem);
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, OnExit);            
            this._trayIcon.ContextMenuStrip = trayMenu;

            ServiceManager.StateChanged += OnServiceManagerStateChanged;

            // If we're starting minimized or if we have a startup action, we'll skip a lot of the startup actions that happen
            // during OnLoad so call it here to make sure context menu items are in a valid state.
            if (!_allowMakeVisible || _suppressNextVisible)
            {
                _logger.Info("Calling OnLoad because starting minimized");
                OnLoad(null, EventArgs.Empty);
            }

            _startupComplete = true;

            // Perform actions that require elevation, but ignore if we're not elevated
            if (Util.IsRunAsAdmin())
            {
                if (_options.StartService)
                {
                    if (ServiceManager.CurrentState == ServiceManager.State.ServiceStopped) OnStartBtnClick(null, EventArgs.Empty);
                }
                else if (_options.StopService)
                {
                    if (ServiceManager.CurrentState == ServiceManager.State.ServiceRunning) OnStartBtnClick(null, EventArgs.Empty);
                }
                else if (_options.RunInteractive)
                {
                    OnOpenITunesClick(null, EventArgs.Empty);
                }
                else if (shouldInstallService)
                {
                    OnInstallBtnClick(null, EventArgs.Empty);
                }
                else if (shouldUninstallService)
                {
                    StartUninstall();
                }
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void ShowAfterOtherInstanceStarted()
        {
            _allowMakeVisible = true;
            Visible = true;
        }

        protected override void SetVisibleCore(bool value)
        {
            if (value && _suppressNextVisible)
            {
                value = false;
                _suppressNextVisible = false;
            }
            else
            {
                value = _allowMakeVisible && value;
            }
            if (value == Visible) return;

            base.SetVisibleCore(value);
            if (_trayIcon != null && !ServiceManager.InstallingOrUninstalling)
            {
                _trayIcon.Visible = !value;

                if (!value && _showMinimizedToTrayNotice && (ServiceManager.CurrentState != ServiceManager.State.ServiceInterrupted))
                {
                    _trayIcon.ShowBalloonTip(5000, "iTuneService Manager", "This application has been minimized to the tray.", ToolTipIcon.Info);
                    _showMinimizedToTrayNotice = false;
                }
            }
        }

        public void OnLoad( object sender, EventArgs e )
        {
            // Add UAC to appropriate boxes if necessary
            Util.CheckSetUACIcon(installBtn);
            Util.CheckSetUACIcon(UninstallBtn);
            Util.CheckSetUACIcon(startBtn);
            Util.CheckSetUACIcon(_serviceControlMenuItem);
            if (Service.ServiceStatus != ServiceControllerStatus.Stopped)
            {
                // If the service is running we'll need admin privileges to stop
                // the service before running in interactive mode.
                Util.CheckSetUACIcon(_runInteractiveMenuItem);
            }

            // Grab current info
            var programFilesDirs = new[]
                                   {
                                       Environment.GetEnvironmentVariable( "ProgramFiles(x86)" ),
                                       Environment.GetEnvironmentVariable( "ProgramFiles" )
                                   };
            var savedCreds = Credentials.RetrieveCredential();
            var startDomain = Environment.MachineName;
            var startUser = Environment.UserName;
            var startPassword = string.Empty;
            var shouldInstallService = _options.InstallService && !Service.IsServiceInstalled && _options.HasFullInstallInfo;

            // Override starting info from command line if we've got an install command
            if (shouldInstallService)
            {
                savedCreds = null;
                startDomain = _options.Domain;
                startUser = _options.User;
                try
                {
                    startPassword = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(_options.Password));
                }
                catch (Exception)
                {
                    startPassword = _options.Password;
                }
            }

            // See who's running the current service and look up his credentials if possible
            var currentUser = Service.GetServiceUsername();
            if ( currentUser != null )
            {
                startDomain = currentUser.Domain == "." ? Environment.MachineName : currentUser.Domain;
                startUser = currentUser.Username;
                if ( savedCreds != null && currentUser.UserEquals( savedCreds.Item1 ) )
                {
                    startPassword = savedCreds.Item2;
                }
                else
                {
                    if ( savedCreds != null ) Credentials.RemoveCredential();
                    startPassword = string.Empty;
                }
            }

            _logger.Info( "programFilesDirs: " + programFilesDirs.Aggregate( ( s1, s2 ) => s1 + ", " + s2 ) );

            // Get list of installed users for our combobox
            var localCreds = DomainAuthCredentials.GetLocalUsers();

            // Add list to combobox and select start user if there
            if ( localCreds != null )
            {
                if (usernameBox.Items.Count == 0)
                    usernameBox.Items.AddRange( localCreds.Cast<object>().ToArray() );

                var userToSelect = localCreds.FirstOrDefault( c => c.UserEquals( startDomain + "\\" + startUser ) );

                if ( userToSelect != null )
                {
                    usernameBox.SelectedItem = userToSelect;
                    EnsureUsernameBoxTextCorrect(); // this is necessary when the value was set when we started minimized
                }

                passwordBox1.Text = startPassword;
                passwordBox2.Text = startPassword;

                _logger.Info( "computerNameBox.Text: " + computerNameBox.Text );
                _logger.Info( "Environment.UserName: " + Environment.UserName );

                // Initialize the state as setup until we can check
                ServiceManager.CurrentState = ServiceManager.State.Setup;

                // Use currently set iTunes path or try to detect
                var currentiTunesPath = ServiceManager.GetServiceiTunesPath();
                if ( currentiTunesPath != null )
                {
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName( currentiTunesPath );
                    iTunesPathBox.Text = currentiTunesPath;
                }
                else if (shouldInstallService)
                {
                    iTunesPathBox.Text = _options.ITunesPath;
                    openFileDialog1.InitialDirectory = _options.ITunesPath;
                }
                else
                {
                    bool found = false;
                    foreach (var source in programFilesDirs.Where( s => s != null ))
                    {
                        if ( !found && ( found = File.Exists( source + @"\iTunes\iTunes.exe" ) ) )
                        {
                            _logger.Info( "iTunes Directory: " + source + @"\iTunes\iTunes.exe" );
                            iTunesPathBox.Text = source + @"\iTunes\iTunes.exe";
                            openFileDialog1.InitialDirectory = source + @"\iTunes\iTunes.exe";
                        }
                    }
                    if ( !found )
                    {
                        _logger.Error( "iTunes Directory: Not Found" );
                    }
                }

                // Check to see if service is installed / running
                if (Service.IsServiceInstalled)
                {
                    try
                    {
                        var serviceStatus = Service.ServiceStatus;
                        _logger.Info("Service Status: " + serviceStatus);

                        ServiceManager.CurrentState = serviceStatus == ServiceControllerStatus.Stopped
                                                          ? ServiceManager.State.ServiceStopped
                                                          : ServiceManager.State.ServiceRunning;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error while getting service status.", ex);
                    }
                }

                // Manually trigger actions that fire on app events
                CheckSetupComplete();
                OnServiceManagerStateChanged( null, ServiceManager.CurrentState );
            }
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
            // Update tray icon context menu for state changes
            if (ServiceManager.InstallingOrUninstalling) state = ServiceManager.State.Setup;
            switch (state)
            {
                case ServiceManager.State.Setup:
                case ServiceManager.State.SetupComplete:
                    _serviceControlMenuItem.Enabled = false;
                    _serviceControlMenuItem.Text = "<Service Not Installed>";
                    _runInteractiveMenuItem.Visible = false;
                    break;
                case ServiceManager.State.ServiceRunning:
                    _serviceControlMenuItem.Enabled = true;
                    _serviceControlMenuItem.Text = "Stop Service";
                    _runInteractiveMenuItem.Visible = true;
                    if (_startupComplete && !Visible) _trayIcon.ShowBalloonTip(3000, "iTuneService Manager", "Service was started successfully!", ToolTipIcon.Info);
                    break;
                case ServiceManager.State.ServiceStopped:
                    _serviceControlMenuItem.Enabled = true;
                    _serviceControlMenuItem.Text = "Start Service";
                    _runInteractiveMenuItem.Visible = true;
                    if (_startupComplete && !Visible) _trayIcon.ShowBalloonTip(3000, "iTuneService Manager", "Service was stopped!", ToolTipIcon.Info);
                    break;
                case ServiceManager.State.ServiceInterrupted:
                    _serviceControlMenuItem.Enabled = false;
                    _serviceControlMenuItem.Text = "<iTunes is in Interactive Mode>";
                    _runInteractiveMenuItem.Visible = false;
                    break;
                default:
                    if (Debugger.IsAttached) Debugger.Break();
                    break;
            }

            // Update main window UI for state changes
            if (ServiceManager.InstallingOrUninstalling) state = ServiceManager.State.ServiceInterrupted;
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
                case ServiceManager.State.ServiceInterrupted:
                    selectITunesExeBtn.Enabled = false;
                    computerNameBox.Enabled = false;
                    usernameBox.Enabled = false;
                    passwordBox1.Enabled = false;
                    passwordBox2.Enabled = false;
                    pictureBox1.Visible = false;
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
                    openITunes.Enabled = true;
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
                EnsureUsernameBoxTextCorrect();
            }
        }

        private void EnsureUsernameBoxTextCorrect()
        {
            if (!IsHandleCreated) return;
            var selected = (DomainAuthCredentials)usernameBox.Items[usernameBox.SelectedIndex];
            if (usernameBox.Text == selected.Username) return;
            BeginInvoke(new Action(() => usernameBox.Text = selected.Username)); // http://stackoverflow.com/a/1052762
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
            if (!Util.IsRunAsAdmin())
            {
				// Relaunch as admin because this command requires admin privileges
                Util.RelaunchAsAdmin(string.Format("--{0} --{1} \"{2}\" --{3} \"{4}\" --{5} \"{6}\" --{7} \"{8}\"", 
                                                   OptionConstants.InstallService,
                                                   OptionConstants.ITunesPath, iTunesPathBox.Text,
                                                   OptionConstants.Domain, computerNameBox.Text,
                                                   OptionConstants.User, usernameBox.Text,
                                                   OptionConstants.Password, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordBox1.Text))));
                startBtn.Enabled = true;
                return;
            }

            ServiceManager.InstallingOrUninstalling = true;
            
            Visible = false;

            var box = new InstallWin(this, CurrentCredentials) { Visible = true, Owner = this };

            HideTitleBarItems(box.Handle);
        }

        private void OnUninstallBtnClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,
                                "Are you sure you want to uninstall the iTuneServer service?",
                                "Uninstall Service",
                                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                StartUninstall();
            }
        }

        private void StartUninstall()
        {
            if (!Util.IsRunAsAdmin())
            {
                // Relaunch as admin because this command requires admin privileges
                Util.RelaunchAsAdmin(string.Format("--{0}", OptionConstants.UninstallService));
                startBtn.Enabled = true;
                return;
            }

            ServiceManager.InstallingOrUninstalling = true;

            Visible = false;

            var box = new UninstallWin(this) { Owner = this, Visible = true };

            HideTitleBarItems(box.Handle);
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

            if (!Util.IsRunAsAdmin())
            {
				// Relaunch as admin because this command requires admin privileges
                Util.RelaunchAsAdmin(string.Format("--{0} --{1}",
                                                   OptionConstants.Minimized, 
                                                   shouldStart ? OptionConstants.StartService: OptionConstants.StopService));
                startBtn.Enabled = true;
                return;
            }

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
                    _logger.Error(string.Format("Error while {0} service.", shouldStart ? "starting" : "stopping"), ex);
                    var answer = MessageBox.Show(this, ex.Message, string.Format("Error {0} Service", shouldStart ? "Starting" : "Stopping"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    if (answer == DialogResult.Cancel) break;
                }
            }
        }

        private void OnOpenITunesClick(object sender, EventArgs e)
        {
            if (!_setupComplete)
            {
                MessageBox.Show(this, "There is not enough information to run iTunes. Please uninstall and reinstall the service.", "Not Enough Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var interruptingService = ServiceManager.CurrentState == ServiceManager.State.ServiceRunning;

            if (interruptingService)
            {
                if (!Util.IsRunAsAdmin())
                {
                    // Relaunch as admin because this command requires admin privileges
                    Util.RelaunchAsAdmin(string.Format("--{0} --{1}",
                                                       OptionConstants.Minimized, OptionConstants.RunInteractive));
                    startBtn.Enabled = true;
                    return;
                }

                // Attempt to stop service
                OnStartBtnClick(sender, e);

                // Exit if couldn't successfully stop service
                if (ServiceManager.CurrentState == ServiceManager.State.ServiceRunning) return;

                ServiceManager.CurrentState = ServiceManager.State.ServiceInterrupted;

                _wasVisibleWhenRunInteractive = Visible;

                Visible = false;
            }

            var p = new Process();

            p.EnableRaisingEvents = interruptingService;
            p.Exited += RestartServiceOnProcessExit;

            var creds = CurrentCredentials;
            p.StartInfo.Domain = creds.Domain;
            p.StartInfo.UserName = creds.Username;
            p.StartInfo.Password = creds.GetPasswordAsSecureString();

            p.StartInfo.FileName = iTunesPathBox.Text;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(iTunesPathBox.Text);
            p.StartInfo.ErrorDialog = true;
            p.StartInfo.LoadUserProfile = true;

            try
            {
                p.Start();
            }
            catch (Exception ex)
            {
                var msg = string.Format("Error running iTunes as '{0}'", creds.Username);
                _logger.Error(msg, ex);
                MessageBox.Show(this, msg + "\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (interruptingService) RestartServiceOnProcessExit(this, EventArgs.Empty);
            }
        }

        private void RestartServiceOnProcessExit(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RestartServiceOnProcessExit(sender, e)));
                return;
            }

            if (_wasVisibleWhenRunInteractive) Visible = true;

            Activate();

            System.Threading.Thread.Sleep(250);
            OnStartBtnClick(sender, e);
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
            InfoLbl.Text = startBtn.Text == MainFormStrings.ACTION_START ? MainFormStrings.INFO_LABEL_START : MainFormStrings.INFO_LABEL_STOP;
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
                MessageBox.Show(this, "All information must be entered to be able to empty the given user's recycle bin.", "Not Enough Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show(this, string.Format("Emptying the recycle bin gave a funny return code ({0:x8}) and may not have worked correctly.", p.ExitCode), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(this, "Successfully emptied the recycle bin!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                try { p.Kill(); }
                catch (Exception ex)
                {
                    _logger.Error("Error killing empty recycle bin task!", ex);
                }
                MessageBox.Show(this, "Timed out waiting for the recycle bin to empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Visible = false;
                e.Cancel = true;
            }
            else this._trayIcon.Visible = false;
        }
    }
}
