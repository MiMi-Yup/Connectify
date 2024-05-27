using System.Net;

namespace API.DTOs.Responses
{
    public class APIReponse<T>
    {
        public T? Data { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string? ErrorMessage { get; set; }
    }
}
