using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using log4net;
using XHTD_SERVICES.Data.Repositories;
using RestSharp;
using XHTD_SERVICES_SYNC_BRAVO.Models.Response;
using XHTD_SERVICES.Data.Models.Response;
using Newtonsoft.Json;
using XHTD_SERVICES_SYNC_BRAVO.Models.Values;
using XHTD_SERVICES.Helper;
using XHTD_SERVICES.Helper.Models.Request;
using System.Threading;
using XHTD_SERVICES.Data.Dtos;
using log4net;

namespace XHTD_SERVICES_SYNC_BRAVO.Jobs
{
    public class SyncImageJob : IJob
    {
        ILog _logger = LogManager.GetLogger("SecondFileAppender");

        protected readonly ScaleImageRepository _scaleImageRepository;

        private static string strToken;

        public SyncImageJob(
            ScaleImageRepository scaleImageRepository
            )
        {
            _scaleImageRepository = scaleImageRepository;
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
            _logger.Info("===================================------------------===================================");
            _logger.Info("Start process Sync Image job");

            GetToken();

            List<ScaleImageDto> scaleImages = _scaleImageRepository.GetList();

            if (scaleImages == null || scaleImages.Count == 0)
            {
                _logger.Info($"Tất cả ảnh phiếu cân đã được đồng bộ");
                return;
            }

            _logger.Info($"Thực hiện đồng bộ ảnh: {JsonConvert.SerializeObject(scaleImages)}");

            bool isSynced = await SyncScaleImageToDMS(scaleImages);
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
                _logger.Info("getToken error: " + ex.Message);
            }
        }

        public async Task<bool> SyncScaleImageToDMS(List<ScaleImageDto> scaleImages)
        {
            IRestResponse response = HttpRequest.SyncScaleImageToDMS(strToken, scaleImages);

            var content = response.Content;

            var responseData = JsonConvert.DeserializeObject<GetSyncResponse>(content);

            var successList = responseData?.data?.success;

            var failList = responseData?.data?.fails;

            if (successList != null)
            {
                foreach (var itemSuccess in successList)
                {
                    _logger.Info($"Đồng bộ thành công: {itemSuccess.Code}");
                    await this._scaleImageRepository.UpdateSyncSuccess(itemSuccess.Code);
                }
            }

            if (failList != null)
            {
                foreach (var itemFail in failList)
                {
                    _logger.Info($"Đồng bộ thất bại: {itemFail.Code}");
                    await this._scaleImageRepository.UpdateSyncFail(itemFail.Code);
                }
            }

            return true;
        }
    }
}
