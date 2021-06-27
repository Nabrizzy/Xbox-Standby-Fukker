using Autofac;
using System.ServiceProcess;

namespace XboxStandbyFukker
{
    static class Program
    {
        static void Main()
        {
            var builder = new ContainerBuilder();

            // Register windows service
            builder.RegisterType<XboxStandbyFukkerService>().AsSelf().InstancePerLifetimeScope();

            // Register dependencies
            //builder.RegisterType<ClassType>().InstancePerLifetimeScope();

            // Start service
            ServiceBase.Run(builder.Build().Resolve<XboxStandbyFukkerService>());
        }
    }
}
