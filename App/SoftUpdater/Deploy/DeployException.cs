//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using System;
using System.Runtime.Serialization;

namespace SoftUpdater.Deploy
{
    [Serializable]
    internal class DeployException : Exception
    {
        public DeployException()
        {
        }

        public DeployException(string message) : base(message)
        {
        }

        public DeployException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeployException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
