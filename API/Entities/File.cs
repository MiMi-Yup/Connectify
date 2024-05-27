using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    public class File
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FilePath { get; set; } = default!;

        [NotMapped]
        public string FileName => string.IsNullOrEmpty(FilePath) ? string.Empty : Path.GetFileName(FilePath);

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public AppUser UploadedBy { get; set; } = default!;
    }
}
