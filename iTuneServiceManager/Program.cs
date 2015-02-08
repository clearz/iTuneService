using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Common;

namespace iTuneServiceManager
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            Logger.Instance.WriteToConsole = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        
    }
}
