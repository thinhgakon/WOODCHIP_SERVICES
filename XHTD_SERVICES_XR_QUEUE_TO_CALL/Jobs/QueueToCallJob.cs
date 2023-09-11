﻿using System;
using System.Threading.Tasks;
using Quartz;
using XHTD_SERVICES.Data.Repositories;

namespace XHTD_SERVICES_XR_QUEUE_TO_CALL.Jobs
{
    public class QueueToCallJob : IJob
    {
        protected readonly StoreOrderOperatingRepository _storeOrderOperatingRepository;

        protected readonly TroughRepository _troughRepository;

        protected readonly CallToTroughRepository _callToTroughRepository;

        protected readonly QueueToCallLogger _queueToCallLogger;

        private static string CODE_MACHINE_MAIN = "10";

        private static string CODE_MACHINE_9 = "9";

        private static string CODE_MACHINE_10 = "10";

        public QueueToCallJob(
            StoreOrderOperatingRepository storeOrderOperatingRepository,
            TroughRepository troughRepository,
            CallToTroughRepository callToTroughRepository,
            QueueToCallLogger queueToCallLogger
            )
        {
            _storeOrderOperatingRepository = storeOrderOperatingRepository;
            _troughRepository = troughRepository;
            _callToTroughRepository = callToTroughRepository;
            _queueToCallLogger = queueToCallLogger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await Task.Run(() =>
            {
                QueueToCallProcess();
            });
        }

        public async void QueueToCallProcess()
        {
            _queueToCallLogger.LogInfo("Start process XR QueueToCall service");

            try { 
                // 1. Lay danh sach don hang chua duoc xep vao may xuat
                var orders = await _storeOrderOperatingRepository.GetXiMangRoiOrdersAddToQueueToCall();
                if (orders == null || orders.Count == 0)
                {
                    _queueToCallLogger.LogInfo($"Khong co don hang nao de them vao hang doi");
                    return;
                }

                // 2. Voi moi don hang o B1 thi thuc hien
                // 3. Tim may xuat hien tai co it khoi luong don nhat (tuong ung voi type product)
                // 4. Tim STT lon nhat trong may tim duoc o B3: maxIndex
                // 5. Them don hang vao may o B3 voi index = maxIndex + 1
                foreach (var order in orders)
                {
                    var orderId = (int)order.OrderId;
                    var deliveryCode = order.DeliveryCode;
                    var vehicle = order.Vehicle;
                    var sumNumber = (decimal)order.SumNumber;
                    var typeProduct = order.TypeProduct;

                    // Đẩy tối đa 02 xe vào máng 10, còn lại đẩy vào máng 9
                    var machineCode = CODE_MACHINE_10;

                    var numberOrderInQueueMachine10 = _callToTroughRepository.GetNumberOrderInQueue(CODE_MACHINE_10);

                    _queueToCallLogger.LogInfo($"Hien tai co {numberOrderInQueueMachine10} order dang cho tai machine 10");

                    if (numberOrderInQueueMachine10 >= 2)
                    {
                        machineCode = CODE_MACHINE_9;
                    }

                    _queueToCallLogger.LogInfo($"Thuc hien them orderId {orderId} deliveryCode {deliveryCode} vao may {machineCode}");

                    if (!String.IsNullOrEmpty(machineCode)){ 
                        await _callToTroughRepository.AddItem(orderId, deliveryCode, vehicle, machineCode, sumNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                _queueToCallLogger.LogInfo($"Errrrorrr: {ex.Message} ==== {ex.StackTrace} ===== {ex.InnerException}");
            }
        }
    }
}
