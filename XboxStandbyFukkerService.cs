using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Media;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media;

namespace XboxStandbyFukker
{
    public partial class XboxStandbyFukkerService : ServiceBase
    {
        private readonly Timer _timer = new Timer();
        private readonly EventLog _log = new EventLog();
        private const string EVENT_SOURCE = "Xbox-Standby-Fukker";
        private const string EVENT_LOG = "Application";
        private static readonly string[] XBOX_HWIDS = new[]
        {
            @"USB\VID_045E&PID_0B17&IGA_00"
        };

        public XboxStandbyFukkerService()
        {
            AutoLog = false;

            if (!EventLog.SourceExists(EVENT_SOURCE))
            {
                EventLog.CreateEventSource(EVENT_SOURCE, EVENT_LOG);
            }

            while (true)
            {
                PlaySound();
                System.Threading.Thread.Sleep(500);
            }

            _log.Source = EVENT_SOURCE;
            _log.Log = EVENT_LOG;

            InitializeComponent();
        }

        private bool IsHeadsetConnected()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");
            var collection = searcher.Get();

            foreach (var obj in collection)
            {
                foreach (var prop in obj.Properties)
                {
                    if (prop?.Value is not null
                        && prop.Value is string value
                        && XBOX_HWIDS.Any(x => value.Contains(x)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void PlaySound()
        {
            string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Resources", "8khz.wav");
            var player = new MediaPlayer();
            player.Open(new Uri(path));
            player.Volume = 0.01;
            player.Play();
        }

        private void Check(object sender, ElapsedEventArgs e)
        {
            try
            {
                _log.WriteEntry($"Trigger check: {DateTime.Now:HH-mm-ss.ff}", EventLogEntryType.Information);
                if (IsHeadsetConnected())
                {
                    _log.WriteEntry("Headset is connected!", EventLogEntryType.Information);
                    PlaySound();
                }
                else
                {
                    _log.WriteEntry("Headset is not connected.", EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                _log.WriteEntry($"Exception occurred: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}", EventLogEntryType.Error);
            }
        }

        protected override void OnStart(string[] args)
        {
            _log.WriteEntry("Startup Xbox Standby Fukker.", EventLogEntryType.Information);
            //_timer.Interval = 240000;
            _timer.Interval = 2500;
            _timer.Elapsed += Check;
            _timer.Start();
        }

        protected override void OnStop()
        {
            _log.WriteEntry("Shutdown Xbox Standby Fukker.", EventLogEntryType.Information);
            _timer.Stop();
            _timer.Elapsed -= Check;
        }
    }
}
