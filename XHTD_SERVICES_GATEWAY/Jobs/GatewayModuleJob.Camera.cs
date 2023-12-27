using CHCNetSDK;
using XHTD_SERVICES_GATEWAY.NetSDKCS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XHTD_SERVICES_GATEWAY.Jobs
{
    public partial class GatewayModuleJob
    {
        private string CAMERA_1_IP = "";
        private string CAMERA_1_PORT = "";
        private string CAMERA_1_USERNAME = "";
        private string CAMERA_1_PASSWORD = "";

        private string CAMERA_2_IP = "";
        private string CAMERA_2_PORT = "";
        private string CAMERA_2_USERNAME = "";
        private string CAMERA_2_PASSWORD = "";

        private string URL_IMAGE = "";

        private void LoadCamera()
        {
            CAMERA_1_IP = "10.15.15.103";
            CAMERA_1_PORT = "554";
            CAMERA_1_USERNAME = "admin";
            CAMERA_1_PASSWORD = "abcd@1234";

            CAMERA_2_IP = "10.15.15.103";
            CAMERA_2_PORT = "554";
            CAMERA_2_USERNAME = "admin";
            CAMERA_2_PASSWORD = "abcd@1234";

            URL_IMAGE = @"C:\MBF6\MMES";
        }
    }
}