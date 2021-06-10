using System.Runtime.Serialization;

namespace MTSEntBlocks.ExceptionBlock.Handlers
{
    public class MTSUIException : MTSException, ISerializable
    {
        public MTSUIException()
            : base()
        {
            
        }

        public MTSUIException(string message)
            : base(message)
        {
            
        }

        public MTSUIException(string message, System.Exception inner)
            : base(message, inner)
        {
            
        }

        protected MTSUIException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }
    }
}
