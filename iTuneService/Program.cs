using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using Common;
using log4net;

namespace iTuneService
{
    static class Program
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(String[] args)
        {
            // Store info for use by logger
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            GlobalContext.Properties["whichApp"] = "SRV";
            
            _logger.Debug("Args.Length: " + args.Length);

            var servicesToRun = new ServiceBase[] 
            { 
                new iTuneService(args[0]) 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
