using System;
using System.Threading;
using System.IO;
using System.Windows.Forms;

//del $(OutDir)iTuneServiceManager.pdb
//del $(OutDir)iTuneServiceManager.exe.config
//del $(OutDir)iTuneServiceManager.pdb
//del $(OutDir)iTuneServiceManager.exe.config


namespace iTuneServiceManager
{
	public partial class InstallWin : Form
	{
		private readonly Logger _logger = Logger.GetLogger(writeToConsole: true);

	    private MainForm _mainForm;

		public void Invoke(InstallWin i, Action method)
		{
		    Action toInvoke =
		        () =>
		        {
		            try
		            {
		                method();
		            }
		            catch (Exception e)
		            {
		                _logger.Log(e);
		                ShowError("Error @ " + method.GetType().Name);
		            }
		        };

		    if (i.InvokeRequired)
		        i.Invoke(toInvoke);
		    else
		        toInvoke();
		}

        public InstallWin(MainForm mainForm)
		{
			InitializeComponent();
            _mainForm = mainForm;
			new Thread(Install).Start();
		}

		public void ShowError(String err)
		{
			MessageBox.Show(this, err, "Error Installing Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Closed += (sender, args) => { Thread.Sleep(10); _mainForm.Visible = true; }; 
			Close();
		}


		public void Install()
		{
			Invoke(this, ValidateUserCredentials);
            Thread.Sleep(1000);
			Invoke(this, GiveUserLoginAsServicePermission); 
            Thread.Sleep(1000);
			Invoke(this, InstalliTuneService); 
            Thread.Sleep(1000);
			Invoke(this, StartiTuneService); 
            Thread.Sleep(2000);
            Invoke(this, Hide);           
        }

		public void ValidateUserCredentials()
		{
            _logger.Log("ValidateUserCredentials" + _mainForm.computerNameBox.Text);

            var isValid = ServiceManager.AuthenticateUser(_mainForm.usernameBox.Text, _mainForm.passwordBox1.Text);
			if ( !isValid )
			{
                _logger.Log("Throwing IOException: The given password does not match the user " + _mainForm.usernameBox.Text);
                throw new IOException("The given password does not match the user " + _mainForm.usernameBox.Text);
			}

            _logger.Log("User Validated" + _mainForm.computerNameBox.Text);
			ValidateUserCredentialsTick.Visible = true;
		}

		public void GiveUserLoginAsServicePermission()
		{
			try
			{
                Permission.SetRight(_mainForm.usernameBox.Text, "SeServiceLogonRight");
			}
			catch ( Exception exception )
			{
                _logger.Log("Could not set right SeServiceLogonRight to " + _mainForm.iTunesPathBox.Text + ": " + exception);
			}
			GiveUserLoginAsServicePermissionTick.Visible = true;
		}

		public void InstalliTuneService()
		{
            var iTunesPath = "/ITunesPath=" + _mainForm.iTunesPathBox.Text;
            var userName = "/UserName=" + _mainForm.computerNameBox.Text + @"\" + _mainForm.usernameBox.Text;

            // Encrypt password for transfer using today's date as a long string for the password
            var password = "/EncryptedPassword=" + Encryption.Encrypt(_mainForm.passwordBox1.Text, DateTime.Now.ToLongDateString());

            _logger.Log("InstallWin.InstalliTuneService() Enter");
		    _logger.Log(iTunesPath);
		    _logger.Log(userName);
		    _logger.Log(password);
		    
            var installArgs = new[]
		    {
		        iTunesPath,
		        userName,
		        password,
		        "./iTuneService.exe"
		    };
			ServiceManager.Install(installArgs);
			
            InstalliTuneServiceTick.Visible = true;
		}

		public void StartiTuneService()
		{
			ServiceManager.StartService(ServiceManager.ServiceName);
			StartiTuneServiceTick.Visible = true;
			SuccessLbl.Visible = true;
		}

		public new void Hide()
		{
			_mainForm.pictureBox1.Visible = _mainForm.passwordBox1.Enabled = _mainForm.passwordBox2.Enabled = _mainForm.usernameBox.Enabled = _mainForm.selectITunesExeBtn.Enabled = _mainForm.installBtn.Enabled = false;
			_mainForm.UninstallBtn.Enabled = true;
			_mainForm.startBtn.Enabled = true;
			_mainForm.startBtn.Text = "Stop";
			_mainForm.openITunes.Enabled = false;

			Closed += (sender, args) => { _mainForm.Visible = true; };
			Close();
		}
	}
}
