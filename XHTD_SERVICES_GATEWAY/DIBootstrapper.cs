using Autofac.Extras.Quartz;
using Autofac;
using System.Collections.Specialized;
using XHTD_SERVICES_GATEWAY.Schedules;
using XHTD_SERVICES.Data.Repositories;
using XHTD_SERVICES_GATEWAY.Jobs;
using XHTD_SERVICES.Data.Entities;
using NDTan;
using XHTD_SERVICES.Helper;
using XHTD_SERVICES_GATEWAY.Business;

namespace XHTD_SERVICES_GATEWAY
{
    public static class DIBootstrapper
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<XHTD_Entities>().AsSelf();
            builder.RegisterType<RfidRepository>().AsSelf();
            builder.RegisterType<Notification>().AsSelf();
            builder.RegisterType<PLC>().AsSelf();
            builder.RegisterType<GatewayLogger>().AsSelf();
            builder.RegisterType<ScaleApiLib>().AsSelf();

            RegisterScheduler(builder);

            return builder.Build();
        }

        private static void RegisterScheduler(ContainerBuilder builder)
        {
            var schedulerConfig = new NameValueCollection {
              {"quartz.threadPool.threadCount", "20"},
              {"quartz.scheduler.threadName", "MyScheduler"}
            };

            builder.RegisterModule(new QuartzAutofacFactoryModule
            {
                ConfigurationProvider = c => schedulerConfig
            });

            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(GatewayModuleJob).Assembly));
            builder.RegisterType<JobScheduler>().AsSelf();
        }
    }
}
