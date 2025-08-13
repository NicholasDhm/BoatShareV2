using System.ComponentModel.DataAnnotations;

namespace boat_share.DTOs
{
    public class ReservationDTO
    {
        public int ReservationId { get; set; }

        [Required]
        public required int UserId { get; set; }

        [Required]
        public required int BoatId { get; set; }

        [Required]
        public required DateTime StartTime { get; set; }

        [Required]
        public required DateTime EndTime { get; set; }

        [Required]
        [RegularExpression("^(Standard|Substitution|Contingency)$")]
        public required string ReservationType { get; set; }

        public string Status { get; set; } = "Pending";
        public string Notes { get; set; } = string.Empty;
    }

    public class ReservationResponseDTO
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int BoatId { get; set; }
        public string? BoatName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationHours { get; set; }
        public decimal TotalCost { get; set; }
        public required string Status { get; set; }
        public required string ReservationType { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateReservationDTO
    {
        [Required]
        public required int BoatId { get; set; }

        [Required]
        public required DateTime StartTime { get; set; }

        [Required]
        public required DateTime EndTime { get; set; }

        [Required]
        [RegularExpression("^(Standard|Substitution|Contingency)$")]
        public required string ReservationType { get; set; }

        public string Notes { get; set; } = string.Empty;
    }

    // Legacy DTO for compatibility
    public class ReservationDBO
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public int BoatId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalCost { get; set; }
        public required string Status { get; set; }
        public required string ReservationType { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
