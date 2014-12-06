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
		
		public void Invoke(InstallWin i, Action method)
		{
			if (i.InvokeRequired)
			{
				i.Invoke((Action) (() =>
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
				}));
			}
			else method();
		}

		public InstallWin()
		{
			InitializeComponent();
			new Thread(Install).Start();
		}

		public void ShowError(String err)
		{
			MessageBox.Show(this, err, "Error Installing Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Closed += (sender, args) => { Thread.Sleep(10); Owner.Visible = true; }; 
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
			var form = ( (MainForm)Owner );
			_logger.Log("ValidateUserCredentials" + form.computerNameBox.Text);
			bool isValid = ServiceManager.AuthenticateUser(form.usernameBox.Text, form.passwordBox1.Text);
			if ( !isValid )
			{
				_logger.Log("Throwing IOException: The given password does not match the user " + form.usernameBox.Text);
				throw new IOException("The given password does not match the user " + form.usernameBox.Text);
			}

			_logger.Log("User Validated" + form.computerNameBox.Text);
			ValidateUserCredentialsTick.Visible = true;
		}

		public void GiveUserLoginAsServicePermission()
		{
			var form = ( (MainForm)Owner );
			try
			{
				Permission.SetRight(form.usernameBox.Text, "SeServiceLogonRight");
			}
			catch ( Exception exception )
			{
				_logger.Log("Could not set right SeServiceLogonRight to " + form.iTunesPathBox.Text + ": " + exception);
			}
			GiveUserLoginAsServicePermissionTick.Visible = true;
		}

		public void InstalliTuneService()
		{
			var form = ( (MainForm)Owner );
			_logger.Log("InstallWin.InstalliTuneService() Enter");
			_logger.Log("/ITunesPath=" + form.iTunesPathBox.Text);
			_logger.Log("/UserName=" + form.computerNameBox.Text + @"\" + form.usernameBox.Text);
			_logger.Log("/EncryptedPassword=" + Encryption.Encrypt(form.passwordBox1.Text, DateTime.Now.ToLongDateString()));
			var strz = new[]
                           {
                               "/ITunesPath=" + form.iTunesPathBox.Text,
                               "/UserName=" + form.computerNameBox.Text + @"\" + form.usernameBox.Text,
                               "/EncryptedPassword=" + Encryption.Encrypt(form.passwordBox1.Text, DateTime.Now.ToLongDateString()),
                               "./iTuneService.exe"
                           };
			ServiceManager.Install(strz);
			InstalliTuneServiceTick.Visible = true;

		}

		public void StartiTuneService()
		{

			ServiceManager.StartService("iTuneServer Service");
			StartiTuneServiceTick.Visible = true;
			SuccessLbl.Visible = true;
		}

		public void Hide()
		{
			var form = ( (MainForm)Owner );
			form.pictureBox1.Visible = form.passwordBox1.Enabled = form.passwordBox2.Enabled = form.usernameBox.Enabled = form.selectITunesExeBtn.Enabled = form.installBtn.Enabled = false;
			form.UninstallBtn.Enabled = true;
			form.startBtn.Enabled = true;
			form.startBtn.Text = "Stop";
			form.openITunes.Enabled = false;

			Closed += (sender, args) => { form.Visible = true; };
			Close();
		}
	}
}
