using System;
using System.Runtime.Serialization;

namespace DCMS.Core
{
    /// <summary>
    /// 内部异常类
    /// </summary>
    [Serializable]
    public class DCMSException : Exception
    {

        public DCMSException()
        {
        }

 
        public DCMSException(string message)
            : base(message)
        {
        }


        public DCMSException(string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {
        }


        protected DCMSException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public DCMSException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}
