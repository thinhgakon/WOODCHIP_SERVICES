using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using XHTD_SERVICES.Data.Repositories;
using XHTD_SERVICES_GATEWAY.Models.Response;
using System.Runtime.InteropServices;
using XHTD_SERVICES.Helper;
using XHTD_SERVICES.Data.Common;
using Autofac;
using XHTD_SERVICES_GATEWAY.Business;
using XHTD_SERVICES.Data.Dtos;
using CHCNetSDK;
using System.IO;
using XHTD_SERVICES_GATEWAY.Device;
using XHTD_SERVICES.Data.Entities;

namespace XHTD_SERVICES_GATEWAY.Jobs
{
    public partial class GatewayModuleJob : IJob
    {
        protected readonly RfidRepository _rfidRepository;

        protected readonly GatewayLogger _gatewayLogger;

        private IntPtr h21 = IntPtr.Zero;

        private static bool DeviceConnected = false;

        private List<CardNoLog> tmpCardNoLst_In = new List<CardNoLog>();

        private List<CardNoLog> tmpCardNoLst_Out = new List<CardNoLog>();

        private List<CardNoLog> tmpInvalidCardNoLst = new List<CardNoLog>();

        protected const string C3400_CBV_IP_ADDRESS = "192.168.1.247";

        [DllImport(@"C:\\Windows\\System32\\plcommpro.dll", EntryPoint = "Connect")]
        public static extern IntPtr Connect(string Parameters);

        [DllImport(@"C:\\Windows\\System32\\plcommpro.dll", EntryPoint = "PullLastError")]
        public static extern int PullLastError();

        [DllImport(@"C:\\Windows\\System32\\plcommpro.dll", EntryPoint = "GetRTLog")]
        public static extern int GetRTLog(IntPtr h, ref byte buffer, int buffersize);

        public GatewayModuleJob(
            RfidRepository rfidRepository,
            GatewayLogger gatewayLogger
            )
        {
            _rfidRepository = rfidRepository;
            _gatewayLogger = gatewayLogger;
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

                LoadCamera();

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
            var ipAddress = C3400_CBV_IP_ADDRESS;
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

                                    List<FileDto> files = new List<FileDto>();
                                    
                                    try
                                    {
                                        IStreamCamera camera = new DahuaStreamCamera();
                                        var gatewayImage1 = camera.CaptureStream(CAMERA_1_IP, CAMERA_1_USERNAME, CAMERA_1_PASSWORD, "IN", 1, URL_IMAGE);

                                        if (!String.IsNullOrEmpty(gatewayImage1))
                                        {
                                            FileInfo fi1 = new FileInfo(gatewayImage1);

                                            bool exists = fi1.Exists;
                                            string justFileName = fi1.Name;
                                            string fullFileName = fi1.FullName;
                                            string extn = fi1.Extension;
                                            string directoryName = fi1.DirectoryName;
                                            long size = fi1.Length;

                                            FileDto fileDto1 = new FileDto();

                                            fileDto1.ByteData = FileHelper.ConvertImageToBase64(gatewayImage1);
                                            fileDto1.Name = justFileName;
                                            fileDto1.Extension = extn;

                                            files.Add(fileDto1);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _gatewayLogger.LogInfo($"Chup anh {CAMERA_1_IP} khong thanh cong");
                                    }

                                    try
                                    {
                                        IStreamCamera camera = new DahuaStreamCamera();
                                        var gatewayImage2 = camera.CaptureStream(CAMERA_2_IP, CAMERA_2_USERNAME, CAMERA_2_PASSWORD, "OUT", 2, URL_IMAGE);

                                        if (!String.IsNullOrEmpty(gatewayImage2))
                                        {
                                            FileInfo fi2 = new FileInfo(gatewayImage2);

                                            bool exists = fi2.Exists;
                                            string justFileName = fi2.Name;
                                            string fullFileName = fi2.FullName;
                                            string extn = fi2.Extension;
                                            string directoryName = fi2.DirectoryName;
                                            long size = fi2.Length;

                                            FileDto fileDto2 = new FileDto();

                                            fileDto2.ByteData = FileHelper.ConvertImageToBase64(gatewayImage2);
                                            fileDto2.Name = justFileName;
                                            fileDto2.Extension = extn;

                                            files.Add(fileDto2);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _gatewayLogger.LogInfo($"Chup anh {CAMERA_2_IP} khong thanh cong");
                                    }

                                    // Thực hiện nghiệp vụ
                                    var checkInOutData = new GatewayCheckInOutRequestDto
                                    {
                                        CheckTimeDateTimeValue = DateTime.Now,
                                        RfId = cardNoCurrent,
                                        Files = files
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
