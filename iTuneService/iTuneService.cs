using System;
using System.Threading;
using System.Runtime;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;
using Common;

namespace iTuneService
{
    class iTuneService : ServiceBase
    {
        public const string PublicServiceName = "iTuneServer Service";

        private static Logger _log = Logger.Instance;

        Process _iTunesProcess;
        string _iTunesFileName;

        public iTuneService(String fn)
        {
            _iTunesFileName = fn;

            ServiceName = Constants.ServiceName;
            EventLog.Source = Constants.ServiceName;
            EventLog.Log = "Application";

            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanStop = true;

            if (!EventLog.SourceExists(Constants.ServiceName))
                EventLog.CreateEventSource(Constants.ServiceName, "Application");
        }

        protected override void OnStart(string[] args)
        {
            _iTunesProcess = new Process();
            _iTunesProcess.StartInfo.FileName = _iTunesFileName;
            _iTunesProcess.Start();

            _log.Write("iTuneServer: Service Starting");
            _log.Write("Starting Process: " + _iTunesFileName);
            
            Thread.Sleep(50);

            if (!_iTunesProcess.HasExited)
            {
                _log.Write("Process: '" + _iTunesFileName + "' has started with a pid of " + _iTunesProcess.Id);
                _log.Write("iTuneServer: Service Started");
            }

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            _log.Write("iTuneServer: Service Stopping");
            _log.Write("Shutting down Process: '" + _iTunesFileName + "', PID: " + _iTunesProcess.Id);
            _iTunesProcess.Kill();
            Thread.Sleep(50);
            if (_iTunesProcess.HasExited)
            {
                _log.Write("Process: '" + _iTunesFileName + "' has exited");
                _log.Write("iTuneServer: Service Stopped");
            }

            base.OnStop();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
        }


        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }


        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }

        private void InitializeComponent()
        {

        }
    }

}
