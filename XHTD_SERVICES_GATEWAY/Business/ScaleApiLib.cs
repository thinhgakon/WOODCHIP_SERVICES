using Newtonsoft.Json;
using XHTD_SERVICES_GATEWAY.Models.Response;
using XHTD_SERVICES.Helper;
using XHTD_SERVICES.Data.Dtos;

namespace XHTD_SERVICES_GATEWAY.Business
{
    public class ScaleApiLib
    {
        public DesicionScaleResponse SaleOrder(GatewayCheckInOutRequestDto gatewayData)
        {
            var strToken = HttpRequest.GetMmesToken();

            var updateResponse = HttpRequest.SyncGatewayDataToDMS(strToken, gatewayData);

            var updateResponseContent = updateResponse.Content;

            var response = JsonConvert.DeserializeObject<DesicionScaleResponse>(updateResponseContent);

            return response;
        }
    }
}
