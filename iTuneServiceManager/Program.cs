using System;
using System.Diagnostics;
using System.Windows.Forms;
using log4net;

namespace iTuneServiceManager
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            // Store info for use by logger
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            GlobalContext.Properties["whichApp"] = "MGR";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        
    }
}
