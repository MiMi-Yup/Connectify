namespace API.DTOs.Responses
{
    public class APIPagingRes<T> : APIRes<ICollection<T>>
    {
        public int TotalCount { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
