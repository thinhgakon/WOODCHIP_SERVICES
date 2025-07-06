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
    public class SyncOrderJob : IJob
    {
        protected readonly ScaleBillRepository _scaleBillRepository;
        protected readonly PartnerRepository _partnerRepository;
        protected readonly SyncOrderLogger _syncOrderLogger;

        private static string strToken;

        public SyncOrderJob(
            ScaleBillRepository scaleBillRepository,
            SyncOrderLogger syncOrderLogger,
            PartnerRepository partnerRepository
            )
        {
            _scaleBillRepository = scaleBillRepository;
            _syncOrderLogger = syncOrderLogger;
            _partnerRepository = partnerRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await Task.Run(async () =>
            {
                await SyncOrderProcess();
            });
        }

        public async Task SyncOrderProcess()
        {
            _syncOrderLogger.LogInfo("===================================- Start process Sync Order job -===================================");

            GetToken();

            List<ScaleBillRequestDto> scaleBills = _scaleBillRepository.GetList();

            if (scaleBills == null || scaleBills.Count == 0)
            {
                _syncOrderLogger.LogInfo($"Tất cả phiếu cân đã được đồng bộ");
                return;
            }

            foreach (var item in scaleBills)
            {
                bool isSynced = await SyncScaleBillToDMS(item);
            }
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

        public async Task<bool> SyncScaleBillToDMS(ScaleBillRequestDto scaleBills)
        {
            _syncOrderLogger.LogInfo($"Thực hiện đồng bộ phiếu: {JsonConvert.SerializeObject(scaleBills)}");

            IRestResponse partnerResponse = HttpRequest.GetPartner(strToken, scaleBills);
            var partnerResponseData = JsonConvert.DeserializeObject<GetPartnerResponse>(partnerResponse.Content);

            if(partnerResponseData?.data == null)
            {
                var partner = await _partnerRepository.GetPartner(scaleBills?.PartnerCode);

                IRestResponse addPartnerResponse = HttpRequest.AddPartner(strToken, partner);
                var addResult = JsonConvert.DeserializeObject<GetPartnerResponse>(addPartnerResponse.Content);
                if (addResult?.data == null) return false;
            }

            IRestResponse response = HttpRequest.SyncScaleBillToDMS(strToken, scaleBills);

            var content = response.Content;

            var responseData = JsonConvert.DeserializeObject<GetSyncResponse>(content);

            var successList = responseData?.data?.success;

            var failList = responseData?.data?.fails;

            if (successList != null)
            {
                foreach (var itemSuccess in successList)
                {
                    _syncOrderLogger.LogInfo($"Đồng bộ thành công: {itemSuccess.Code}");
                    await this._scaleBillRepository.UpdateSyncSuccess(itemSuccess.Code);
                }
            }

            if (failList != null)
            {
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
