using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;
using Common;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace iTuneService
{
    class iTuneService : ServiceBase
    {
        public const string PublicServiceName = "iTuneServer Service";

        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(iTuneService));

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
            _log.Info("iTuneServer: Service Starting");

            _log.Info("iTuneServer: Initializing audio");
            InitAudio();

            //  Start the actual process
            _iTunesProcess = new Process { StartInfo = { FileName = _iTunesFileName } };
            _log.Info("Starting Process: " + _iTunesFileName);
            _iTunesProcess.Start();
            
            // Pause a moment to allow iTunes to start or fail
            Thread.Sleep(200);

            if (!_iTunesProcess.HasExited)
            {
                _log.Info("Process: '" + _iTunesFileName + "' has started with a pid of " + _iTunesProcess.Id);
                _log.Info("iTuneServer: Service Started");
            }
            else
            {
                _log.Info("Process: '" + _iTunesFileName + "' has exited prematurely");
            }

            base.OnStart(args);
        }

        /// <summary>
        /// Initialize audio so iTunes will be able to play. Audio is initialized when a user logs in
        /// but not if only the service has started.
        /// </summary>
        protected void InitAudio()
        {
            _log.Info("Entering");
            try
            {
                MMDevice device;
                var deviceEnum = new MMDeviceEnumerator();
                var initializedDevices = new HashSet<string>();

                foreach (var role in new[] { Role.Console, Role.Multimedia })
                {
                    _log.InfoFormat("Checking for default audio endpoint for role {0}", role);
                    if (!deviceEnum.HasDefaultAudioEndpoint(DataFlow.Render, role))
                    {
                        _log.InfoFormat("No default audio endpoint found for role {0}", role);
                        return;
                    }

                    _log.InfoFormat("Default audio endpoint WAS found for role {0}", role);

                    device = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, role);

                    _log.InfoFormat("Default audio endpoint loaded for role {0}. ID='{1}', FriendlyName='{2}', DeviceFriendlyName='{3}'",
                                    role, device.ID, device.FriendlyName, device.DeviceFriendlyName);

                    if (initializedDevices.Contains(device.ID))
                    {
                        _log.InfoFormat("Skipping init for ID='{0}' because already processed.", device.ID);
                        continue;
                    }
                    initializedDevices.Add(device.ID);

                    using (var waveOut = new WasapiOut(device, AudioClientShareMode.Shared, true, 300))
                    using (var mixer = new WaveMixerStream32())
                    {
                        _log.Info("Calling Init");
                        waveOut.Init(mixer);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("Caught Exception", e);
            }
            _log.Info("Exiting");
        }

        protected override void OnStop()
        {
            _log.Info("iTuneServer: Service Stopping");
            _log.Info("Shutting down Process: '" + _iTunesFileName + "', PID: " + _iTunesProcess.Id);
            _iTunesProcess.Kill();
            Thread.Sleep(50);
            if (_iTunesProcess.HasExited)
            {
                _log.Info("Process: '" + _iTunesFileName + "' has exited");
                _log.Info("iTuneServer: Service Stopped");
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
