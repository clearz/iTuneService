using System;
using System.Drawing;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Common;

namespace iTuneServiceManager
{
    public partial class UninstallWin : Form
    {
		private readonly Logger _logger = Logger.Instance;

        private readonly MainForm _mainForm;

        public void InvokeIfRequired(Action method)
        {
            if (InvokeRequired)
                Invoke(method);
            else
                method();
        }

        public UninstallWin(MainForm mainForm)
        {
            if (mainForm == null) throw new ArgumentNullException("mainForm");
            _mainForm = mainForm;
            InitializeComponent();

            new Thread(Uninstall).Start();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            _mainForm.CheckSetupComplete();
            _mainForm.Visible = true;
        }

        public void Uninstall()
        {
	        Label currentLabel = null;
            var waitBeforeReturnToMainForm = 1600;

            try
            {
                Thread.Sleep(500);

                currentLabel = StoppingiTuneServiceTick;
                StopiTuneService();
                UpdateStatus(currentLabel, true);

                Thread.Sleep(1000);

                currentLabel = UninstalliTuneServiceLbl;
                UninstalliTuneService();
                InvokeIfRequired(
                    () =>
                    {
                        UninstalliTuneServiceTick.Visible = true;
                        SuccessLbl.Visible = true;
                    });

                ServiceManager.CurrentState = ServiceManager.State.Setup;
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
                        SuccessLbl.Text = "FAILED!";
                        SuccessLbl.ForeColor = Color.Red;
                        SuccessLbl.Show();
                        tickLabel.Text = "✖";
                        tickLabel.ForeColor = Color.Red;
                    }
                    tickLabel.Show();
                });
        }

        public void StopiTuneService()
        {
            if (Service.ServiceStatus == ServiceControllerStatus.Running)
            {
                ServiceManager.StopService();
            }
        }

        public void UninstalliTuneService()
        {
            ServiceManager.Uninstall();
            Credentials.RemoveCredential();
        }
    }
}
