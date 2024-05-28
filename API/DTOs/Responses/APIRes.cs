using System.Net;

namespace API.DTOs.Responses
{
    public class APIRes<T>
    {
        public T? Data { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string? ErrorMessage { get; set; }
    }
}
