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
using System.Configuration;
using System.Collections.Specialized;

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
            var camera1 = ConfigurationManager.GetSection("Device/Camera1") as NameValueCollection;
            var camera2 = ConfigurationManager.GetSection("Device/Camera2") as NameValueCollection;
            var common = ConfigurationManager.GetSection("Device/Common") as NameValueCollection;

            CAMERA_1_IP = camera1["Ip"];
            CAMERA_1_PORT = camera1["Port"];
            CAMERA_1_USERNAME = camera1["Username"];
            CAMERA_1_PASSWORD = camera1["Password"];

            CAMERA_2_IP = camera2["Ip"];
            CAMERA_2_PORT = camera2["Port"];
            CAMERA_2_USERNAME = camera2["Username"];
            CAMERA_2_PASSWORD = camera2["Password"];

            URL_IMAGE = common["UrlImage"];
        }
    }
}