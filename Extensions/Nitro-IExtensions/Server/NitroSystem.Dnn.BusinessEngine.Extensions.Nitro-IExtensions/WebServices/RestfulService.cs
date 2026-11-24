using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.WebServices;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.NitroIII.Services.WebServices
{
    public class RestfulService
    {
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExecuteSqlCommand _sqlCommand;
        private readonly IAppModelService _appModelService;
        private readonly ITypeLoaderFactory _typeLoaderFactory;

        public RestfulService(ICacheService cacheService, IRepositoryBase repository, IExecuteSqlCommand sqlCommand, IUnitOfWork unitOfWork, IAppModelService appModelService, ITypeLoaderFactory typeLoaderFactory)
        {
            _cacheService = cacheService;
            _repository = repository;
            _sqlCommand = sqlCommand;
            _unitOfWork = unitOfWork;
            _appModelService = appModelService;
            _typeLoaderFactory = typeLoaderFactory;
        }

        public async Task<object> CallWebService(ActionDto action, string basePath)
        {
            var objWebServiceRestfulInfo = await _repository.GetByColumnAsync<WebServiceRestfulInfo>("ServiceId", action.ServiceId.Value);
            var restfulService = HybridMapper.Map<WebServiceRestfulInfo, WebServiceRestfulViewModel>(objWebServiceRestfulInfo);

            if (!string.IsNullOrEmpty(objWebServiceRestfulInfo.Params)) restfulService.Params = JsonConvert.DeserializeObject<IEnumerable<ParamInfo>>(objWebServiceRestfulInfo.Params);
            if (!string.IsNullOrEmpty(objWebServiceRestfulInfo.Headers)) restfulService.Headers = JsonConvert.DeserializeObject<IEnumerable<ParamInfo>>(objWebServiceRestfulInfo.Headers);
            if (!string.IsNullOrEmpty(objWebServiceRestfulInfo.BodyFormData)) restfulService.BodyFormData = JsonConvert.DeserializeObject<IEnumerable<ParamInfo>>(objWebServiceRestfulInfo.BodyFormData);
            if (!string.IsNullOrEmpty(objWebServiceRestfulInfo.Authorization)) restfulService.Authorization = JsonConvert.DeserializeObject<AuthorizationInfo>(objWebServiceRestfulInfo.Authorization);

            var headerModel = restfulService.HeaderAppModelId.HasValue
                ? await _appModelService.GetAppModelAsync(restfulService.HeaderAppModelId.Value)
                : null;

            var bodyModel = restfulService.BodyAppModelId.HasValue
                ? await _appModelService.GetAppModelAsync(restfulService.BodyAppModelId.Value)
                : null;

            var webServiceResult = new WebServiceResult();

            if (restfulService.Params != null && restfulService.Params.Any())
            {
                List<string> queryParams = new List<string>();
                foreach (var item in restfulService.Params)
                {
                    queryParams.Add(item.ParamName + "=" + ParamHelper.ParseParams(item.ParamValue, action.Params));
                }

                if (queryParams.Count > 0) restfulService.Url += "?" + string.Join("&", queryParams);
            }

            var request = (HttpWebRequest)WebRequest.Create(restfulService.Url);

            request.Method = restfulService.Method;

            if (restfulService.Headers != null)
            {
                foreach (var item in restfulService.Headers.Where(h => !h.IsSystem || h.IsSelected))
                {
                    switch (item.ParamName)
                    {
                        case "Accept":
                            request.Accept = ParamHelper.ParseParams(item.ParamValue, action.Params);
                            break;
                        case "Connection":
                            request.Connection = ParamHelper.ParseParams(item.ParamValue, action.Params);
                            break;
                        case "Content-Length":
                            request.ContentLength = long.Parse(ParamHelper.ParseParams(item.ParamValue, action.Params));
                            break;
                        case "Content-Type":
                            request.ContentType = ParamHelper.ParseParams(item.ParamValue, action.Params);
                            break;
                        case "Date":
                            request.Date = DateTime.Parse(ParamHelper.ParseParams(item.ParamValue, action.Params));
                            break;
                        case "Expect":
                            request.Expect = ParamHelper.ParseParams(item.ParamValue, action.Params);
                            break;
                        case "Host":
                            request.Host = ParamHelper.ParseParams(item.ParamValue, action.Params);
                            break;
                        case "If-Modified-Since":
                            request.IfModifiedSince = DateTime.Parse(ParamHelper.ParseParams(item.ParamValue, action.Params));
                            break;
                        case "Range":
                            request.AddRange(int.Parse(ParamHelper.ParseParams(item.ParamValue, action.Params)));
                            break;
                        case "Referer":
                            request.Referer = ParamHelper.ParseParams(item.ParamValue, action.Params);
                            break;
                        case "Transfer-Encoding":
                            request.TransferEncoding = ParamHelper.ParseParams(item.ParamValue, action.Params);
                            break;
                        case "User-Agent":
                            request.UserAgent = ParamHelper.ParseParams(item.ParamValue, action.Params);
                            break;
                        case "Proxy-Connection":
                            //request.Proxy = ParamHelper.ParseParams(item.ParamValue);;
                            break;
                        default:
                            request.Headers.Add(item.ParamName, ParamHelper.ParseParams(item.ParamValue, action.Params));
                            break;
                    }
                }
            }

            if (restfulService.Authorization != null && !string.IsNullOrEmpty(restfulService.Authorization.Type))
            {
                if (restfulService.Authorization.Type == "Bearer")
                {
                    string bearer = ParamHelper.ParseParams(restfulService.Authorization.Bearer, action.Params);

                    request.Headers.Add("Authorization",  bearer);
                }
                //else if (restfulService.Authorization.Type == "Basic")
                //{
                //    string username = restfulService.Authorization.BasicAuth.Username;
                //    string password = restfulService.Authorization.BasicAuth.Password;

                //    String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                //    request.Headers.Add("Authorization", "Basic " + encoded);
                //}
            }

            if (!string.IsNullOrEmpty(restfulService.BodyRaw))
            {
                var bodyRaw = ParamHelper.ParseParams(restfulService.BodyRaw, action.Params);

                byte[] dataStream = Encoding.UTF8.GetBytes(bodyRaw);

                request.ContentLength = dataStream.Length;
                Stream newStream = request.GetRequestStream();

                newStream.Write(dataStream, 0, dataStream.Length);
                newStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    webServiceResult.StatusCode = response.StatusCode;
                    webServiceResult.StatusDescription = response.StatusDescription;

                    var headerDict = response.Headers.AllKeys
                        .ToDictionary(key => key, key => response.Headers[key]);

                    var headerJson = JsonConvert.SerializeObject(headerDict);

                    if (headerModel != null)
                    {
                        var type = _typeLoaderFactory.GetTypeFromAssembly(headerModel.TypeRelativePath, headerModel.TypeFullName, "", basePath);
                        webServiceResult.Headers = JsonConvert.DeserializeObject(headerJson, type);
                    }

                    string text;
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        text = sr.ReadToEnd();
                    }

                    webServiceResult.BodyJson = text;

                    if (bodyModel != null)
                    {
                        var type = _typeLoaderFactory.GetTypeFromAssembly(bodyModel.TypeRelativePath, bodyModel.TypeFullName, "", basePath);
                        var token = JToken.Parse(text);

                        if (token is JArray)
                        {
                            var listType = typeof(List<>).MakeGenericType(type);
                            webServiceResult.Body = JsonConvert.DeserializeObject(text, listType);
                        }
                        else if (token is JObject)
                            webServiceResult.Body = JsonConvert.DeserializeObject(text, type);
                    }
                }
            }
            catch (Exception ex2)
            {
                //await _sqlCommand.ExecuteSqlCommandTextAsync(_unitOfWork, $"INSERT INTO dbo.Temp VALUES('{ex2.Message}')");

                throw new Exception(ex2.Message);
            }

            //var json = JsonConvert.SerializeObject(webServiceResult);
            //await _sqlCommand.ExecuteSqlCommandTextAsync(_unitOfWork, $"INSERT INTO dbo.Temp VALUES('{json}')");

            return webServiceResult;
        }
    }
}
