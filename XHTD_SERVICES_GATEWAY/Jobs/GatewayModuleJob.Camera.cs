using CHCNetSDK;
using NetSDKCS;
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
        #region Camera variables
        //private uint iLastErr = 0;
        //private string str;
        private int m_lUserID_1 = -1;
        private int m_lUserID_2 = -1;
        private int m_lRealHandle_1 = -1;
        private int m_lRealHandle_2 = -1;
        private bool m_bInitSDK = false;

        CHCNet.REALDATACALLBACK RealData = null;
        public CHCNet.NET_DVR_PTZPOS m_struPtzCfg;
        #endregion

        #region Field Camera Dahua
        private const int m_WaitTime = 5000;
        private const int SyncFileSize = 5 * 1024 * 1204;
        private static fDisConnectCallBack m_DisConnectCallBack;
        private static fHaveReConnectCallBack m_ReConnectCallBack;
        private static fRealDataCallBackEx2 m_RealDataCallBackEx2;
        private static fSnapRevCallBack m_SnapRevCallBack;

        private IntPtr m_LoginID_1 = IntPtr.Zero;
        private IntPtr m_LoginID_2 = IntPtr.Zero;
        private NET_DEVICEINFO_Ex m_DeviceInfo;
        private IntPtr m_RealPlayID_1 = IntPtr.Zero;
        private IntPtr m_RealPlayID_2 = IntPtr.Zero;
        private uint m_SnapSerialNum = 1;
        private bool m_IsInSave = false;
        private int SpeedValue = 4;
        private const int MaxSpeed = 8;
        private const int MinSpeed = 1;
        #endregion

        #region Camera functions
        private void LoadCamera()
        {
            m_DisConnectCallBack = new fDisConnectCallBack(DisConnectCallBack);
            m_ReConnectCallBack = new fHaveReConnectCallBack(ReConnectCallBack);
            m_RealDataCallBackEx2 = new fRealDataCallBackEx2(RealDataCallBackEx);
            m_SnapRevCallBack = new fSnapRevCallBack(SnapRevCallBack);
            try
            {
                NETClient.Init(m_DisConnectCallBack, IntPtr.Zero, null);
                NETClient.SetAutoReconnect(m_ReConnectCallBack, IntPtr.Zero);
                NETClient.SetSnapRevCallBack(m_SnapRevCallBack, IntPtr.Zero);
                //InitOrLogoutUI();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Process.GetCurrentProcess().Kill();
            }

            LoginCameraDahua(ref m_LoginID_1, "Camera1IP", "Camera1Port", "Camera1Username", "Camera1Password");
        }

        private void LoginCameraHikvision(ref int m_lUserID_1, string CameraIP, string CameraPort, string CameraUsername, string CameraPassword)
        {
            if (m_lUserID_1 < 0)
            {
                string DVRIPAddress = "";
                short DVRPortNumber = 1;
                string DVRUserName = "";
                string DVRPassword = "";

                CHCNet.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNet.NET_DVR_DEVICEINFO_V30();

                m_lUserID_1 = CHCNet.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID_1 < 0)
                {
                    var iLastErr = CHCNet.NET_DVR_GetLastError();
                    var str = "Đăng nhập camera không thành công, error code = " + iLastErr;
                    //MessageBox.Show(str, "Lỗi kết nối camera", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                if (m_lRealHandle_1 >= 0)
                {
                    //MessageBox.Show("Please stop live view first!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!CHCNet.NET_DVR_Logout(m_lUserID_1))
                {
                    var iLastErr = CHCNet.NET_DVR_GetLastError();
                    var str = "NET_DVR_Logout failed, error code = " + iLastErr;
                    //MessageBox.Show(str, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                m_lUserID_1 = -1;
            }
            return;
        }

        private void LoginCameraDahua(ref IntPtr m_LoginID_1, string CameraIP, string CameraPort, string CameraUsername, string CameraPassword)
        {
            if (IntPtr.Zero == m_LoginID_1)
            {
                string DVRIPAddress = "192.168.1.248";
                string DVRPortNumber = "37777";
                string DVRUserName = "admin";
                string DVRPassword = "123456a@";

                ushort port = 0;
                try
                {
                    port = Convert.ToUInt16(DVRPortNumber);
                }
                catch
                {
                    //MessageBox.Show("Input port error(输入端口错误)!");
                    return;
                }
                m_DeviceInfo = new NET_DEVICEINFO_Ex();
                m_LoginID_1 = NETClient.LoginWithHighLevelSecurity(DVRIPAddress, port, DVRUserName, DVRPassword, EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
                if (IntPtr.Zero == m_LoginID_1)
                {
                    //MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
            }
            else
            {
                bool result = NETClient.Logout(m_LoginID_1);
                if (!result)
                {
                    //MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                m_LoginID_1 = IntPtr.Zero;
            }
        }

        //private void LiveViewCameraHikvision(ref int m_lUserID_1, ref int m_lRealHandle_1, PictureBox cameraViewBox1)
        //{
        //    if (m_lUserID_1 < 0)
        //    {
        //        //MessageBox.Show("Please login to the device first!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    if (m_lRealHandle_1 < 0)
        //    {
        //        CHCNet.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNet.NET_DVR_PREVIEWINFO
        //        {
        //            hPlayWnd = cameraViewBox1.Handle,
        //            lChannel = short.Parse("1"),
        //            dwStreamType = 0,
        //            dwLinkMode = 0,
        //            bBlocked = true,
        //            dwDisplayBufNum = 1,
        //            byProtoType = 0,
        //            byPreviewMode = 0
        //        };

        //        if (RealData == null)
        //        {
        //            RealData = new CHCNet.REALDATACALLBACK(RealDataCallBack);
        //        }

        //        IntPtr pUser = new IntPtr();

        //        m_lRealHandle_1 = CHCNet.NET_DVR_RealPlay_V40(m_lUserID_1, ref lpPreviewInfo, null/*RealData*/, pUser);
        //        if (m_lRealHandle_1 < 0)
        //        {
        //            var iLastErr = CHCNet.NET_DVR_GetLastError();
        //            var str = "NET_DVR_RealPlay_V40 failed, error code = " + iLastErr;
        //            MessageBox.Show(str, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        if (!CHCNet.NET_DVR_StopRealPlay(m_lRealHandle_1))
        //        {
        //            var iLastErr = CHCNet.NET_DVR_GetLastError();
        //            var str = "NET_DVR_StopRealPlay failed, error code = " + iLastErr;
        //            MessageBox.Show(str, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }
        //        m_lRealHandle_1 = -1;
        //    }
        //    return;
        //}

        //private void LiveViewCameraDahua(ref IntPtr m_LoginID_1, ref IntPtr m_RealPlayID_1, PictureBox cameraViewBox1)
        //{
        //    if (IntPtr.Zero == m_RealPlayID_1)
        //    {
        //        // realplay 监视
        //        EM_RealPlayType type;
        //        //if (streamtype_comboBox.SelectedIndex == 0)
        //        //{
        //        type = EM_RealPlayType.Realplay;
        //        //}
        //        //else
        //        //{
        //        //    type = EM_RealPlayType.Realplay_1;
        //        //}
        //        m_RealPlayID_1 = NETClient.RealPlay(m_LoginID_1, 0, cameraViewBox1.Handle, type);
        //        if (IntPtr.Zero == m_RealPlayID_1)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        NETClient.SetRealDataCallBack(m_RealPlayID_1, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
        //        //start_realplay_button.Text = "StopReal(停止监视)";
        //        //channel_comboBox.Enabled = false;
        //        //streamtype_comboBox.Enabled = false;
        //        //save_button.Enabled = true;
        //    }
        //    else
        //    {
        //        // stop realplay 关闭监视
        //        bool ret = NETClient.StopRealPlay(m_RealPlayID_1);
        //        if (!ret)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        m_RealPlayID_1 = IntPtr.Zero;
        //        //start_realplay_button.Text = "StartReal(开始监视)";
        //        //realplay_pictureBox.Refresh();
        //        //channel_comboBox.Enabled = true;
        //        //streamtype_comboBox.Enabled = true;
        //        //save_button.Enabled = false;
        //        if (m_IsInSave)
        //        {
        //            m_IsInSave = false;
        //            //save_button.Text = "StartSave(开始保存)";
        //        }
        //    }
        //}

        private void RealDataCallBack(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser)
        {
            if (dwBufSize > 0)
            {
                byte[] sData = new byte[dwBufSize];
                Marshal.Copy(pBuffer, sData, 0, (int)dwBufSize);

                string str = "data.ps";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)dwBufSize;
                fs.Write(sData, 0, iLen);
                fs.Close();
            }
        }

        private string CaptureCameraHikvision(int m_lUserID_1)
        {
            int lChannel = short.Parse("1");

            CHCNet.NET_DVR_JPEGPARA lpJpegPara = new CHCNet.NET_DVR_JPEGPARA
            {
                wPicQuality = 0,
                wPicSize = 0xff
            };

            string rootPath = "C:\\MBF6\\MMES";

            string capturedTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            //string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Captures");
            string folderPath = Path.Combine(rootPath, "Captures");
            string imgFileName = Path.Combine(folderPath, capturedTime + ".jpg");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!CHCNet.NET_DVR_CaptureJPEGPicture(m_lUserID_1, lChannel, ref lpJpegPara, imgFileName))
            {
                var iLastErr = CHCNet.NET_DVR_GetLastError();
                var str = "NET_DVR_CaptureJPEGPicture failed, error code = " + iLastErr;
                //MessageBox.Show(str, "Lỗi");
                //Close();
            }
            else
            {
                //var str = "Chụp ảnh thành công!";
                //MessageBox.Show(str, "Thông báo");

                // Nếu ko sleep giữa 2 lần chụp camera khác nhau thì chỉ chụp 1 ảnh: chỉ run 1 lần CaptureScaleImage
                Thread.Sleep(500);

                return imgFileName;
            }
            return "";
        }

        private string CaptureCameraDahua(IntPtr m_RealPlayID, string type, int number)
        {
            if (IntPtr.Zero == m_RealPlayID)
            {
                //MessageBox.Show(this, "Please realplay first(请先打开监视)!");
                return "";
            }

            string path = "C:\\MBF6\\MMES";
            var now = DateTime.Now;

            string currentYear = now.ToString("yyyy");
            string currentMonth = now.ToString("MM");
            string currentDay = now.ToString("dd");

            string capturedTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var typeString = type == "IN" ? "anhvao" : "anhra";

            var fileName = capturedTime + $"_{typeString}" + $"_{number.ToString()}" + ".jpg";

            string folderPath = Path.Combine(path, currentYear, currentMonth, currentDay);
            string imgFileName = Path.Combine(folderPath, fileName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            bool result = NETClient.CapturePicture(m_RealPlayID, imgFileName, EM_NET_CAPTURE_FORMATS.JPEG);
            if (!result)
            {
                //MessageBox.Show(this, NETClient.GetLastError());
                return "";
            }
            //MessageBox.Show(this, "client capture success(本地抓图成功)!");

            return imgFileName;
        }

        public string ConvertImageToBase64(string filePath)
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(filePath);

                string base64Image = Convert.ToBase64String(imageData);

                return base64Image;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Có lỗi xảy ra khi chuyển đổi hình ảnh sang định dạng base64: " + ex.Message, "Lỗi");
                return null;
            }
        }
        #endregion

        #region CallBack 回调
        private void DisConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            //this.BeginInvoke((Action)UpdateDisConnectUI);
        }

        private void UpdateDisConnectUI()
        {
            //this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo) --- Offline(离线)";
        }

        private void ReConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            //this.BeginInvoke((Action)UpdateReConnectUI);
        }

        private void UpdateReConnectUI()
        {
            //this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo) --- Online(在线)";
        }

        private void RealDataCallBackEx(IntPtr lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr param, IntPtr dwUser)
        {
            //do something such as save data,send data,change to YUV. 比如保存数据，发送数据，转成YUV等.
        }

        private void SnapRevCallBack(IntPtr lLoginID, IntPtr pBuf, uint RevLen, uint EncodeType, uint CmdSerial, IntPtr dwUser)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "capture";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (EncodeType == 10) //.jpg
            {
                DateTime now = DateTime.Now;
                string fileName = "async" + CmdSerial.ToString() + ".jpg";
                string filePath = path + "\\" + fileName;
                byte[] data = new byte[RevLen];
                Marshal.Copy(pBuf, data, 0, (int)RevLen);
                using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    stream.Write(data, 0, (int)RevLen);
                    stream.Flush();
                    stream.Dispose();
                }
            }
        }
        #endregion
    }
}