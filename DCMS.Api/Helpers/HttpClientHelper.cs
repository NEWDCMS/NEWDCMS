using System;
using System.Net.Http;
using System.Threading.Tasks;


namespace DCMS.Api.Helpers
{
    public class HttpClientHelper
    {
        private static readonly HttpClient _httpClient;
        public const string FileCenterEndpoint = "http://resources.jsdcms.com:9100/";

        static HttpClientHelper()
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri(FileCenterEndpoint) };
            _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            try
            {
                _httpClient.SendAsync(new HttpRequestMessage
                {
                    Method = new HttpMethod("HEAD"),
                    RequestUri = new Uri(FileCenterEndpoint + "/")
                });//.Result.EnsureSuccessStatusCode();
            }
            catch (System.Net.Http.HttpRequestException)
            {
            }
            catch (Exception )
            {
            }
        }

        public async Task<string> PostAsync(string url, MultipartFormDataContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync(url, content);
                return await response.Content.ReadAsStringAsync();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
