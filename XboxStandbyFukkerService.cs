using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        private const string HELPER_ARGS = "-f \"%d;%ws\"";

        private static readonly string AUDIO_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sound.wav");
        private static readonly TimeSpan INTERVAL = new TimeSpan(0, 0, 3, 0);
        private static readonly string HELPER_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "helper.exe");
        private static readonly string DEVICE_NAME_COMPARER = ConfigurationManager.AppSettings["DeviceName"].ToLower();
        private static readonly bool WRITE_LOGS = bool.Parse(ConfigurationManager.AppSettings["WriteLogs"]);
        private static readonly ProcessStartInfo PROCESS_START_INTO = new ProcessStartInfo()
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            FileName = HELPER_PATH,
            Arguments = HELPER_ARGS
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

            IsHeadsetConnected();

            InitializeComponent();
        }

        private bool IsHeadsetConnected()
        {
            Process process = Process.Start(PROCESS_START_INTO);
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd().ToLower().Contains(DEVICE_NAME_COMPARER);
        }
        private void PlaySound()
        {
            if (WRITE_LOGS)
            {
                _log.WriteEntry("Playing sound to keep Xbox Wireless Headset alive.", EventLogEntryType.Information);
            }
            var player = new MediaPlayer();
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
            if (WRITE_LOGS)
            {
                _log.WriteEntry("Startup Xbox Standby Fukker.", EventLogEntryType.Information);
            }
            _timer.Interval = INTERVAL.TotalMilliseconds;
            _timer.Elapsed += Check;
            _timer.Start();
            Check(this, null);
        }
        protected override void OnStop()
        {
            if (WRITE_LOGS)
            {
                _log.WriteEntry("Shutdown Xbox Standby Fukker.", EventLogEntryType.Information);
            }
            _timer.Stop();
            _timer.Elapsed -= Check;
        }
    }
}
