namespace XinGePushSDK.NET.Res
{
    public class Ret
    {
        public int ret_code { get; set; } //返回码
        public string err_msg { get; set; }	//	请求出错时的错误信息
        public dynamic result { get; set; }  //请求正确时，若有额外数据要返回，则结果封装在该字段的json中。若无额外数据，则可能无此字段

    }
}
