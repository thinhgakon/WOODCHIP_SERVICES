using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using XHTD_SERVICES.Helper.Models.Request;
using XHTD_SERVICES.Helper.Models.Response;
using RestSharp;
using Newtonsoft.Json;
using log4net;
using XHTD_SERVICES.Data.Entities;
using XHTD_SERVICES.Data.Models;
using XHTD_SERVICES.Data.Dtos;

namespace XHTD_SERVICES.Helper
{
    public static class HttpRequest
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(HttpRequest));

        public static IRestResponse GetWebsaleToken()
        {
            var apiUrl = ConfigurationManager.GetSection("API_WebSale/Url") as NameValueCollection;
            var account = ConfigurationManager.GetSection("API_WebSale/Account") as NameValueCollection;

            var requestData = new GetTokenRequest
            {
                userName = account["username"].ToString(),
                password = account["password"].ToString(),
            };

            var client = new RestClient(apiUrl["GetToken"]);
            var request = new RestRequest();

            request.Method = Method.POST;

            request.AddJsonBody(requestData);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;

            IRestResponse response = client.Execute(request);

            return response;
        }

        public static IRestResponse SyncScaleBillToDMS(string token, List<ScaleBillDto> scaleBills)
        {
            try {

                var requestObj = scaleBills.Select(x=> new ScaleBillRequestDto(x)).ToList();
                var apiUrl = ConfigurationManager.GetSection("API_WebSale/Url") as NameValueCollection;

                var client = new RestClient(apiUrl["SyncScaleBill"]);
                var request = new RestRequest
                {
                    Method = Method.PUT
                };
                request.AddJsonBody(requestObj);
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.RequestFormat = DataFormat.Json;

                IRestResponse response = client.Execute(request);

                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static IRestResponse SyncScaleImageToDMS(string token, List<ScaleImageDto> scaleImages)
        {
            try
            {
                var requestObj = scaleImages.Select(x => new ScaleImageRequestDto
                {
                    ModuleType = x.Type == "IN" ? "SCALEIN" : "SCALEOUT",
                    SyncCode = x.Id.ToString(),
                    ScaleCode = x.ScaleBillCode,
                    Files = new FileDto
                    {
                        //ByteData = Convert.FromBase64String(FileHelper.ConvertImageToBase64(x.Attachment.Url)),
                        ByteData = FileHelper.ConvertImageToBase64(x.Attachment.Url),
                        Name = x.Attachment.Title,
                        Extension = x.Attachment.Extension,
                    }
                })
                .ToList();

                var apiUrl = ConfigurationManager.GetSection("API_WebSale/Url") as NameValueCollection;

                var client = new RestClient(apiUrl["SyncImageBill"]);
                var request = new RestRequest
                {
                    Method = Method.POST
                };
                request.AddJsonBody(requestObj);
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.RequestFormat = DataFormat.Json;

                IRestResponse response = client.Execute(request);

                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static IRestResponse GetDMSToken()
        {
            var apiUrl = ConfigurationManager.GetSection("API_DMS/Url") as NameValueCollection;
            var account = ConfigurationManager.GetSection("API_DMS/Account") as NameValueCollection;

            var requestData = new GetDMSTokenRequest
            {
                grant_type = account["grant_type"].ToString(),
                username = account["username"].ToString(),
                password = account["password"].ToString(),
            };

            var client = new RestClient(apiUrl["GetToken"]);
            var request = new RestRequest();

            request.Method = Method.POST;

            request.AddJsonBody(requestData);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;

            IRestResponse response = client.Execute(request);

            return response;
        }

        public static IRestResponse SendDMSMsg(string token, SendMsgRequest messenge)
        {
            var apiUrl = ConfigurationManager.GetSection("API_DMS/Url") as NameValueCollection;

            var requestData = new SendMsgRequest
            {
                Type = messenge.Type,
                Source = messenge.Source,
                Status = messenge.Status,
                Direction = messenge.Direction,
                Content = messenge.Content,
                Data = messenge.Data
            };

            var client = new RestClient(apiUrl["SendMsg"]);
            var request = new RestRequest();

            request.Method = Method.POST;
            request.AddJsonBody(requestData);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;

            IRestResponse response = client.Execute(request);

            return response;
        }

        public static IRestResponse SendInforNotification(string receiver, string message)
        {
            var apiUrl = ConfigurationManager.GetSection("API_DMS/Url") as NameValueCollection;

            var requestData = new SendInforNotificationRequest
            {
                UserNameSender = apiUrl["UserNameSender"],
                UserNameReceiver = receiver,
                ContentMessage = message,
            };

            var client = new RestClient(apiUrl["SendInforNotification"]);
            var request = new RestRequest();

            request.Method = Method.POST;
            request.AddJsonBody(requestData);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;

            IRestResponse response = client.Execute(request);

            return response;
        }

        public static IRestResponse UpdateWeightInWebSale(string token, string deliveryCode, double weight)
        {
            logger.Info($"UpdateWeightInWebSale API: deliveryCode={deliveryCode} weight={weight}");

            var apiUrl = ConfigurationManager.GetSection("API_Scale/Url") as NameValueCollection;

            var requestData = new UpdateWeightRequest
            {
                deliveryCode = deliveryCode,
                weight = weight,
            };

            var client = new RestClient(apiUrl["UpdateWeightIn"]);
            var request = new RestRequest();

            request.Method = Method.POST;
            request.AddJsonBody(requestData);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;

            IRestResponse response = client.Execute(request);

            return response;
        }
    }
}
