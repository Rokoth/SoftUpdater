using System;
using System.Runtime.Serialization;

namespace SoftUpdaterClient.Service
{
    [Serializable]
    internal class ConditionExpressionException : Exception
    {
        public ConditionExpressionException()
        {
        }

        public ConditionExpressionException(string message) : base(message)
        {
        }

        public ConditionExpressionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConditionExpressionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}