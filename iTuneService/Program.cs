using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using Common;
using log4net;

namespace iTuneService
{
    static class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(String[] args)
        {
            // Store info for use by logger
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            GlobalContext.Properties["whichApp"] = "SRV";
            
            _logger.InfoFormat("iTuneService entry. Version={0}; Command line has {1} argument(s): \"{2}\"",
                               Application.ProductVersion, args.Length, string.Join(" ", args));


            var servicesToRun = new ServiceBase[] 
            { 
                new iTuneService(args[0]) 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
