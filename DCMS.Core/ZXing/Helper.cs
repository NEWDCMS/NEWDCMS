using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;



namespace DCMS.Core.ZXing
{
    /// <summary>
    /// 条形码和二维码
    /// </summary>
    public class BarcodeHelper
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        ///// <returns></returns>
        public static string GenerateQR(string text, int width, int height)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            string base64String = "";
            var qrCodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    DisableECI = true,//设置内容编码
                    CharacterSet = "UTF-8", //设置二维码的宽度和高度
                    Width = width,
                    Height = height,
                    Margin = 1//设置二维码的边距,单位不是固定像素
                }
            };
            var pixelData = qrCodeWriter.Write(text);

            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                // save to stream as PNG   
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                base64String = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(ms.ToArray()));
            }
            return base64String;
        }

        public static byte[] BitmapToArray(Bitmap bmp)
        {
            byte[] byteArray = null;

            using (MemoryStream stream = new MemoryStream())
            {

                bmp.Save(stream, ImageFormat.Png);
                byteArray = stream.GetBuffer();
            }

            return byteArray;
        }


        /// <summary>
        /// 生成一维条形码
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public static string GenerateBarCode(string text, int width, int height)
        {
            string base64String = "";
            var qrCodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_39,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = 2
                }
            };
            var pixelData = qrCodeWriter.Write(text);

            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                // save to stream as PNG   
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                base64String = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(ms.ToArray()));
            }
            return base64String;
        }

        /// <summary>
        /// 读取二维码或者条形码从图片
        /// </summary>
        /// <param name="imgFile"></param>
        /// <returns></returns>
        public static string ReadFromImage(string imgFile)
        {

            //if (string.IsNullOrWhiteSpace(imgFile))
            //{
            //    return "";
            //}
            //Image img = Image.FromFile(imgFile);
            //Bitmap b = new Bitmap(img);

            ////该类名称为BarcodeReader,可以读二维码和条形码
            //var zzb = new BarcodeReader
            //{
            //    Options = new DecodingOptions
            //    {
            //        CharacterSet = "UTF-8"
            //    }
            //};
            //Result r = zzb.Decode(BitmapToArray(b));
            //string resultText = r.Text;
            //b.Dispose();
            //img.Dispose();

            return "";

        }

        /// <summary>
        /// 生成带Logo的二维码
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public static Bitmap GenerateImageQR(string text, int width, int height)
        {
            //Logo 图片
            string logoPath = System.AppDomain.CurrentDomain.BaseDirectory + @"\img\logo.png";
            Bitmap logo = new Bitmap(logoPath);
            //构造二维码写码器
            MultiFormatWriter writer = new MultiFormatWriter();
            Dictionary<EncodeHintType, object> hint = new Dictionary<EncodeHintType, object>
            {
                { EncodeHintType.CHARACTER_SET, "UTF-8" },
                { EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H }
            };
            //hint.Add(EncodeHintType.MARGIN, 2);//旧版本不起作用，需要手动去除白边

            //生成二维码 
            BitMatrix bm = writer.encode(text, BarcodeFormat.QR_CODE, width + 30, height + 30, hint);
            bm = DeleteWhite(bm);
            var barcodeWriter = new BarcodeWriter<Bitmap>();
            Bitmap map = barcodeWriter.Write(bm);

            //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
            int[] rectangle = bm.getEnclosingRectangle();

            //计算插入图片的大小和位置
            int middleW = Math.Min(rectangle[2] / 3, logo.Width);
            int middleH = Math.Min(rectangle[3] / 3, logo.Height);
            int middleL = (map.Width - middleW) / 2;
            int middleT = (map.Height - middleH) / 2;

            Bitmap bmpimg = new Bitmap(map.Width, map.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmpimg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.DrawImage(map, 0, 0, width, height);
                //白底将二维码插入图片
                g.FillRectangle(Brushes.White, middleL, middleT, middleW, middleH);
                g.DrawImage(logo, middleL, middleT, middleW, middleH);
            }
            return bmpimg;
        }

        /// <summary>
        /// 删除默认对应的空白
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        private static BitMatrix DeleteWhite(BitMatrix matrix)
        {
            int[] rec = matrix.getEnclosingRectangle();
            int resWidth = rec[2] + 1;
            int resHeight = rec[3] + 1;

            BitMatrix resMatrix = new BitMatrix(resWidth, resHeight);
            resMatrix.clear();
            for (int i = 0; i < resWidth; i++)
            {
                for (int j = 0; j < resHeight; j++)
                {
                    if (matrix[i + rec[0], j + rec[1]])
                    {
                        resMatrix[i, j] = true;
                    }
                }
            }
            return resMatrix;
        }

        /// <summary>
        /// 图片转为base64编码的字符串
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static string ImgToBase64String(Bitmap bmp)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// base64编码的字符串转为图片
        /// </summary>
        /// <param name="strbase64"></param>
        /// <returns></returns>
        public static Bitmap Base64StringToImage(string strbase64)
        {
            try
            {
                byte[] arr = Convert.FromBase64String(strbase64);
                MemoryStream ms = new MemoryStream(arr);
                Bitmap bmp = new Bitmap(ms);
                ms.Close();
                return bmp;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }


}
