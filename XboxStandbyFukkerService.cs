using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Timers;
using System.Windows.Media;

namespace XboxStandbyFukker
{
    public partial class XboxStandbyFukkerService : ServiceBase
    {
        private readonly Timer _timer = new Timer();
        private readonly EventLog _log = new EventLog();

        private const string EVENT_SOURCE = "XboxStandbyFukker";
        private const string EVENT_LOG = "Application";

        private static readonly string AUDIO_PATH = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources", "40khz.wav");
        private static readonly TimeSpan INTERVAL = new TimeSpan(0, 0, 3, 0); // 3 minutes
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
            _log.WriteEntry("Playing sound to keep Xbox Wireless Headset alive.", EventLogEntryType.Information);
            var player = new MediaPlayer();
            _log.WriteEntry("Sound located at: " + AUDIO_PATH);
            player.Open(new Uri(AUDIO_PATH));
            player.Volume = 0.02;
            player.Play();
        }
        private void Check(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (IsHeadsetConnected())
                {
                    PlaySound();
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
            _timer.Interval = INTERVAL.TotalMilliseconds;
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
