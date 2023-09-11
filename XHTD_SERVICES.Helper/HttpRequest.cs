﻿using System;
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

            //request.AddHeader("Accept", "application/json");
            //request.AddHeader("Content-Type", "multipart/form-data");
            //request.Parameters.Clear();
            //request.AddParameter("username", requestData.userName);
            //request.AddParameter("password", requestData.password);

            IRestResponse response = client.Execute(request);

            return response;
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

        public static IRestResponse UpdateWeightOutWebSale(string token, string deliveryCode, double weight)
        {
            logger.Info($"UpdateWeightOutWebSale API: deliveryCode={deliveryCode} weight={weight}");

            var apiUrl = ConfigurationManager.GetSection("API_Scale/Url") as NameValueCollection;

            var requestData = new UpdateWeightRequest
            {
                deliveryCode = deliveryCode,
                weight = weight,
            };

            var client = new RestClient(apiUrl["UpdateWeightOut"]);
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

        public static IRestResponse SaleOrderWebSale(string token, string deliveryCode)
        {
            logger.Info($"SaleOrderWebSale API: deliveryCode={deliveryCode}");

            var apiUrl = ConfigurationManager.GetSection("API_Scale/Url") as NameValueCollection;

            var requestData = new UpdateWeightRequest
            {
                deliveryCode = deliveryCode,
            };

            var client = new RestClient($"{apiUrl["SaleOrder"]}/{deliveryCode}");
            var request = new RestRequest();

            request.Method = Method.PUT;
            //request.AddJsonBody(requestData);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;

            IRestResponse response = client.Execute(request);

            return response;
        }

        public static string LoginSMSBrandName()
        {
            var smsBrandNameConfig = ConfigurationManager.GetSection("SMS_BRANDNAME") as NameValueCollection;

            var UserName = smsBrandNameConfig["UserName"];
            var Password = smsBrandNameConfig["Password"];
            var BindMode = smsBrandNameConfig["BindMode"];
            var LoginUrl = smsBrandNameConfig["LoginUrl"];

            string sId = string.Empty;
            var url = LoginUrl.Replace("{UserName}", UserName)
                                      .Replace("{Password}", Password)
                                      .Replace("{BindMode}", BindMode);

            try
            {
                var client = new RestClient(url);
                client.Timeout = 10000;

                var request = new RestRequest(Method.GET);

                var response = client.Execute(request);

                string tmpValueReturn = response.Content;

                if (!string.IsNullOrWhiteSpace(tmpValueReturn))
                {
                    tmpValueReturn = tmpValueReturn.Replace("{", "")
                                                   .Replace("}", "")
                                                   .Replace("\"", "");
                    string[] tmpValueReturnArr = tmpValueReturn.Split(',');
                    if (tmpValueReturnArr.Length > 0)
                    {
                        sId = tmpValueReturnArr[0].Replace("sid:", "").Trim();
                    }
                }

                return sId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while making request to {url}", ex);
            }
        }

        public static bool SendSMSBrandName(string content, string recipient = null)
        {
            var sId = LoginSMSBrandName();

            if (string.IsNullOrEmpty(sId))
            {
                logger.Info($"Login sms brandname khong thanh cong");
                return false;
            }

            var smsBrandNameConfig = ConfigurationManager.GetSection("SMS_BRANDNAME") as NameValueCollection;

            var BrandName = smsBrandNameConfig["BrandName"];
            var SendUrl = smsBrandNameConfig["SendUrl"];
            var Recipient = recipient ?? smsBrandNameConfig["Recipient"];

            var sendUrl = SendUrl.Replace("{Sid}", sId)
                                          .Replace("{BrandName}", BrandName)
                                          .Replace("{Recipient}", Recipient)
                                          .Replace("{Content}", content);

            try
            {
                var client = new RestClient(sendUrl);
                client.Timeout = 10000;

                var request = new RestRequest(Method.GET);

                var response = client.Execute(request);

                string tmpValueReturn = response.Content;

                string status = string.Empty;

                if (!string.IsNullOrWhiteSpace(tmpValueReturn))
                {
                    tmpValueReturn = tmpValueReturn.Replace("{", "")
                                                   .Replace("}", "")
                                                   .Replace("\"", "");
                    string[] tmpValueReturnArr = tmpValueReturn.Split(',');
                    if (tmpValueReturnArr.Length > 0)
                    {
                        status = tmpValueReturnArr[1].Replace("status:", "").Trim();
                    }
                }

                return status == "200";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while making request to {sendUrl}", ex);
            }
        }
    }
}
