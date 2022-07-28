using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace DCMS.Core
{
    public class HttpHelper
    {
        public static string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        public static string WebApiPost(string url, object param)
        {
            try
            {
                string requestJson = JsonConvert.SerializeObject(param);
                JObject requestData = (JObject)JsonConvert.DeserializeObject(requestJson);
                string body = JObjectToPostData(requestData);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] buffer = Encoding.UTF8.GetBytes(body);
                request.ContentLength = buffer.Length;

                using (Stream reqStream = request.GetRequestStream()) //获取
                {
                    reqStream.Write(buffer, 0, buffer.Length);//向当前流中写入字节
                    reqStream.Close(); //关闭当前流
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string responseStr = string.Empty;
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                }
                return responseStr;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static String JObjectToPostData(JObject jObj)
        {
            StringBuilder sb = new StringBuilder();

            IDictionary<string, JToken> source = jObj;
            foreach (string keyStr in source.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.Append(keyStr + "=" + source[keyStr].ToString());
            }
            return sb.ToString();
        }
    }
}
