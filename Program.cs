using System.ServiceProcess;

namespace XboxStandbyFukker
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new XboxStandbyFukkerService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
