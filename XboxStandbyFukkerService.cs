using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace XboxStandbyFukker
{
    public partial class XboxStandbyFukkerService : ServiceBase
    {
        private readonly EventLog Log = new EventLog();
        private const string EVENT_SOURCE = "Xbox-Standby-Fukker";
        private const string EVENT_LOG = "Application";

        public XboxStandbyFukkerService()
        {
            AutoLog = false;

            if (!EventLog.SourceExists(EVENT_SOURCE))
            {
                EventLog.CreateEventSource(EVENT_SOURCE, EVENT_LOG);
            }

            Log.Source = EVENT_SOURCE;
            Log.Log = EVENT_LOG;
            Log.WriteEntry("Initialize Xbox Standby Fukker", EventLogEntryType.Information);

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
