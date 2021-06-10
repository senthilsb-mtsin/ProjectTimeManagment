using System;
using System.Runtime.Serialization;

namespace MTSEntBlocks.ExceptionBlock
{
    public class MTSException : Exception, ISerializable
    {
        public MTSException()
          : base()
        {
        }

        public MTSException(string message)
          : base(message)
        {
        }

        public MTSException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        protected MTSException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}