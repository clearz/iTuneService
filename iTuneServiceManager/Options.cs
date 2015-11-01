using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace iTuneServiceManager
{
    public static class OptionConstants
    {
        public const string Minimized = "minimized";
        public const string StartService = "start";
        public const string StopService = "stop";
        public const string RunInteractive = "interactive";
        public const string InstallService = "install";
        public const string UninstallService = "uninstall";
        public const string ITunesPath = "itunes";
        public const string Domain = "domain";
        public const string User = "user";
        public const string Password = "password";
    }

    public class Options
    {
        [Option('m', OptionConstants.Minimized, DefaultValue = false, HelpText = "Starts the application in the tray.")]
        public bool Minimized { get; set; }

        [Option(OptionConstants.StartService, DefaultValue = false, HelpText = "Starts the iTunes service.")]
        public bool StartService { get; set; }
        [Option(OptionConstants.StopService, DefaultValue = false, HelpText = "Stops the iTunes service.")]
        public bool StopService { get; set; }
        [Option(OptionConstants.RunInteractive, DefaultValue = false, HelpText = "Runs iTunes in interactive mode, stopping and starting the iTunes service as necessary.")]
        public bool RunInteractive { get; set; }

        [Option(OptionConstants.InstallService, DefaultValue = false, HelpText = "Installs the iTunes service.")]
        public bool InstallService { get; set; }
        [Option(OptionConstants.UninstallService, DefaultValue = false, HelpText = "Installs the iTunes service.")]
        public bool UninstallService { get; set; }
        [Option(OptionConstants.ITunesPath, HelpText = "Location of iTunes for installing the iTunes service.")]
        public string ITunesPath { get; set; }
        [Option(OptionConstants.Domain, HelpText = "Domain for installing the iTunes service.")]
        public string Domain { get; set; }
        [Option(OptionConstants.User, HelpText = "User for installing the iTunes service.")]
        public string User { get; set; }
        [Option(OptionConstants.Password, HelpText = "Password for installing the iTunes service.")]
        public string Password { get; set; }

        public bool HasFullInstallInfo
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ITunesPath) &&
                       !string.IsNullOrWhiteSpace(Domain) &&
                       !string.IsNullOrWhiteSpace(User) &&
                       !string.IsNullOrWhiteSpace(Password);
            }
        }
    }
}
