﻿using Autofac.Extras.Quartz;
using Autofac;
using System.Collections.Specialized;
using XHTD_SERVICES_SYNC_BRAVO.Schedules;
using XHTD_SERVICES.Data.Repositories;
using XHTD_SERVICES_SYNC_BRAVO.Jobs;
using XHTD_SERVICES.Data.Entities;
using XHTD_SERVICES.Helper;

namespace XHTD_SERVICES_SYNC_BRAVO
{
    public static class DIBootstrapper
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<XHTD_Entities>().AsSelf();
            builder.RegisterType<mmes_bravoEntities>().AsSelf();
            builder.RegisterType<RfidRepository>().AsSelf();
            builder.RegisterType<ScaleBillRepository>().AsSelf();
            builder.RegisterType<ScaleImageRepository>().AsSelf();
            builder.RegisterType<SyncOrderLogger>().AsSelf();
            builder.RegisterType<PartnerRepository>().AsSelf();


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

            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(SyncOrderJob).Assembly));
            builder.RegisterType<JobScheduler>().AsSelf();
        }
    }
}
