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
using System.Windows.Media;

namespace XboxStandbyFukker
{
    public partial class XboxStandbyFukkerService : ServiceBase
    {
        private readonly EventLog Log = new EventLog();
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

            Log.Source = EVENT_SOURCE;
            Log.Log = EVENT_LOG;

            bool test = IsHeadsetConnected();

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

        protected override void OnStart(string[] args)
        {
            Log.WriteEntry("Startup Xbox Standby Fukker.", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            Log.WriteEntry("Shutdown Xbox Standby Fukker.", EventLogEntryType.Information);
        }
    }
}
