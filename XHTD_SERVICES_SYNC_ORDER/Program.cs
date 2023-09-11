using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using XHTD_SERVICES.Data.Entities;
using XHTD_SERVICES_SYNC_ORDER.Models.Request;

namespace XHTD_SERVICES_SYNC_ORDER
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //Initialize the mapper
            var config = new MapperConfiguration(cfg =>
                    cfg.CreateMap<ScaleBill, ScaleBillDto>()
                );

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
