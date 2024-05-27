using System.Net;
using System.Text.Json;

namespace API.Errors
{
    public class APIException : Exception
    {
        private string _message;
        public string[] Errors { get; set; }
        public new string Message
        {
            get => _message;
            set
            {
                _message = value;
            }
        }
        public HttpStatusCode StatusCode { get; set; }

        public APIException(string message = "Server Error", string[]? errors = null) : base(message)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            _message = message;
            Errors = errors != null ? errors : new string[] { message };
        }

        public APIException(string message = "Server Error", HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string[]? errors = null) : base(message)
        {
            StatusCode = statusCode;
            _message = message;
            Errors = errors != null ? errors : new string[] { message };
        }

        public override string ToString() => JsonSerializer.Serialize(new
        {
            message = Message,
            statusCode = StatusCode,
            errors = Errors
        });
    }
}
