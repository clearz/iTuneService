using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CommandLine;
using Common;
using log4net;
using Microsoft.VisualBasic.ApplicationServices;

namespace iTuneServiceManager
{
    public class SingleInstanceController
        : WindowsFormsApplicationBase
    {
        public Options Options { get; private set; }

        public SingleInstanceController(Options options)
        {
            Options = options;

            EnableVisualStyles = true;

            // Set the application to single instance mode
            IsSingleInstance = true;

            Startup += OnStartup;
            StartupNextInstance += OnStartupNextInstance;
        }

        public void SetSingleInstance(bool isSingleInstance)
        {
            IsSingleInstance = isSingleInstance;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            // Automatically elevate if not starting minimized
            if (!Util.IsRunAsAdmin() && !Options.Minimized)
            {
                Util.RelaunchAsAdmin(string.Join(" ", e.CommandLine));

                // Always cancel startup because either we're relaunching or tge
                // user canceled and we should take this to mean he'd rather not
                // run the application at all.
                e.Cancel = true;
            }
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
            Options = Program.GetCommandLineOptions(e.CommandLine);
            if (!Options.Minimized)
            {
                mainForm.ShowAfterOtherInstanceStarted();
            }

            base.OnStartupNextInstance(e);
        }

        protected override void OnCreateMainForm()
        {
            // Instantiate for a new application
            MainForm = new MainForm(Options);
        }

    }
    internal static class Program
    {
        private static readonly ILog _logger;

        static Program()
        {
            // Store info for use by logger
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            GlobalContext.Properties["whichApp"] = "MGR";

            _logger = LogManager.GetLogger(typeof(SingleInstanceController));
        }

        [STAThread]
        private static void Main(string[] args)
        {
            var sanitizedArgs = args.ToList();
            for (var i = 0; i < sanitizedArgs.Count - 1; i++)
            {
                if (sanitizedArgs[i].ToUpperInvariant() == "--" + OptionConstants.Password.ToUpperInvariant())
                {
                    sanitizedArgs[i + 1] = "<obfuscated>";
                }
            }
            _logger.InfoFormat("iTuneServiceManager entry. Version={0}; Command line has {1} argument(s): \"{2}\"", 
                               Application.ProductVersion, args.Length, string.Join(" ", sanitizedArgs));

            var options = GetCommandLineOptions(args);
            var controller = new SingleInstanceController(options);
            Util.RegisterSetAppSingleInstance(controller.SetSingleInstance);
            controller.Run(args);
        }

        public static Options GetCommandLineOptions(IEnumerable<string> args)
        {
            var options = new Options();
            var argsAsArray = args.ToArray();

            if (!Parser.Default.ParseArguments(argsAsArray, options))
            {
                _logger.ErrorFormat("Invalid command line: {0}", string.Join(" ", argsAsArray));
            }

            return options;
        }
    }
}
