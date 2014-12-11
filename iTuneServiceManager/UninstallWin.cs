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

		public void Invoke(UninstallWin i, Action method)
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

        public UninstallWin(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            new Thread(Uninstall).Start();
        }

        public void Uninstall()
        {
            Thread.Sleep(1000);
            Invoke(this, StopiTuneService);
            Thread.Sleep(1000);
            Invoke(this, UninstalliTuneService);
            Thread.Sleep(1600);
			Invoke(this, Hide);
        }

        public void ShowError(string err)
        {
            MessageBox.Show(this, err, "Error Uninstalling Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Visible = false;
            _mainForm.Visible = true;
        }

        public void UninstalliTuneService()
        {
            ServiceManager.Uninstall();
			UninstalliTuneServiceTick.Visible = true;
			SuccessLbl.Visible = true;
        }

        public void StopiTuneService()
        {
            if (ServiceManager.ServiceStatus == ServiceControllerStatus.Running)
            {
                ServiceManager.StopService(ServiceManager.ServiceName);
                _mainForm.startBtn.Enabled = true;
            }

            StoppingiTuneServiceTick.Visible = true;
        }

        public new void Hide()
        {
            _mainForm.pictureBox1.Visible = false;
            _mainForm.passwordBox1.Enabled = _mainForm.passwordBox2.Enabled = _mainForm.usernameBox.Enabled = _mainForm.selectITunesExeBtn.Enabled = true;
            _mainForm.installBtn.Enabled = _mainForm.ITunesExeFound && _mainForm.PasswordsMatching;
            _mainForm.UninstallBtn.Enabled = false;
            _mainForm.startBtn.Enabled = false;
            _mainForm.openITunes.Enabled = true;
			Closed += (sender, args) => { _mainForm.Visible = true; };
			Close();
        }
    }
}
