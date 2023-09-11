﻿using System;
using Autofac;
using log4net;
using XHTD_SERVICES_SYNC_ORDER;
using XHTD_SERVICES_SYNC_ORDER.Schedules;

namespace XHTD_DEBUG
{
    internal class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {            
            IContainer container = DIBootstrapper.Init();
            container.Resolve<JobScheduler>().Start();

            Console.ReadLine();
        }
    }
}
