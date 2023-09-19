using System.ComponentModel.DataAnnotations;

namespace BGList.DTO
{
    public class MechanicDTO
    {
        [Required]
        public int Id { get; set; }
        
        public string? Name { get; set; }
    }
}
