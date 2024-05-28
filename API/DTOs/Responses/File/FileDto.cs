namespace API.DTOs.Responses.File
{
    public class FileDto
    {
        public Guid Id { get; set; }

        public string FilePath { get; set; } = default!;

        public string FileName => string.IsNullOrEmpty(FilePath) ? string.Empty : Path.GetFileName(FilePath);
    }
}
