﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using XHTD_SERVICES.Data.Repositories;
using XHTD_SERVICES_GATEWAY.Models.Response;
using XHTD_SERVICES.Data.Models.Response;
using System.Runtime.InteropServices;
using XHTD_SERVICES.Device.PLCM221;
using XHTD_SERVICES.Device;
using XHTD_SERVICES.Data.Entities;
using XHTD_SERVICES.Helper;
using Microsoft.AspNet.SignalR.Client;
using System.Threading;
using XHTD_SERVICES.Data.Common;
using Autofac;
using XHTD_SERVICES_GATEWAY.Business;
using XHTD_SERVICES_GATEWAY.Hubs;
using System.Net.NetworkInformation;

namespace XHTD_SERVICES_GATEWAY.Jobs
{
    public class GatewayModuleJob : IJob
    {
        protected readonly StoreOrderOperatingRepository _storeOrderOperatingRepository;

        protected readonly RfidRepository _rfidRepository;

        protected readonly CategoriesDevicesRepository _categoriesDevicesRepository;

        protected readonly CategoriesDevicesLogRepository _categoriesDevicesLogRepository;

        protected readonly SystemParameterRepository _systemParameterRepository;

        protected readonly PLCBarrier _barrier;

        protected readonly TCPTrafficLight _trafficLight;

        protected readonly Notification _notification;

        protected readonly GatewayLogger _gatewayLogger;

        private IntPtr h21 = IntPtr.Zero;

        private static bool DeviceConnected = false;

        private List<CardNoLog> tmpCardNoLst_In = new List<CardNoLog>();

        private List<CardNoLog> tmpCardNoLst_Out = new List<CardNoLog>();

        private List<CardNoLog> tmpInvalidCardNoLst = new List<CardNoLog>();

        private tblCategoriesDevice c3400, rfidRa1, rfidRa2, rfidVao1, rfidVao2, m221, barrierVao, barrierRa, trafficLightIn, trafficLightOut;

        protected const string CBV_ACTIVE = "CBV_ACTIVE";

        private static bool isActiveService = true;

        protected const string C3400_CBV_IP_ADDRESS = "10.0.9.1";

        protected const string M221_CBV_IP_ADDRESS = "10.0.9.2";

        protected const string C3400_951_2_IP_ADDRESS = "10.0.9.5";

        private IHubProxy HubProxy { get; set; }

        private string ServerURI = URIConfig.SIGNALR_GATEWAY_SERVICE_URL;

        private HubConnection Connection { get; set; }

        [DllImport(@"C:\\Windows\\System32\\plcommpro.dll", EntryPoint = "Connect")]
        public static extern IntPtr Connect(string Parameters);

        [DllImport(@"C:\\Windows\\System32\\plcommpro.dll", EntryPoint = "PullLastError")]
        public static extern int PullLastError();

        [DllImport(@"C:\\Windows\\System32\\plcommpro.dll", EntryPoint = "GetRTLog")]
        public static extern int GetRTLog(IntPtr h, ref byte buffer, int buffersize);

        public GatewayModuleJob(
            StoreOrderOperatingRepository storeOrderOperatingRepository, 
            RfidRepository rfidRepository,
            CategoriesDevicesRepository categoriesDevicesRepository,
            CategoriesDevicesLogRepository categoriesDevicesLogRepository,
            SystemParameterRepository systemParameterRepository,
            PLCBarrier barrier,
            TCPTrafficLight trafficLight,
            Notification notification,
            GatewayLogger gatewayLogger
            )
        {
            _storeOrderOperatingRepository = storeOrderOperatingRepository;
            _rfidRepository = rfidRepository;
            _categoriesDevicesRepository = categoriesDevicesRepository;
            _categoriesDevicesLogRepository = categoriesDevicesLogRepository;
            _systemParameterRepository = systemParameterRepository;
            _barrier = barrier;
            _trafficLight = trafficLight;
            _notification = notification;
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
                // Connect Scale Hub
                ConnectScaleHubAsync();

                // Get System Parameters
                await LoadSystemParameters();

                if (!isActiveService)
                {
                    _gatewayLogger.LogInfo("Service cong bao ve dang TAT.");
                    return;
                }

                _gatewayLogger.LogInfo("Start gateway service");
                _gatewayLogger.LogInfo("----------------------------");

                // Get devices info
                await LoadDevicesInfo();

                AuthenticateGatewayModule();
            });                                                                                                                     
        }

        private async void ConnectScaleHubAsync()
        {
            Connection = new HubConnection(ServerURI);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("ScaleHub");
            try
            {
                await Connection.Start();
                _gatewayLogger.LogInfo($"Connected scale hub {ServerURI}");
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                _gatewayLogger.LogInfo($"Connect failed scale hub {ServerURI}");
            }
        }

        private void Connection_Closed()
        {
        }

        public async Task LoadSystemParameters()
        {
            var parameters = await _systemParameterRepository.GetSystemParameters();

            var activeParameter = parameters.FirstOrDefault(x => x.Code == CBV_ACTIVE);

            if (activeParameter == null || activeParameter.Value == "0")
            {
                isActiveService = false;
            }
            else
            {
                isActiveService = true;
            }
        }

        public async Task LoadDevicesInfo()
        {
            var devices = await _categoriesDevicesRepository.GetDevices("CBV");

            c3400 = devices.FirstOrDefault(x => x.Code == "CBV.C3-400");

            rfidVao1 = devices.FirstOrDefault(x => x.Code == "CBV.C3-400.RFID-IN-1");
            rfidVao2 = devices.FirstOrDefault(x => x.Code == "CBV.C3-400.RFID-IN-2");
            rfidRa1 = devices.FirstOrDefault(x => x.Code == "CBV.C3-400.RFID-OUT-1");
            rfidRa2 = devices.FirstOrDefault(x => x.Code == "CBV.C3-400.RFID-OUT-1");

            m221 = devices.FirstOrDefault(x => x.Code == "CBV.M221");

            barrierVao = devices.FirstOrDefault(x => x.Code == "CBV.M221.BRE-IN");
            barrierRa = devices.FirstOrDefault(x => x.Code == "CBV.M221.BRE-OUT");

            trafficLightIn = devices.FirstOrDefault(x => x.Code == "CBV.DGT-IN");
            trafficLightOut = devices.FirstOrDefault(x => x.Code == "CBV.DGT-OUT");
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
            var ipAddress = c3400?.IpAddress;
            try
            {
                string str = $"protocol=TCP,ipaddress={ipAddress},port={c3400?.PortNumber},timeout=2000,passwd=";
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

                        // Send SMS Brandname: theo chu kỳ 30 phút
                        try { 
                            if (Program.SendSmsLastTime == null || Program.SendSmsLastTime < DateTime.Now.AddMinutes(-30)) 
                            {
                                Program.SendSmsLastTime = DateTime.Now;

                                var SmsContent = "CBV failed. Ping: ";

                                Ping myPing = new Ping();
                                PingReply replyC3400CBV = myPing.Send(C3400_CBV_IP_ADDRESS, 1000);
                                PingReply replyM221CBV = myPing.Send(M221_CBV_IP_ADDRESS, 1000);
                                PingReply replyC34009512 = myPing.Send(C3400_951_2_IP_ADDRESS, 1000);

                                if (replyC3400CBV != null)
                                {
                                    SmsContent += $"C3400-CBV: {replyC3400CBV.Status}. ";
                                }

                                if (replyM221CBV != null)
                                {
                                    SmsContent += $"M221-CBV: {replyM221CBV.Status}. ";
                                }

                                if (replyC34009512 != null)
                                {
                                    SmsContent += $"C3400-951-2: {replyC34009512.Status}";
                                }

                                HttpRequest.SendSMSBrandName(SmsContent);
                                HttpRequest.SendSMSBrandName(SmsContent, "0773392020");
                            }
                        }
                        catch (Exception ex)
                        {
                            _gatewayLogger.LogInfo($"Send SMS error: {ex.Message}");
                        }

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
                                    var isLuongVao = doorCurrent == rfidVao1.PortNumberDeviceIn.ToString()
                                                    || doorCurrent == rfidVao2.PortNumberDeviceIn.ToString();

                                    var isLuongRa = doorCurrent == rfidRa1.PortNumberDeviceIn.ToString()
                                                    || doorCurrent == rfidRa2.PortNumberDeviceIn.ToString();

                                    if(isLuongVao)
                                    {
                                        if (Program.IsLockingRfidIn) { 
                                            _gatewayLogger.LogInfo($"== Cong VAO dang xu ly => Ket thuc {cardNoCurrent} == ");

                                            new GatewayHub().SendMessage("IS_LOCKING_RFID_IN", "1");

                                            //continue;
                                        }
                                        else
                                        {
                                            new GatewayHub().SendMessage("IS_LOCKING_RFID_IN", "0");
                                        }
                                    }

                                    if (isLuongRa)
                                    {
                                        if (Program.IsLockingRfidOut) { 
                                            _gatewayLogger.LogInfo($"== Cong RA dang xu ly => Ket thuc {cardNoCurrent} == ");

                                            new GatewayHub().SendMessage("IS_LOCKING_RFID_OUT", "1");

                                            //continue;
                                        }
                                        else
                                        {
                                            new GatewayHub().SendMessage("IS_LOCKING_RFID_OUT", "0");
                                        }
                                    }

                                    // Gửi signalr thông tin RFID cho chức năng nhận diện RFID trên app mobile
                                    SendRFIDInfo(isLuongVao, cardNoCurrent);

                                    // 2. Loại bỏ các tag đã check trước đó
                                    if (tmpInvalidCardNoLst.Count > 10)
                                    { 
                                        tmpInvalidCardNoLst.RemoveRange(0, 3); 
                                    }

                                    if (tmpInvalidCardNoLst.Exists(x => x.CardNo.Equals(cardNoCurrent) && x.DateTime > DateTime.Now.AddSeconds(-15)))
                                    {
                                        //_gatewayLogger.LogInfo($@"2. Tag da duoc check truoc do => Ket thuc.");
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
                                            //_gatewayLogger.LogInfo($@"2. Tag da duoc check truoc do => Ket thuc.");
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
                                            //_gatewayLogger.LogInfo($@"2. Tag da duoc check truoc do => Ket thuc.");
                                            continue;
                                        }
                                    }

                                    _gatewayLogger.LogInfo("----------------------------");
                                    _gatewayLogger.LogInfo($"Tag: {cardNoCurrent}, door: {doorCurrent}, time: {timeCurrent}");
                                    _gatewayLogger.LogInfo("-----");

                                    var inout = "";
                                    if (isLuongVao)
                                    {
                                        inout = "IN";
                                        _gatewayLogger.LogInfo($"1. Xe VAO cong");
                                    }
                                    else
                                    {
                                        inout = "OUT";
                                        _gatewayLogger.LogInfo($"1. Xe RA cong");
                                    }

                                    _gatewayLogger.LogInfo($"2. Kiem tra tag da check truoc do");

                                    // 3. Kiểm tra cardNoCurrent có hợp lệ hay không
                                    string vehicleCodeCurrent = _rfidRepository.GetVehicleCodeByCardNo(cardNoCurrent);

                                    if (!String.IsNullOrEmpty(vehicleCodeCurrent))
                                    {
                                        _gatewayLogger.LogInfo($"3. Tag hop le: vehicle={vehicleCodeCurrent}");
                                    }
                                    else
                                    {
                                        _gatewayLogger.LogInfo($"3. Tag KHONG hop le => Ket thuc.");

                                        await SendNotificationCBV(0, inout, cardNoCurrent, $"RFID {cardNoCurrent} không thuộc hệ thống");

                                        var newCardNoLog = new CardNoLog { CardNo = cardNoCurrent, DateTime = DateTime.Now };
                                        tmpInvalidCardNoLst.Add(newCardNoLog);

                                        continue;
                                    }

                                    // 4. Kiểm tra cardNoCurrent có đang chứa đơn hàng hợp lệ không
                                    tblStoreOrderOperating currentOrder = null;
                                    var isValidCardNo = false;

                                    if (isLuongVao)
                                    {
                                        currentOrder = await _storeOrderOperatingRepository.GetCurrentOrderEntraceGateway(vehicleCodeCurrent);

                                        isValidCardNo = OrderValidator.IsValidOrderEntraceGateway(currentOrder);
                                    }
                                    else if (isLuongRa)
                                    {
                                        currentOrder = await _storeOrderOperatingRepository.GetCurrentOrderExitGateway(vehicleCodeCurrent);

                                        isValidCardNo = OrderValidator.IsValidOrderExitGateway(currentOrder);
                                    }

                                    if (currentOrder == null)
                                    {
                                        _gatewayLogger.LogInfo($"4. Tag KHONG co don hang => Ket thuc.");

                                        await SendNotificationCBV(0, inout, cardNoCurrent, $"{vehicleCodeCurrent} - RFID {cardNoCurrent} không có đơn hàng");

                                        var newCardNoLog = new CardNoLog { CardNo = cardNoCurrent, DateTime = DateTime.Now };
                                        tmpInvalidCardNoLst.Add(newCardNoLog);

                                        continue;
                                    }
                                    else if (isValidCardNo == false)
                                    {
                                        _gatewayLogger.LogInfo($"4. Tag KHONG co don hang hop le => Ket thuc.");

                                        await SendNotificationCBV(0, inout, cardNoCurrent, $"{vehicleCodeCurrent} - RFID {cardNoCurrent} không có đơn hàng hợp lệ", currentOrder.DeliveryCode);

                                        var newCardNoLog = new CardNoLog { CardNo = cardNoCurrent, DateTime = DateTime.Now };
                                        tmpInvalidCardNoLst.Add(newCardNoLog);

                                        continue;
                                    }
                                    else
                                    {
                                        await SendNotificationCBV(1, inout, cardNoCurrent, $"{vehicleCodeCurrent} - RFID {cardNoCurrent} có đơn hàng hợp lệ", currentOrder.DeliveryCode);

                                        var newCardNoLog = new CardNoLog { CardNo = cardNoCurrent, DateTime = DateTime.Now };

                                        if (isLuongVao)
                                        {
                                            tmpCardNoLst_In.Add(newCardNoLog);

                                            Program.IsLockingRfidIn = true;
                                        }
                                        else if (isLuongRa)
                                        {
                                            tmpCardNoLst_Out.Add(newCardNoLog);

                                            Program.IsLockingRfidOut = true;
                                        }
                                    }

                                    var currentDeliveryCode = currentOrder.DeliveryCode;
                                    _gatewayLogger.LogInfo($"4. Tag co don hang hop le DeliveryCode = {currentDeliveryCode}");

                                    var isUpdatedOrder = false;
                                    bool isSuccessOpenBarrier = true;

                                    bool isNormalOrder = currentOrder.CatId != OrderCatIdCode.CLINKER
                                                      && currentOrder.TypeXK != OrderTypeXKCode.JUMBO
                                                      && currentOrder.TypeXK != OrderTypeXKCode.SLING;

                                    var currentTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                                    if (isLuongVao)
                                    {
                                        if (isNormalOrder)
                                        {
                                            isUpdatedOrder = await _storeOrderOperatingRepository.UpdateOrderConfirm2ByDeliveryCode(currentDeliveryCode);

                                            if (isUpdatedOrder)
                                            {
                                                SendInfoNotification($"{currentOrder.DriverUserName}", $"{currentDeliveryCode} vào cổng lúc {currentTime}");

                                                _gatewayLogger.LogInfo($"5. Đơn hàng thông thường (không phải CLINKER, JUMBO, SLING) =>  Đã xác thực trạng thái vào cổng");
                                            }
                                        }
                                        else
                                        {
                                            isUpdatedOrder = true;
                                            _gatewayLogger.LogInfo($"5. Đơn hàng nội bộ => Không update trạng thái vào cổng.");
                                        }

                                        if (isUpdatedOrder)
                                        {
                                            _gatewayLogger.LogInfo($"6. Mở barrier");
                                            isSuccessOpenBarrier = OpenBarrier("IN");

                                            Thread.Sleep(3000);

                                            _gatewayLogger.LogInfo($"7. Bật đèn xanh");
                                            if(TurnOnGreenTrafficLight("IN"))
                                            {
                                                _gatewayLogger.LogInfo($"7.2. Bật đèn xanh thành công");
                                            }
                                            else
                                            {
                                                _gatewayLogger.LogInfo($"7.2. Bật đèn xanh thất bại");
                                            }

                                            if (isNormalOrder)
                                            {
                                                try
                                                {
                                                    var SaledOrderWebSale = DIBootstrapper.Init().Resolve<ScaleApiLib>().SaleOrder(currentDeliveryCode);
                                                    _gatewayLogger.LogInfo($"7.3. Gọi API cập nhật in phiếu thành công {currentDeliveryCode}: Code={SaledOrderWebSale.Code} Message={SaledOrderWebSale.Message}");
                                                }
                                                catch (Exception ex)
                                                {
                                                    _gatewayLogger.LogInfo($"7.3. Gọi API cập nhật in phiếu ERROR:  {ex.Message} === {ex.StackTrace} === {ex.InnerException}");
                                                }
                                            }

                                            Thread.Sleep(12000);

                                            _gatewayLogger.LogInfo($"8. Bật đèn đỏ");
                                            if(TurnOnRedTrafficLight("IN"))
                                            {
                                                _gatewayLogger.LogInfo($"8.2. Bật đèn đỏ thành công");
                                            }
                                            else
                                            {
                                                _gatewayLogger.LogInfo($"8.2. Bật đèn đỏ thất bại");
                                            }
                                        }
                                        else
                                        {
                                            _gatewayLogger.LogInfo($"5. Confirm 2 failed.");
                                        }
                                    }
                                    else if (isLuongRa)
                                    {
                                        if (isNormalOrder)
                                        {
                                            isUpdatedOrder = await _storeOrderOperatingRepository.UpdateOrderConfirm8ByVehicleCode(vehicleCodeCurrent);

                                            if (isUpdatedOrder)
                                            {
                                                SendInfoNotification($"{currentOrder.DriverUserName}", $"{currentDeliveryCode} ra cổng lúc {currentTime}");

                                                _gatewayLogger.LogInfo($"5. Đơn hàng thông thường (không phải CLINKER, JUMBO, SLING) => Đã xác thực trạng thái ra cổng");
                                            }
                                        }
                                        else
                                        {
                                            isUpdatedOrder = true;
                                            _gatewayLogger.LogInfo($"5. Đơn hàng nội bộ => Không update trạng thái ra cổng.");
                                        }

                                        if (isUpdatedOrder)
                                        {
                                            _gatewayLogger.LogInfo($"6. Mở barrier");
                                            isSuccessOpenBarrier = OpenBarrier("OUT");

                                            Thread.Sleep(3000);

                                            _gatewayLogger.LogInfo($"7. Bật đèn xanh");
                                            if(TurnOnGreenTrafficLight("OUT"))
                                            {
                                                _gatewayLogger.LogInfo($"7.2. Bật đèn xanh thành công");
                                            }
                                            else
                                            {
                                                _gatewayLogger.LogInfo($"7.2. Bật đèn xanh thất bại");
                                            }

                                            Thread.Sleep(15000);

                                            _gatewayLogger.LogInfo($"8. Bật đèn đỏ");
                                            if(TurnOnRedTrafficLight("OUT"))
                                            {
                                                _gatewayLogger.LogInfo($"8.2. Bật đèn đỏ thành công");
                                            }
                                            else
                                            {
                                                _gatewayLogger.LogInfo($"8.2. Bật đèn đỏ thất bại");
                                            }
                                        }
                                        else
                                        {
                                            _gatewayLogger.LogInfo($"5. Confirm 8 failed.");
                                        }
                                    }

                                    if (isUpdatedOrder)
                                    {
                                        if (isSuccessOpenBarrier)
                                        {
                                            _gatewayLogger.LogInfo($"9. Ghi log thiet bi mo barrier");

                                            string luongText = isLuongVao ? "vào" : "ra";
                                            string deviceCode = isLuongVao ? "CBV.M221.BRE-IN" : "CBV.M221.BRE-OUT";
                                            var newLog = new CategoriesDevicesLogItemResponse
                                            {
                                                Code = deviceCode,
                                                ActionType = 1,
                                                ActionInfo = $"Mở barrier cho xe {currentOrder.Vehicle} {luongText}, theo đơn hàng {currentDeliveryCode}",
                                                ActionDate = DateTime.Now,
                                            };

                                            await _categoriesDevicesLogRepository.CreateAsync(newLog);
                                        }
                                        else
                                        {
                                            _gatewayLogger.LogInfo($"9. Mo barrier KHONG thanh cong");
                                        }
                                    }

                                    if (isLuongVao)
                                    {
                                        _gatewayLogger.LogInfo($"10. Giai phong RFID IN");

                                        Program.IsLockingRfidIn = false;
                                    }
                                    else if (isLuongRa)
                                    {
                                        _gatewayLogger.LogInfo($"10. Giai phong RFID OUT");

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

        public bool OpenBarrier(string luong)
        {
            var isConnectSuccessed = false;
            int count = 0;

            try { 
                int portNumberDeviceIn = luong == "IN" ? (int)barrierVao.PortNumberDeviceIn : (int)barrierRa.PortNumberDeviceIn;
                int portNumberDeviceOut = luong == "IN" ? (int)barrierVao.PortNumberDeviceOut : (int)barrierRa.PortNumberDeviceOut;

                while (!isConnectSuccessed && count < 6)
                {
                    count++;

                    _gatewayLogger.LogInfo($@"OpenBarrier: count={count}");

                    M221Result isConnected = _barrier.ConnectPLC(m221.IpAddress);

                    if (isConnected == M221Result.SUCCESS)
                    {
                        _barrier.ResetOutputPort(portNumberDeviceIn);

                        Thread.Sleep(500);

                        M221Result batLan1 = _barrier.ShuttleOutputPort(byte.Parse(portNumberDeviceIn.ToString()));

                        if (batLan1 == M221Result.SUCCESS)
                        {
                            _gatewayLogger.LogInfo($"Bat lan 1 thanh cong: {_barrier.GetLastErrorString()}");
                        }
                        else
                        {
                            _gatewayLogger.LogInfo($"Bat lan 1 that bai: {_barrier.GetLastErrorString()}");
                        }

                        Thread.Sleep(500);

                        M221Result batLan2 = _barrier.ShuttleOutputPort(byte.Parse(portNumberDeviceIn.ToString()));

                        if (batLan2 == M221Result.SUCCESS)
                        {
                            _gatewayLogger.LogInfo($"Bat lan 2 thanh cong: {_barrier.GetLastErrorString()}");
                        }
                        else
                        {
                            _gatewayLogger.LogInfo($"Bat lan 2 that bai: {_barrier.GetLastErrorString()}");
                        }

                        Thread.Sleep(500);

                        _barrier.Close();

                        _gatewayLogger.LogWarn($"OpenBarrier count={count} thanh cong");

                        isConnectSuccessed = true;
                    }
                    else
                    {
                        _gatewayLogger.LogWarn($"OpenBarrier count={count}: Ket noi PLC khong thanh cong {_barrier.GetLastErrorString()}");

                        Thread.Sleep(1000);
                    }
                }

                return isConnectSuccessed;
            }
            catch (Exception ex)
            {
                _gatewayLogger.LogInfo($"OpenBarrier Error: {ex.Message} == {ex.StackTrace} == {ex.InnerException}");
                return false;
            }
        }

        public string GetTrafficLightIpAddress(string code)
        {
            var ipAddress = "";

            if (code == "IN")
            {
                ipAddress = trafficLightIn?.IpAddress;
            }
            else if (code == "OUT")
            {
                ipAddress = trafficLightOut?.IpAddress;
            }

            return ipAddress;
        }

        public bool TurnOnGreenTrafficLight(string code)
        {
            var ipAddress = GetTrafficLightIpAddress(code);

            if (String.IsNullOrEmpty(ipAddress))
            {
                return false;
            }

            _gatewayLogger.LogInfo($"7.1. IP đèn: {ipAddress}");

            _trafficLight.Connect(ipAddress);

            return _trafficLight.TurnOnGreenOffRed();
        }

        public bool TurnOnRedTrafficLight(string code)
        {
            var ipAddress = GetTrafficLightIpAddress(code);

            if (String.IsNullOrEmpty(ipAddress))
            {
                return false;
            }

            _gatewayLogger.LogInfo($"8.1. IP đèn: {ipAddress}");

            _trafficLight.Connect(ipAddress);

            return _trafficLight.TurnOffGreenOnRed();
        }

        public async Task StartIfNeededAsync()
        {
            if (Connection.State == ConnectionState.Disconnected)
            {
                await Connection.Start();

                _gatewayLogger.LogInfo($"Reconnect Connection: {Connection.State}");
            }
        }

        private async Task SendNotificationCBV(int status, string inout, string cardNo, string message, string deliveryCode = "")
        {
            new GatewayHub().SendNotificationCBV(status, inout, cardNo, message, deliveryCode);
            //try
            //{
            //    await StartIfNeededAsync();

            //    HubProxy.Invoke("SendNotificationCBV", status, inout, cardNo, message, deliveryCode).Wait();

            //    _gatewayLogger.LogInfo($"SendNotificationCBV: status={status}, inout={inout}, cardNo={cardNo}, message={message}");
            //}
            //catch (Exception ex)
            //{
            //    _gatewayLogger.LogInfo($"SendNotificationCBV error: {ex.Message}");
            //}
        }

        public void SendRFIDInfo(bool isLuongVao, string cardNo)
        {
            try
            {
                if (isLuongVao)
                {
                    _notification.SendNotification(
                        "GATE_WAY_RFID",
                        null,
                        1,
                        cardNo,
                        0,
                        null,
                        null,
                        0,
                        null,
                        null,
                        null
                    );

                    //_gatewayLogger.LogInfo($"Sent entrace RFID to app: {cardNo}");
                }
                else
                {
                    _notification.SendNotification(
                       "GATE_WAY_OUT_RFID",
                       null,
                       1,
                       cardNo,
                       1,
                       null,
                       null,
                       0,
                       null,
                       null,
                       null
                   );

                    //_gatewayLogger.LogInfo($"Sent exit RFID to app: {cardNo}");
                }
            }
            catch (Exception ex)
            {
                _gatewayLogger.LogInfo($"SendNotification Ex: {ex.Message} == {ex.StackTrace} == {ex.InnerException}");
            }
        }

        public void SendInfoNotification(string receiver, string message)
        {
            try
            {
                _notification.SendInforNotification(receiver, message);
            }
            catch (Exception ex)
            {
                _gatewayLogger.LogInfo($"SendInfoNotification Ex: {ex.Message} == {ex.StackTrace} == {ex.InnerException}");
            }
        }
    }
}
