using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES_GATEWAY.Device
{
    public interface IStreamCamera
    {
        string GetRtspUrl(string CameraIP, string CameraPort, string CameraUsername, string CameraPassword);
        string CaptureStream(string CameraIP, string CameraUsername, string CameraPassword, string type, int number, string UrlImage);
    }
}
