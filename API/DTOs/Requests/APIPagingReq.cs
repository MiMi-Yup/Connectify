using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Requests
{
    public class APIPagingReq<T>
    {
        public T? Data { get; set; }

        [Required]
        [Range(-1, int.MaxValue)]
        public int PageIndex { get; set; } = 0;

        [Required]
        [Range(-1, int.MaxValue)]
        public int PageSize { get; set; } = 24;
    }
}
