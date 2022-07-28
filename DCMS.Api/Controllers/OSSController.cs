using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Ocr20191230.Models;
using System.Drawing;
using Aliyun.OSS;
using System.Collections.Generic;
using System.Linq;
using DCMS.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IO;
using System.Threading.Tasks;
using Client = AlibabaCloud.SDK.Ocr20191230.Client;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于对象存储服务
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/oss")]
    public class OSSController : BaseAPIController
    {
        private static readonly string AccessKeyId = "LTAI5t7JkPKgoHSCcQGtP6SH";
        private static readonly string AccessKeySecret = "FE34yuXDrA3EBORf3qosqt8LkHiLgJ";
        private static readonly string OCR_Endpoint = "ocr.cn-shanghai.aliyuncs.com";
        private static readonly string OSS_Endpoint = "oss-cn-shanghai.aliyuncs.com";

        public OSSController(ILogger<BaseAPIController> logger) : base(logger)
        { }


        [HttpPost("upload/{storeId}/{userId}")]
        [SwaggerOperation("upload")]
        public async Task<APIResult<List<string>>> Upload(int? storeId, int? userId, IFormFile file)
        {
            if (!storeId.HasValue || storeId.Value == 0)
                return this.Error3<List<string>>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (file == null || file.Length <= 0)
                    {
                        return this.Error3<List<string>>("请上传图片");
                    }

                    long fileSize = 0;
                    fileSize = file.Length;
                    if (fileSize > 307200000)
                    {
                        return this.Error3<List<string>>("图片大小不能超过3MB");
                    }

                    //Bucket名称。
                    var bucketName = "dcmsoss";

                    var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    var pix = Convert.ToInt64(ts.TotalMilliseconds).ToString();

                    //Object完整路径。Object完整路径中不能包含Bucket名称。
                    var objectName = pix + "_" + file.FileName;

                    // 创建OssClient实例。
                    var client = new OssClient(OSS_Endpoint, AccessKeyId, AccessKeySecret);

                    //file.ContentType Content-Type: multipart/form-data

                    //上传文件
                    using (var stream = file.OpenReadStream())
                    {
                        if (stream != null)
                        {
                            //try
                            //{
                            //    using var possibleImage = Image.FromStream(stream);
                            //}
                            //catch
                            //{
                            //    return this.Error3<RecognizePoiNameResponseBody>("文件类型必须是图片");
                            //}
                            //byte[] binaryData = Encoding.ASCII.GetBytes(objectContent);

                            var bytes = StreamToBytes(stream);
                            var requestContent = new MemoryStream(bytes);
                            var result = client.PutObject(bucketName, objectName, requestContent);
                            if (result != null)
                            {
                                var url = "https://dcmsoss.oss-cn-shanghai.aliyuncs.com/{0}?versionId={1}";
                                if (requestContent != null)
                                    requestContent.Dispose();

                                var keys = new List<string>();
                                var ocr = Recognize(string.Format(url, objectName, result.VersionId));
                                var data = ocr?.Body?.Data ?? null;
                                if (data != null)
                                {
                                    var brand = data.Summary.Brand;
                                    keys.Add(brand);
                                    if (data.Signboards.Count > 0)
                                    {
                                        var labels = data
                                        .Signboards[0]
                                        .Texts
                                        .Where(s => s.Tag == "other")
                                        .Select(s => s.Label)
                                        .ToList();

                                        keys.AddRange(labels);
                                    }
                                }

                                return this.Successful3("识别成功", keys);
                            }
                            else
                            {
                                return this.Error3<List<string>>("识别失败");
                            }
                        }
                        else
                        {
                            return this.Error3<List<string>>("识别失败");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return this.Error3<List<string>>(ex.Message);
                }
            });
        }

        private byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        private RecognizePoiNameResponse Recognize(string url)
        {
            var config = new Config
            {
                //访问的域名
                Endpoint = OCR_Endpoint,
                // 您的AccessKey ID
                AccessKeyId = AccessKeyId,
                // 您的AccessKey Secret
                AccessKeySecret = AccessKeySecret
            };

            var client = new Client(config);
            var recognizePoiNameRequest = new RecognizePoiNameRequest
            {
                ImageURL = url
            };

            // 复制代码运行请自行打印 API 的返回值
            return client.RecognizePoiName(recognizePoiNameRequest);
        }
    }
}
