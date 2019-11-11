using System.Collections.Generic;

namespace IdentityServer4.MongoDbAdapter.Demo.ViewModels
{
    public class HttpFailureResponseViewModel
    {
        #region Constructor

        public HttpFailureResponseViewModel(string message, string messageCode = null)
        {
            Message = message;
            MessageCode = messageCode;
        }

        public HttpFailureResponseViewModel(string message, string messageCode, Dictionary<string, object> additionalData)
        {
            Message = message;
            MessageCode = messageCode;
            AdditionalData = additionalData;
        }

        #endregion

        #region Properties

        public string Message { get; }

        public string MessageCode { get; }

        public Dictionary<string, object> AdditionalData { get; set; }

        #endregion
    }
}