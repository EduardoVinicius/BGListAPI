using System.ComponentModel.DataAnnotations;

namespace BGList.DTO
{
    public class DomainDTO
    {
        [Required]
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}
