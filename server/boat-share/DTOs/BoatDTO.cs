using System.ComponentModel.DataAnnotations;

namespace boat_share.DTOs
{
    public class BoatDTO
    {
        public int BoatId { get; set; }
        
        [Required]
        [StringLength(200)]
        public required string Name { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 50)]
        public int Capacity { get; set; }
        
        public decimal HourlyRate { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        public int AssignedUsersCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class BoatCreateDTO
    {
        [Required]
        [StringLength(200)]
        public required string Name { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 50)]
        public int Capacity { get; set; }
        
        public decimal HourlyRate { get; set; } = 0;
    }

    public class BoatUpdateDTO
    {
        [StringLength(200)]
        public string? Name { get; set; }
        
        [StringLength(100)]
        public string? Type { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        [Range(1, 50)]
        public int? Capacity { get; set; }
        
        public decimal? HourlyRate { get; set; }
        
        public bool? IsActive { get; set; }
    }
}
