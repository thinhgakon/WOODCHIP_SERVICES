﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES_GATEWAY.Device
{
    public class DahuaStreamCamera : IStreamCamera
    {
        public string CaptureStream(string CameraIP, string CameraUsername, string CameraPassword, string type, int number, string path)
        {
            string imageUrl = $"http://{CameraIP}/cgi-bin/snapshot.cgi";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imageUrl);
            request.Timeout = 7000;
            request.Credentials = new NetworkCredential(CameraUsername, CameraPassword);

            try
            {
                // Gửi yêu cầu và nhận phản hồi
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Đọc dữ liệu từ phản hồi
                using (Stream stream = response.GetResponseStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);

                        var now = DateTime.Now;

                        string currentYear = now.ToString("yyyy");
                        string currentMonth = now.ToString("MM");
                        string currentDay = now.ToString("dd");

                        string capturedTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        var typeString = type == "IN" ? "anhvao" : "anhra";

                        var fileName = capturedTime + $"_{typeString}" + $"_{number.ToString()}" + ".jpg";

                        string folderPath = Path.Combine(path, currentYear, currentMonth, currentDay);
                        string imgFileName = Path.Combine(folderPath, fileName);

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // Lưu ảnh vào một tệp
                        File.WriteAllBytes(imgFileName, ms.ToArray());

                        return imgFileName;
                    }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return null;
            }
        }

        public string GetRtspUrl(string CameraIP, string CameraPort, string CameraUsername, string CameraPassword)
        {
            string url = $"rtsp://{CameraUsername}:{CameraPassword}@{CameraIP}:{CameraPort}/cam/realmonitor?channel=1&subtype=0";
            return url;
        }
    }
}
