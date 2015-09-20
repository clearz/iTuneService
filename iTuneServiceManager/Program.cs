using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Microsoft.VisualBasic.ApplicationServices;

namespace iTuneServiceManager
{
    public class SingleInstanceController
        : WindowsFormsApplicationBase
    {
        public SingleInstanceController()
        {
            EnableVisualStyles = true;

            // Set the application to single instance mode
            IsSingleInstance = true;

            StartupNextInstance += OnStartupNextInstance;
        }

        void OnStartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            var mainForm = (MainForm)MainForm;

            // Always activate the window if it's currently visible
            if (mainForm.Visible)
            {
                mainForm.Activate();
                return;
            }

            // Make main form visible if not a request to start minimized
            var startMinimized = e.CommandLine.Any(a => a.ToLower() == "-m");
            if (!startMinimized)
            {
                mainForm.ShowAfterOtherInstanceStarted();
            }
        }

        protected override void OnCreateMainForm()
        {
            // Instantiate for a new application
            var startMinimized = CommandLineArgs.Any(a => a.ToLower() == "-m");
            MainForm = new MainForm(startMinimized);
        }
    }
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            // Store info for use by logger
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            GlobalContext.Properties["whichApp"] = "MGR";

            var controller = new SingleInstanceController();
            controller.Run(args);
        }
    }
}
