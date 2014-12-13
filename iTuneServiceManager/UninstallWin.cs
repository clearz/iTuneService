using System;
using System.Threading;
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
    public partial class UninstallWin : Form
    {
		private readonly Logger _logger = Logger.GetLogger(writeToConsole: true);

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
            if (ServiceManager.ServiceStatus == ServiceControllerStatus.Running)
            {
                ServiceManager.StopService(ServiceManager.ServiceName);
            }
        }

        public void UninstalliTuneService()
        {
            ServiceManager.Uninstall();
            ServiceManager.RemoveCredential();
        }
    }
}
