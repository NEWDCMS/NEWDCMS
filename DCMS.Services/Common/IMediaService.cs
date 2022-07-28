namespace DCMS.Services.Common
{
    public interface IMediaService
    {
        string GenerateBarCodeForBase64(string input, int width, int height);
        string GenerateGenerateQRForBase64(string text, int width, int height);
    }
}