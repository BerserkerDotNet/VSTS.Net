using System;

namespace VSTS.Net.Exceptions
{
    [Serializable]
    public class UnknownWorkItemQueryTypeException : Exception
    {
        public UnknownWorkItemQueryTypeException()
        {
        }

        public UnknownWorkItemQueryTypeException(string message)
            : base(message)
        {
        }

        public UnknownWorkItemQueryTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected UnknownWorkItemQueryTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
