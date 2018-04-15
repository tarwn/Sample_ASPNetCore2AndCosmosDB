using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Models.Shared
{
    public class ApiError
    {
        [Obsolete("Serialization only")]
        public ApiError() { }

        public ApiError(int statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetMessageForStatusCoe(statusCode);
        }

        private string GetMessageForStatusCoe(int statusCode)
        {
            switch (statusCode)
            {
                case 400:
                    return "Bad Request";
                case 404:
                    return "Resource Not Found";
                case 500:
                    return "Internal Server Error";
                default:
                    return String.Empty;
            }
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
