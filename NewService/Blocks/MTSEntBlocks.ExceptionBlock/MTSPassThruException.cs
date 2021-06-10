using System;
using System.Runtime.Serialization;

namespace MTSEntBlocks.ExceptionBlock
{
    public class MTSPassThruException : MTSException, ISerializable
    {
        public MTSPassThruException()
          : base()
        {
        }

        public MTSPassThruException(string message)
          : base(message)
        {
        }

        public MTSPassThruException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        protected MTSPassThruException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}