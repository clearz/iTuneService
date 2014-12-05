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
	    private readonly MainForm _form;
		public void Invoke(UninstallWin i, Action method)
		{
			if ( i.InvokeRequired )
			{
				i.Invoke((Action)( () => {
					try
					{
						method();
					}
					catch ( Exception e )
					{
						_logger.Log(e);
						ShowError("Error @ " + method.GetType().Name);
					}
				} ));
			}
			else method();
		}

        public UninstallWin(MainForm form)
        {
            this._form = form;
            InitializeComponent();
            new Thread(run).Start();
        }
        public void run()
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
            this.Visible = false;
            _form.Visible = true;
        }
        public void UninstalliTuneService()
        {
            ServiceManager.Uninstall();
			UninstalliTuneServiceTick.Visible = true;
			SuccessLbl.Visible = true;
        }

        public void StopiTuneService()
        {
			if ( ServiceManager.ServiceStatus == ServiceControllerStatus.Running )
			{
				ServiceManager.StopService("iTune Service");
                _form.startBtn.Enabled = true;
            }
            StoppingiTuneServiceTick.Visible = true;
        }

        public new void Hide()
        {
            _form.pictureBox1.Visible = false;
            _form.passwordBox1.Enabled = _form.passwordBox2.Enabled = _form.usernameBox.Enabled = _form.selectITunesExeBtn.Enabled = true;
            _form.installBtn.Enabled = _form.ITunesExeFound && _form.PasswordsMatching;
            _form.UninstallBtn.Enabled = false;
            _form.startBtn.Enabled = false;
            _form.openITunes.Enabled = true;
			Closed += (sender, args) => { _form.Visible = true; };
			Close();
        }


    }
}
