using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using Common;

namespace iTuneService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(String[] args)
        {
            Logger.Instance.WriteToConsole = true;
            Logger.Instance.Write("Args.Length: " + args.Length);
            var servicesToRun = new ServiceBase[] 
            { 
                new iTuneService(args[0]) 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
