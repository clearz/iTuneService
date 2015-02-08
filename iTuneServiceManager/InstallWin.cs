using System;
using System.Drawing;
using System.Threading;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;
using Common;

//del $(OutDir)iTuneServiceManager.pdb
//del $(OutDir)iTuneServiceManager.exe.config
//del $(OutDir)iTuneServiceManager.pdb
//del $(OutDir)iTuneServiceManager.exe.config


namespace iTuneServiceManager
{
	public partial class InstallWin : Form
	{
		private readonly Logger _logger = Logger.Instance;

	    private MainForm _mainForm;
	    private readonly DomainAuthCredentials _credentials;

		public void InvokeIfRequired(Action method)
		{
		    if (InvokeRequired) 
                Invoke(method);
		    else 
                method();
		}

	    public InstallWin(MainForm mainForm, DomainAuthCredentials credentials)
		{
	        if (mainForm == null) throw new ArgumentNullException("mainForm");
	        if (credentials == null) throw new ArgumentNullException("credentials");
	        _mainForm = mainForm;
	        _credentials = credentials;
	        InitializeComponent();

            new Thread(Install).Start();
		}

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            _mainForm.Visible = true;
        }

	    public void Install()
	    {
	        Label currentLabel = null;
            int waitBeforeReturnToMainForm = 2000;

            try
            {
                Thread.Sleep(750);

                currentLabel = ValidateUserCredentialsTick;
                ValidateUserCredentials();
                UpdateStatus(currentLabel, true);

                Thread.Sleep(1000);

                currentLabel = GiveUserLoginAsServicePermissionTick;
                GiveUserLoginAsServicePermission();
                UpdateStatus(currentLabel, true);

                Thread.Sleep(1000);

                currentLabel = InstalliTuneServiceTick;
                InstalliTuneService();
                UpdateStatus(currentLabel, true);

                Thread.Sleep(1000);

                currentLabel = StartiTuneServiceTick;
                StartiTuneService();
                InvokeIfRequired(
                    () =>
                    {
                        UpdateStatus(currentLabel, true);
                        SuccessLbl.Visible = true;
                    });
            }
            catch (Exception e)
            {
                _logger.Log(e);
                UpdateStatus(currentLabel, false);
                waitBeforeReturnToMainForm = 3000;
            }

            Thread.Sleep(waitBeforeReturnToMainForm);

	        InvokeIfRequired(Close);
	    }

	    private void UpdateStatus(Label tickLabel, bool success)
	    {
	        InvokeIfRequired(
	            () =>
	            {
	                if (!success)
	                {
                        SuccessLbl.Text ="FAILED!";
                        SuccessLbl.ForeColor = Color.Red;
                        SuccessLbl.Show();
                        tickLabel.Text = "✖";
                        tickLabel.ForeColor = Color.Red;
	                }
	                tickLabel.Show();
	            });
	    }

		public void ValidateUserCredentials()
		{
            _logger.Log("ValidateUserCredentials" + _credentials.Domain);

            var isValid = Service.AuthenticateUser(_credentials);
			if ( !isValid )
			{
                _logger.Log("Throwing IOException: The given password does not match the user " + _credentials.ToFullUsername());
                throw new IOException("The given password does not match the user " + _credentials.ToFullUsername());
			}

            _logger.Log("User Validated" + _credentials.Domain);		    
		}

		public void GiveUserLoginAsServicePermission()
		{
			try
			{
                Credentials.SetLogonAsServicePrivilege(_credentials);
			}
			catch ( Exception exception )
			{
                _logger.Log("Could not set right SeServiceLogonRight to " + _mainForm.iTunesPathBox.Text + ": " + exception);
			}
		}

		public void InstalliTuneService()
		{
            var iTunesPath = string.Format("/{0}={1}", Constants.ITunesPathContextArg, _mainForm.iTunesPathBox.Text);
            var userNameArg = string.Format("/{0}={1}", Constants.UserNameContextArg, _credentials.ToFullUsername());

            Credentials.PersistCredentials(_credentials.ToFullUsername(), _credentials.Password);

            _logger.Log("InstallWin.InstalliTuneService() Enter");
		    _logger.Log(iTunesPath);
		    _logger.Log(userNameArg);
		    
            var installArgs = new[]
		    {
		        iTunesPath,
		        userNameArg,
		        "./iTuneService.exe"
		    };
			ServiceManager.Install(installArgs);

            ServiceManager.CurrentState = Service.ServiceStatus == ServiceControllerStatus.Stopped
                                                      ? ServiceManager.State.ServiceStopped
                                                      : ServiceManager.State.ServiceRunning;
        }

		public void StartiTuneService()
		{
			ServiceManager.StartService();
		}
	}
}
