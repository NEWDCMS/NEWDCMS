using System;
using System.Security.Cryptography;

namespace DCMS.Core
{
    /// <summary>
    /// 哈希辅助类
    /// </summary>
    public partial class HashHelper
    {
        /// <summary>
        /// 创建数据哈希
        /// </summary>
        /// <param name="data">用于计算哈希值的数据</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <param name="trimByteCount">将在哈希算法中使用的字节数；保留0以使用所有数组</param>
        /// <returns></returns>
        public static string CreateHash(byte[] data, string hashAlgorithm, int trimByteCount = 0)
        {
            if (string.IsNullOrEmpty(hashAlgorithm))
            {
                throw new ArgumentNullException(nameof(hashAlgorithm));
            }

            var algorithm = (HashAlgorithm)CryptoConfig.CreateFromName(hashAlgorithm);
            if (algorithm == null)
            {
                throw new ArgumentException("无法识别的哈希名称");
            }

            if (trimByteCount > 0 && data.Length > trimByteCount)
            {
                var newData = new byte[trimByteCount];
                Array.Copy(data, newData, trimByteCount);

                return BitConverter.ToString(algorithm.ComputeHash(newData)).Replace("-", string.Empty);
            }

            return BitConverter.ToString(algorithm.ComputeHash(data)).Replace("-", string.Empty);
        }
    }
}
