using System;
using System.Collections.Generic;
using System.Net;

namespace IdentityServer4.MongoDbAdapter.Demo.Models.Exceptions
{
    public class HttpResponseException : Exception
    {
        #region Constructor

        public HttpResponseException(HttpStatusCode statusCode, string messageCode, string message = null) : base(message)
        {
            StatusCode = statusCode;
            MessageCode = messageCode;
        }

        public HttpResponseException(HttpStatusCode statusCode, string messageCode, string message, Dictionary<string, object> additionalData) : base(message)
        {
            StatusCode = statusCode;
            MessageCode = messageCode;
            AdditionalData = additionalData;
        }

        #endregion

        #region Properties

        public string MessageCode { get; }

        public HttpStatusCode StatusCode { get; }

        public Dictionary<string, object> AdditionalData { get; }

        #endregion
    }
}