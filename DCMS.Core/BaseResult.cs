using Newtonsoft.Json;

namespace DCMS.Core
{
    public class BaseResult
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public int Code { get; set; } = 0;
        public int Return { get; set; }

        public APIResult<object> To(BaseResult baseResult)
        {
            return new APIResult<object>()
            {
                Code = baseResult.Code,
                Message = baseResult.Message,
                Success = baseResult.Success,
                Data = baseResult,
                Return = baseResult.Return
            };
        }
    }


    /// <summary>
    /// 表示API请求返回结果
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class APIResult<TResult> 
    {
        /// <summary>
        /// 状态码
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; } = 0;
        public int Return { get; set; }

        private TResult _data;


        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty("data")]
        public TResult Data
        {
            get => _data ?? (_data = default(TResult));
            set => _data = value;
        }

        [JsonProperty("success")]
        public bool Success { get; set; } = false;


        [JsonProperty("message")]
        public string Message { get; set; }


        [JsonProperty("rows")]
        public int Rows { get; set; }


        [JsonProperty("pages")]
        public int Pages { get; set; }
    }




    /// <summary>
    /// 返回结果信息
    /// </summary>
    public class ResultMessage
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = false;

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Massage { get; set; }

    }

}
