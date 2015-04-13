using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using log4net;

namespace iTuneServiceManager
{
    internal static class Program
    {

        [STAThread]
        private static void Main(string[] args)
        {
            // Store info for use by logger
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            GlobalContext.Properties["whichApp"] = "MGR";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new MainForm(args.Any(a => a.ToLower() == "-m"))); // -m flag to start minimised
        }
    }
}
