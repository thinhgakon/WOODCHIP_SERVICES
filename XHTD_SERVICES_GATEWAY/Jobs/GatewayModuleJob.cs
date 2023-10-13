using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using XHTD_SERVICES.Data.Repositories;
using XHTD_SERVICES_GATEWAY.Models.Response;
using System.Runtime.InteropServices;
using XHTD_SERVICES.Helper;
using Microsoft.AspNet.SignalR.Client;
using XHTD_SERVICES.Data.Common;
using Autofac;
using XHTD_SERVICES_GATEWAY.Business;
using XHTD_SERVICES.Data.Dtos;
using CHCNetSDK;
using System.IO;

namespace XHTD_SERVICES_GATEWAY.Jobs
{
    public class GatewayModuleJob : IJob
    {
        protected readonly RfidRepository _rfidRepository;

        protected readonly GatewayLogger _gatewayLogger;

        private IntPtr h21 = IntPtr.Zero;

        private static bool DeviceConnected = false;

        private List<CardNoLog> tmpCardNoLst_In = new List<CardNoLog>();

        private List<CardNoLog> tmpCardNoLst_Out = new List<CardNoLog>();

        private List<CardNoLog> tmpInvalidCardNoLst = new List<CardNoLog>();

        protected const string C3400_CBV_IP_ADDRESS = "10.0.9.1";

        [DllImport(@"C:\\Windows\\System32\\plcommpro.dll", EntryPoint = "Connect")]
        public static extern IntPtr Connect(string Parameters);

        [DllImport(@"C:\\Windows\\System32\\plcommpro.dll", EntryPoint = "PullLastError")]
        public static extern int PullLastError();

        [DllImport(@"C:\\Windows\\System32\\plcommpro.dll", EntryPoint = "GetRTLog")]
        public static extern int GetRTLog(IntPtr h, ref byte buffer, int buffersize);

        #region Camera variables
        private uint iLastErr = 0;
        private int m_lUserID = -1;
        private bool m_bInitSDK = false;
        private int m_lRealHandle = -1;
        private string str;

        CHCNet.REALDATACALLBACK RealData = null;
        public CHCNet.NET_DVR_PTZPOS m_struPtzCfg;
        #endregion

        public GatewayModuleJob(
            RfidRepository rfidRepository,
            GatewayLogger gatewayLogger
            )
        {
            _rfidRepository = rfidRepository;
            _gatewayLogger = gatewayLogger;

            // Login camera
            m_bInitSDK = CHCNet.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                return;
            }
            else
            {
                LoginCameraDahua();
            }
        }

        private void LoginCameraDahua()
        {
            if (m_lUserID < 0)
            {
                string DVRIPAddress = "10.15.15.33";
                short DVRPortNumber = short.Parse("8002");
                string DVRUserName = "admin";
                string DVRPassword = "123456a@";

                CHCNet.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNet.NET_DVR_DEVICEINFO_V30();

                m_lUserID = CHCNet.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNet.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V30 failed, error code = " + iLastErr;

                    _gatewayLogger.LogInfo("Khong the login camera");
                    _gatewayLogger.LogInfo("----------------------------");

                    return;
                }

                _gatewayLogger.LogInfo("Login camera thanh cong");
                _gatewayLogger.LogInfo("----------------------------");
            }
            else
            {
                if (m_lRealHandle >= 0)
                {
                    return;
                }

                if (!CHCNet.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNet.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code = " + iLastErr;
                    return;
                }
                m_lUserID = -1;
            }
            return;
        }

        private string CaptureCameraDahua()
        {
            int lChannel = short.Parse("1");

            CHCNet.NET_DVR_JPEGPARA lpJpegPara = new CHCNet.NET_DVR_JPEGPARA
            {
                wPicQuality = 0,
                wPicSize = 0xff
            };

            string rootPath = "C:\\log4net";

            string capturedTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            //string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Captures");
            string folderPath = Path.Combine(rootPath, "GatewayCaptures");
            string imgFileName = Path.Combine(folderPath, capturedTime + ".jpg");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!CHCNet.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, imgFileName))
            {
                iLastErr = CHCNet.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code = " + iLastErr;

                _gatewayLogger.LogInfo("Chup anh khong thanh cong.");
            }
            else
            {
                str = "Chụp ảnh thành công!";
                _gatewayLogger.LogInfo($"Chup anh thanh cong: {imgFileName}");
                return imgFileName;
            }
            return "";
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await Task.Run(async () =>
            {
                _gatewayLogger.LogInfo("Start gateway service");
                _gatewayLogger.LogInfo("----------------------------");

                AuthenticateGatewayModule();
            });                                                                                                                     
        }

        public void AuthenticateGatewayModule()
        {
            // 1. Connect Device
            while (!DeviceConnected)
            {
                ConnectGatewayModule();
            }

            // 2. Đọc dữ liệu từ thiết bị
            ReadDataFromC3400();
        }

        public bool ConnectGatewayModule()
        {
            var ipAddress = "10.15.15.40";
            var portNumber = 4370;
            try
            {
                string str = $"protocol=TCP,ipaddress={ipAddress},port={portNumber},timeout=2000,passwd=";
                int ret = 0;
                if (IntPtr.Zero == h21)
                {
                    h21 = Connect(str);
                    if (h21 != IntPtr.Zero)
                    {
                        _gatewayLogger.LogInfo($"Connected to C3-400 {ipAddress}");

                        DeviceConnected = true;
                    }
                    else
                    {
                        _gatewayLogger.LogInfo($"Connect to C3-400 {ipAddress} failed");

                        ret = PullLastError();
                        DeviceConnected = false;
                    }
                }
                return DeviceConnected;
            }
            catch (Exception ex)
            {
                _gatewayLogger.LogInfo($@"Connect to C3-400 {ipAddress} error: {ex.Message}");
                return false;
            }
        }

        public async void ReadDataFromC3400()
        {
            _gatewayLogger.LogInfo("Reading RFID from C3-400 ...");

            if (DeviceConnected)
            {
                while (DeviceConnected)
                {
                    int ret = 0, buffersize = 256;
                    string str = "";
                    string[] tmp = null;
                    byte[] buffer = new byte[256];

                    if (IntPtr.Zero != h21)
                    {
                        ret = GetRTLog(h21, ref buffer[0], buffersize);
                        if (ret >= 0)
                        {
                            try {
                                str = Encoding.Default.GetString(buffer);
                                tmp = str.Split(',');

                                // Bắt đầu xử lý khi nhận diện được RFID
                                if (tmp[2] != "0" && tmp[2] != "")
                                {
                                    var cardNoCurrent = tmp[2]?.ToString();
                                    var doorCurrent = tmp[3]?.ToString();
                                    var timeCurrent = tmp[0]?.ToString();

                                    // 1.Xác định xe cân vào / ra
                                    var isLuongVao = doorCurrent == "1"
                                                    || doorCurrent == "2";

                                    var isLuongRa = doorCurrent == "3"
                                                    || doorCurrent == "4";

                                    if (isLuongVao)
                                    {
                                        if (Program.IsLockingRfidIn) { 
                                            _gatewayLogger.LogInfo($"== Cong VAO dang xu ly => Ket thuc {cardNoCurrent} == ");
                                            //continue;
                                        }
                                    }

                                    if (isLuongRa)
                                    {
                                        if (Program.IsLockingRfidOut) { 
                                            _gatewayLogger.LogInfo($"== Cong RA dang xu ly => Ket thuc {cardNoCurrent} == ");

                                            //continue;
                                        }
                                    }

                                    // 2. Loại bỏ các tag đã check trước đó
                                    if (tmpInvalidCardNoLst.Count > 10)
                                    { 
                                        tmpInvalidCardNoLst.RemoveRange(0, 3); 
                                    }

                                    if (tmpInvalidCardNoLst.Exists(x => x.CardNo.Equals(cardNoCurrent) && x.DateTime > DateTime.Now.AddSeconds(-15)))
                                    {
                                        _gatewayLogger.LogInfo($"Tag {cardNoCurrent} da duoc check truoc do => Ket thuc.");
                                        continue;
                                    }

                                    if (isLuongVao)
                                    {
                                        if (tmpCardNoLst_In.Count > 5)
                                        { 
                                            tmpCardNoLst_In.RemoveRange(0, 3); 
                                        }

                                        if (tmpCardNoLst_In.Exists(x => x.CardNo.Equals(cardNoCurrent) && x.DateTime > DateTime.Now.AddMinutes(-3)))
                                        {
                                            _gatewayLogger.LogInfo($"Tag {cardNoCurrent} da duoc check truoc do => Ket thuc.");
                                            continue;
                                        }
                                    }
                                    else if (isLuongRa)
                                    {
                                        if (tmpCardNoLst_Out.Count > 5) 
                                        { 
                                            tmpCardNoLst_Out.RemoveRange(0, 3); 
                                        }

                                        if (tmpCardNoLst_Out.Exists(x => x.CardNo.Equals(cardNoCurrent) && x.DateTime > DateTime.Now.AddMinutes(-3)))
                                        {
                                            _gatewayLogger.LogInfo($"Tag {cardNoCurrent} da duoc check truoc do => Ket thuc.");
                                            continue;
                                        }
                                    }

                                    _gatewayLogger.LogInfo("----------------------------");
                                    _gatewayLogger.LogInfo($"Tag: {cardNoCurrent}, door: {doorCurrent}, time: {timeCurrent}");
                                    _gatewayLogger.LogInfo("-----");

                                    var newCardNoLog = new CardNoLog { CardNo = cardNoCurrent, DateTime = DateTime.Now };
                                    if (isLuongVao)
                                    {
                                        tmpCardNoLst_In.Add(newCardNoLog);

                                        Program.IsLockingRfidIn = true;

                                        _gatewayLogger.LogInfo($"1. Xe VAO cong");
                                    }
                                    else
                                    {
                                        tmpCardNoLst_Out.Add(newCardNoLog);

                                        Program.IsLockingRfidOut = true;

                                        _gatewayLogger.LogInfo($"1. Xe RA cong");
                                    }

                                    // Chụp ảnh
                                    _gatewayLogger.LogInfo($"2. Thuc hien chup anh");
                                    var gatewayImage = CaptureCameraDahua();

                                    FileInfo fi = new FileInfo(gatewayImage);

                                    bool exists = fi.Exists;
                                    string justFileName = fi.Name;
                                    string fullFileName = fi.FullName;
                                    string extn = fi.Extension;
                                    string directoryName = fi.DirectoryName;
                                    long size = fi.Length;

                                    // Thực hiện nghiệp vụ
                                    var checkInOutData = new GatewayCheckInOutRequestDto
                                    {
                                        CheckTime = DateTime.Now,
                                        RfId = cardNoCurrent,
                                        File = new FileDto
                                        {
                                            //ByteData = Convert.FromBase64String(FileHelper.ConvertImageToBase64(x.Attachment.Url)),
                                            ByteData = FileHelper.ConvertImageToBase64(gatewayImage),
                                            Name = justFileName,
                                            Extension = extn,
                                        }
                                    };

                                    _gatewayLogger.LogInfo($"3. Gui du lieu len MMES");
                                    var syncResponse = DIBootstrapper.Init().Resolve<ScaleApiLib>().SyncGatewayDataToDMS(checkInOutData);

                                    var currentTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                                    if (isLuongVao)
                                    {
                                        _gatewayLogger.LogInfo($"4. Giai phong RFID IN");

                                        Program.IsLockingRfidIn = false;
                                    }
                                    else if (isLuongRa)
                                    {
                                        _gatewayLogger.LogInfo($"4. Giai phong RFID OUT");

                                        Program.IsLockingRfidOut = false;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _gatewayLogger.LogError($@"Co loi xay ra khi xu ly RFID {ex.StackTrace} {ex.Message} ");
                                continue;
                            }
                        }
                        else
                        {
                            _gatewayLogger.LogWarn("No data. Reconnect ...");
                            DeviceConnected = false;
                            h21 = IntPtr.Zero;

                            AuthenticateGatewayModule();
                        }
                    }
                }
            }
            else
            {
                DeviceConnected = false;
                h21 = IntPtr.Zero;

                AuthenticateGatewayModule();
            }
        }
    }
}
