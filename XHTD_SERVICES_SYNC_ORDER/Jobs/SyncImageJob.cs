using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using log4net;
using XHTD_SERVICES.Data.Repositories;
using RestSharp;
using XHTD_SERVICES_SYNC_ORDER.Models.Response;
using XHTD_SERVICES.Data.Models.Response;
using Newtonsoft.Json;
using XHTD_SERVICES_SYNC_ORDER.Models.Values;
using XHTD_SERVICES.Helper;
using XHTD_SERVICES.Helper.Models.Request;
using System.Threading;
using XHTD_SERVICES.Data.Dtos;

namespace XHTD_SERVICES_SYNC_ORDER.Jobs
{
    public class SyncImageJob : IJob
    {
        protected readonly ScaleBillRepository _scaleBillRepository;
        protected readonly ScaleImageRepository _scaleImageRepository;

        protected readonly SyncOrderLogger _syncOrderLogger;

        private static string strToken;

        public SyncImageJob(
            ScaleBillRepository scaleBillRepository,
            ScaleImageRepository scaleImageRepository,
            SyncOrderLogger syncOrderLogger
            )
        {
            _scaleBillRepository = scaleBillRepository;
            _scaleImageRepository = scaleImageRepository;
            _syncOrderLogger = syncOrderLogger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await Task.Run(async () =>
            {
                await SyncImageProcess();
            });
        }

        public async Task SyncImageProcess()
        {
            _syncOrderLogger.LogInfo("Start process Sync Image job");

            GetToken();

            List<ScaleImageDto> scaleImages = _scaleImageRepository.GetList();

            if (scaleImages == null || scaleImages.Count == 0)
            {
                _syncOrderLogger.LogInfo("Tất cả ảnh phiếu cân đã được đồng bộ");
                return;
            }

            bool isSynced = await SyncScaleBillToDMS(scaleImages);
        }

        public void GetToken()
        {
            try
            {
                IRestResponse response = HttpRequest.GetWebsaleToken();

                var content = response.Content;

                var responseData = JsonConvert.DeserializeObject<GetMmesTokenResponse>(content);
                strToken = responseData?.data?.accessToken;
            }
            catch (Exception ex)
            {
                _syncOrderLogger.LogInfo("getToken error: " + ex.Message);
            }
        }

        public async Task<bool> SyncScaleBillToDMS(List<ScaleImageDto> scaleImages)
        {
            IRestResponse response = HttpRequest.SyncScaleImageToDMS(strToken, scaleImages);

            var content = response.Content;

            var responseData = JsonConvert.DeserializeObject<GetSyncResponse>(content);

            var successList = responseData.data?.success;

            var failList = responseData.data?.fails;

            if(successList != null) { 
                foreach ( var itemSuccess in successList )
                {
                    _syncOrderLogger.LogInfo($"Đồng bộ thành công: {itemSuccess.Code}");
                    await this._scaleBillRepository.UpdateSyncSuccess(itemSuccess.Code);
                }
            }

            if(failList != null) { 
                foreach (var itemFail in failList)
                {
                    _syncOrderLogger.LogInfo($"Đồng bộ thất bại: {itemFail.Code}");
                    await this._scaleBillRepository.UpdateSyncFail(itemFail.Code);
                }
            }

            return true;
        }
    }
}
