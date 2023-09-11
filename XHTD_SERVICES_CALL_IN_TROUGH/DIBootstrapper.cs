﻿using Autofac.Extras.Quartz;
using Autofac;
using System.Collections.Specialized;
using XHTD_SERVICES_CALL_IN_TROUGH.Schedules;
using XHTD_SERVICES.Data.Repositories;
using XHTD_SERVICES_CALL_IN_TROUGH.Jobs;
using XHTD_SERVICES.Data.Entities;
using XHTD_SERVICES.Helper;

namespace XHTD_SERVICES_CALL_IN_TROUGH
{
    public static class DIBootstrapper
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<XHTD_Entities>().AsSelf();
            builder.RegisterType<StoreOrderOperatingRepository>().AsSelf();
            builder.RegisterType<MachineRepository>().AsSelf();
            builder.RegisterType<CallToTroughRepository>().AsSelf();
            builder.RegisterType<SystemParameterRepository>().AsSelf();
            builder.RegisterType<CallInTroughLogger>().AsSelf();

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

            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(CallInTroughJob).Assembly));
            builder.RegisterType<JobScheduler>().AsSelf();
        }
    }
}
