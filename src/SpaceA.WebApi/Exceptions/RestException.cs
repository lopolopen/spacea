using System;
using System.Collections.Generic;
using System.Net;

namespace SpaceA.WebApi.Exceptions
{
    public class RestException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; private set; }

        public RestException(HttpStatusCode code, string message = null) : base(message)
        {
            this.HttpStatusCode = code;
        }
    }
}