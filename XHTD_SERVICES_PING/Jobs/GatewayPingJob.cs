﻿using System;
using System.Threading.Tasks;
using Quartz;
using System.Net.NetworkInformation;
using log4net;

namespace XHTD_SERVICES_PING.Jobs
{
    public class GatewayPingJob : IJob
    {
        ILog logger = LogManager.GetLogger("GatewayFileAppender");

        protected const string C3400_IP_ADDRESS = "10.0.9.1";
        protected const string C3400_TEST_IP_ADDRESS = "10.0.9.10";
        protected const string M221_IP_ADDRESS = "10.0.9.2";
        protected const string DGT_IN_IP_ADDRESS = "10.0.9.3";
        protected const string DGT_OUT_IP_ADDRESS = "10.0.9.4";

        public GatewayPingJob(
            )
        {
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await Task.Run(() =>
            {
                Console.WriteLine("-------- Start ping server --------");
                logger.Info("-------- Start ping server --------");

                PingServer();
            });                                                                                                                     
        }

        private void PingServer()
        {
            Ping myPing = new Ping();
            PingReply replyC3400 = myPing.Send(C3400_IP_ADDRESS, 1000);
            PingReply replyC3400Test = myPing.Send(C3400_TEST_IP_ADDRESS, 1000);
            PingReply replyM221 = myPing.Send(M221_IP_ADDRESS, 1000);
            PingReply replyLightIn = myPing.Send(DGT_IN_IP_ADDRESS, 1000);
            PingReply replyLightOut = myPing.Send(DGT_OUT_IP_ADDRESS, 1000);

            if (replyC3400 != null)
            {
                Console.WriteLine("C3400 Address: " + replyC3400.Address + " - Status:  " + replyC3400.Status + " - Time : " + replyC3400.RoundtripTime.ToString());
                logger.Info("C3400 Address: " + replyC3400.Address + " - Status:  " + replyC3400.Status + " - Time : " + replyC3400.RoundtripTime.ToString());
            }
            else
            {
                Console.WriteLine("Khong nhan duoc tin hieu ping replyC3400");
                logger.Info("Khong nhan duoc tin hieu ping replyC3400");
            }

            if (replyC3400Test != null)
            {
                Console.WriteLine("C3400 Address: " + replyC3400Test.Address + " - Status:  " + replyC3400Test.Status + " - Time : " + replyC3400Test.RoundtripTime.ToString());
                logger.Info("C3400 Address: " + replyC3400Test.Address + " - Status:  " + replyC3400Test.Status + " - Time : " + replyC3400Test.RoundtripTime.ToString());
            }
            else
            {
                Console.WriteLine("Khong nhan duoc tin hieu ping replyC3400Test");
                logger.Info("Khong nhan duoc tin hieu ping replyC3400Test");
            }

            if (replyM221 != null)
            {
                Console.WriteLine("M221 Address: " + replyM221.Address + " - Status:  " + replyM221.Status + " - Time : " + replyM221.RoundtripTime.ToString());
                logger.Info("M221 Address: " + replyM221.Address + " - Status:  " + replyM221.Status + " - Time : " + replyM221.RoundtripTime.ToString());
            }
            else
            {
                Console.WriteLine("Khong nhan duoc tin hieu ping replyM221");
                logger.Info("Khong nhan duoc tin hieu ping replyM221");
            }

            if (replyLightIn != null)
            {
                Console.WriteLine("LightIn Address: " + replyLightIn.Address + " - Status:  " + replyLightIn.Status + " - Time : " + replyLightIn.RoundtripTime.ToString());
                logger.Info("LightIn Address: " + replyLightIn.Address + " - Status:  " + replyLightIn.Status + " - Time : " + replyLightIn.RoundtripTime.ToString());
            }
            else
            {
                Console.WriteLine("Khong nhan duoc tin hieu ping replyLightIn");
                logger.Info("Khong nhan duoc tin hieu ping replyLightIn");
            }

            if (replyLightOut != null)
            {
                Console.WriteLine("LightOut Address: " + replyLightOut.Address + " - Status:  " + replyLightOut.Status + " - Time : " + replyLightOut.RoundtripTime.ToString());
                logger.Info("LightOut Address: " + replyLightOut.Address + " - Status:  " + replyLightOut.Status + " - Time : " + replyLightOut.RoundtripTime.ToString());
            }
            else
            {
                Console.WriteLine("Khong nhan duoc tin hieu ping replyLightOut");
                logger.Info("Khong nhan duoc tin hieu ping replyLightOut");
            }
        }
    }
}
