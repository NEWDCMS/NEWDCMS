using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DCMS.Core
{
    public class ServiceAuthenticationException : Exception
    {
        public string Content { get; }

        public ServiceAuthenticationException()
        {
        }

        public ServiceAuthenticationException(string content)
        {
            Content = content;
        }
    }

    public class PostAPIResult
    {
        public string Guid { get; set; }
        public string Type { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
        public string Data { get; set; }
    }

    public static class POSTAPI
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>() { new StringEnumConverter() }
        };

        public static string RequestHttp(string url)
        {
            string res = "";
            bool isfinish = false;
            WebRequest request = HttpWebRequest.Create(url);
            request.BeginGetResponse((IAsyncResult result) =>
            {
                HttpWebRequest webrequest = (HttpWebRequest)(result.AsyncState);
                WebResponse response = request.EndGetResponse(result) as HttpWebResponse;
                if (response != null)
                {
                    Stream responseStream = response.GetResponseStream();
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        res = streamReader.ReadToEnd();
                    }
                }
                isfinish = true;
            }, request);
            while (isfinish != true) {; }
            return res;
        }

        public static async Task<string> GetResult(string url)
        {
            var task = Task<string>.Factory.StartNew(() => RequestHttp(url));
            await task;
            return task.Result;
        }

        //public static CookieContainer GetCookieContainer(string CacheKey)
        //{
        //    System.Web.Caching.Cache objCache = System.Web.HttpRuntime.Cache;
        //    return objCache[CacheKey] as CookieContainer;
        //}

        //public static void SetCookieContainer(string cacheKey, CookieContainer cookieContainer)
        //{
        //    if (cookieContainer == null)
        //        return;
        //    System.Web.Caching.Cache objCache = System.Web.HttpRuntime.Cache;
        //    objCache.Insert(cacheKey, cookieContainer);
        //}

        public static T GetApiServer<T>(string url)
        {
            var postResult = new PostAPIResult();
            try
            {
                HttpWebRequest request = null;
                string Cookiesstr = string.Empty;
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:19.0) Gecko/20100101 Firefox/19.0";
                request.Timeout = 60000;
                //request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));
                //request.KeepAlive = true;
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    postResult.Success = false;
                    postResult.Code = 401;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    postResult.Success = false;
                    postResult.Code = 400;
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    postResult.Success = false;
                    postResult.Code = 500;
                }
                else if (response.StatusCode == HttpStatusCode.OK)
                {
                    postResult.Success = true;
                    postResult.Code = 200;
                }

                Stream responseStream = response.GetResponseStream();
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    postResult.Message = streamReader.ReadToEnd();
                }
                responseStream.Close();
                if (response != null)
                {
                    response.Close();
                }

                if (request != null)
                {
                    request.Abort();
                }
            }
            catch (WebException ex)
            {
                CommonHelper.WriteLog(ex.Message + ";" + url, "APIException");

                postResult.Success = false;
                postResult.Code = 500;
                if (ex.Response != null)
                {
                    Stream responseStream = ex.Response.GetResponseStream();
                    using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        postResult.Message = ex.Message + ":" + streamReader.ReadToEnd();
                    }
                    responseStream.Close();
                }
                else
                {
                    postResult.Message = ex.Message;
                }
                //Send mail

            }

            if (postResult.Code == 200)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(postResult.Message);
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
            else
            {
                return default(T);
            }
        }

        public static PostAPIResult PostApiServerV1<T>(T data, string url)
        {
            var postResult = new PostAPIResult();
            string postData = null;
            try
            {
                //postData = JsonConvert.SerializeObject(data);
                postData = data.ToString();
                HttpWebRequest request = null;
                //golcookie
                string Cookiesstr = string.Empty;
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:19.0) Gecko/20100101 Firefox/19.0";
                //request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));
                byte[] postdatabyte = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = postdatabyte.Length;
                request.AllowAutoRedirect = false;
                //request.CookieContainer = golcookie;
                //request.KeepAlive = true;
                //Stream stream;
                var stream = request.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length);
                stream.Close();
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    postResult.Success = false;
                    postResult.Code = 401;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    postResult.Success = false;
                    postResult.Code = 400;
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    postResult.Success = false;
                    postResult.Code = 500;
                }
                else if (response.StatusCode == HttpStatusCode.OK)
                {
                    postResult.Success = true;
                    postResult.Code = 200;
                }

                Stream responseStream = response.GetResponseStream();
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    postResult.Message = streamReader.ReadToEnd();
                }
                responseStream.Close();
                if (response != null)
                {
                    response.Close();
                }

                if (request != null)
                {
                    request.Abort();
                }
            }
            catch (WebException ex)
            {
                CommonHelper.WriteLog(ex.Message + ";" + url, "APIException");

                postResult.Success = false;
                postResult.Code = 500;
                if (ex.Response != null)
                {
                    Stream responseStream = ex.Response.GetResponseStream();
                    using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        postResult.Message = ex.Message + ":" + streamReader.ReadToEnd();
                    }
                    responseStream.Close();
                }
                else
                {
                    postResult.Message = ex.Message;
                }
            }

            postResult.Data = postData;
            return postResult;
        }

        public static T PostApiServer<T>(string data, string url)
        {
            var postResult = new PostAPIResult();
            string postData = null;
            try
            {
                postData = data.ToString();
                HttpWebRequest request = null;
                //golcookie
                string Cookiesstr = string.Empty;
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:19.0) Gecko/20100101 Firefox/19.0";
                //request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));
                byte[] postdatabyte = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = postdatabyte.Length;
                request.AllowAutoRedirect = false;
                //request.CookieContainer = golcookie;
                //request.KeepAlive = true;
                //Stream stream;
                var stream = request.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length);
                stream.Close();
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    postResult.Success = false;
                    postResult.Code = 401;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    postResult.Success = false;
                    postResult.Code = 400;
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    postResult.Success = false;
                    postResult.Code = 500;
                }
                else if (response.StatusCode == HttpStatusCode.OK)
                {
                    postResult.Success = true;
                    postResult.Code = 200;
                }

                Stream responseStream = response.GetResponseStream();
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    postResult.Message = streamReader.ReadToEnd();
                }
                responseStream.Close();
                if (response != null)
                {
                    response.Close();
                }

                if (request != null)
                {
                    request.Abort();
                }
            }
            catch (WebException ex)
            {
                CommonHelper.WriteLog(ex.Message + ";" + url, "APIException");

                postResult.Success = false;
                postResult.Code = 500;
                if (ex.Response != null)
                {
                    Stream responseStream = ex.Response.GetResponseStream();
                    using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        postResult.Message = ex.Message + ":" + streamReader.ReadToEnd();
                    }
                    responseStream.Close();
                }
                else
                {
                    postResult.Message = ex.Message;
                }
            }

            postResult.Data = postData;

            try
            {
                return JsonConvert.DeserializeObject<T>(postResult.Message);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static void PostLogin(string url, string requestForm, out string jsonString, ref CookieContainer cookie)
        {
            HttpWebRequest request = null;
            //CookieContainer cc = new CookieContainer();
            string Cookiesstr = string.Empty;
            //string usernamePassword = username + ":" + password;
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:19.0) Gecko/20100101 Firefox/19.0";
            //request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));
            byte[] postdatabyte = Encoding.UTF8.GetBytes(requestForm);
            request.ContentLength = postdatabyte.Length;
            request.AllowAutoRedirect = false;
            request.CookieContainer = new CookieContainer();
            //request.CookieContainer = new CookieContainer();
            //request.KeepAlive = true;
            //Stream stream;
            var stream = request.GetRequestStream();
            stream.Write(postdatabyte, 0, postdatabyte.Length);
            stream.Close();
            var response = (HttpWebResponse)request.GetResponse();
            cookie.PerDomainCapacity = 50;
            cookie.Add(response.Cookies);
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            string json = streamReader.ReadToEnd();
            jsonString = json;
        }

        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = TimeSpan.FromSeconds(60);
            return httpClient;
        }

        private static async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //认证异常
                    throw new ServiceAuthenticationException(content);
                }
                throw new HttpRequestException(content);
            }
        }


        public static async Task<TResult> GetAsync<TResult>(string server, string urlPath, string partner, string partnerKey, NameValueCollection valueCollection)
        {
            using (var _httpClient = CreateHttpClient())
            {
                try
                {
                    var dataCollection = new DCMSValueCollection();
                    dataCollection.Get.Add(valueCollection);
                    // 获取签名
                    string signature = SecuritySignHelper.GetSecuritySign(partner, partnerKey, dataCollection);
                    //请求URL
                    string urlParams = dataCollection.SetUrlParams(partner, signature);
                    var builder = new UriBuilder(server)
                    {
                        Path = urlPath,
                        Query = urlParams
                    };
                    string url = builder.ToString();
                    //string url = server+ urlPath+"?"+ urlParams;
                    using (var response = await _httpClient.GetAsync(url.ToLower()))
                    {
                        await HandleResponse(response);
                        string serialized = await response.Content.ReadAsStringAsync();
                        TResult result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));
                        return result;
                    };
                }
                catch (Exception)
                {
                    return default(TResult);
                }
            }
        }
        public static async Task<string> SCMSGetAsync(string server, string urlPath, string urlParams)
        {
            using (var _httpClient = CreateHttpClient())
            {
                try
                {
                    var builder = new UriBuilder(server)
                    {
                        Path = urlPath,
                        Query = urlParams
                    };
                    string url = builder.ToString();
                    using (var response = await _httpClient.GetAsync(url.ToLower()))
                    {
                        await HandleResponse(response);
                        string serialized = await response.Content.ReadAsStringAsync();
                        //TResult result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));
                        return serialized;
                    };
                }
                catch (Exception)
                {
                    //return default(TResult);
                    return "";
                }
            }
        }

        public static async Task<TResult> PostAsync<T, TResult>(string server, string urlPath, string partner, string partnerKey, NameValueCollection valueCollection, T data)
        {
            using (var _httpClient = CreateHttpClient())
            {
                try
                {
                    var dataCollection = new DCMSValueCollection();
                    dataCollection.Get.Add(valueCollection);
                    dataCollection.Post.Add(SecuritySignHelper.ToNameValueCollection<T>(data));

                    // 获取签名
                    ////var tempGetCollection = new NameValueCollection();
                    //var postCollection = new NameValueCollection();
                    ////var model = data.Values.OfType<BaseModel>().FirstOrDefault();
                    //if (data != null)
                    //{
                    //    postCollection = SecuritySignHelper.ToNameValueCollection(data);
                    //}
                    string signature = SecuritySignHelper.GetSecuritySign(partner, partnerKey, dataCollection);
                    //请求URL
                    string urlParams = dataCollection.SetUrlParams(partner, signature);

                    //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                    var builder = new UriBuilder(server)
                    {
                        Path = urlPath,
                        Query = urlParams
                    };
                    string url = builder.ToString();
                    string serialized = await Task.Run(() => JsonConvert.SerializeObject(data, _serializerSettings));
                    using (var response = await _httpClient.PostAsync(url.ToLower(), new StringContent(serialized, Encoding.UTF8, "application/json")))
                    {
                        await HandleResponse(response);
                        string responseData = await response.Content.ReadAsStringAsync();
                        return await Task.Run(() => JsonConvert.DeserializeObject<TResult>(responseData));

                    };
                }
                catch (Exception)
                {
                    return default(TResult);
                }
            }
        }

    }
}
