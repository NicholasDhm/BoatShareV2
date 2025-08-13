using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boat_share.Models
{
    /// <summary>
    /// Represents a boat reservation in the system
    /// </summary>
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationId { get; set; }

        [Required]
        public required int UserId { get; set; }

        [Required]
        public required int BoatId { get; set; }

        [Required]
        public required DateTime StartTime { get; set; }

        [Required]
        public required DateTime EndTime { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; } = 0;

        [Required]
        [RegularExpression("^(Confirmed|Unconfirmed|Pending|Cancelled)$", 
            ErrorMessage = "Status must be 'Confirmed', 'Unconfirmed', 'Pending', or 'Cancelled'")]
        public required string Status { get; set; }

        [Required]
        [RegularExpression("^(Standard|Substitution|Contingency)$", 
            ErrorMessage = "ReservationType must be 'Standard', 'Substitution', or 'Contingency'")]
        public required string ReservationType { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the date of the reservation for easy querying
        /// </summary>
        [NotMapped]
        public DateTime ReservationDate => StartTime.Date;

        /// <summary>
        /// Gets the duration of the reservation in hours
        /// </summary>
        [NotMapped]
        public double DurationHours => (EndTime - StartTime).TotalHours;

        /// <summary>
        /// Checks if the reservation is confirmed
        /// </summary>
        [NotMapped]
        public bool IsConfirmed => Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if the reservation is pending
        /// </summary>
        [NotMapped]
        public bool IsPending => Status.Equals("Pending", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if the reservation is cancelled
        /// </summary>
        [NotMapped]
        public bool IsCancelled => Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if the reservation is in the past
        /// </summary>
        [NotMapped]
        public bool IsInPast => EndTime < DateTime.UtcNow;

        /// <summary>
        /// Checks if the reservation is currently active
        /// </summary>
        [NotMapped]
        public bool IsActive => DateTime.UtcNow >= StartTime && DateTime.UtcNow <= EndTime && IsConfirmed;

        /// <summary>
        /// Gets the number of days from now until the reservation starts
        /// </summary>
        [NotMapped]
        public int DaysUntilStart => (int)(StartTime.Date - DateTime.UtcNow.Date).TotalDays;

        /// <summary>
        /// Updates the UpdatedAt timestamp
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Calculates the total cost based on duration and hourly rate
        /// </summary>
        public void CalculateCost(decimal hourlyRate)
        {
            TotalCost = (decimal)DurationHours * hourlyRate;
            MarkAsUpdated();
        }

        /// <summary>
        /// Validates if the reservation times are valid
        /// </summary>
        public bool IsValidTimeRange()
        {
            return StartTime < EndTime && StartTime > DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if this reservation conflicts with another reservation
        /// </summary>
        public bool ConflictsWith(Reservation other)
        {
            if (BoatId != other.BoatId) return false;
            if (ReservationId == other.ReservationId) return false;
            
            return StartTime < other.EndTime && EndTime > other.StartTime;
        }

        /// <summary>
        /// Gets a formatted string representation of the reservation time
        /// </summary>
        [NotMapped]
        public string TimeRangeDisplay => $"{StartTime:yyyy-MM-dd HH:mm} - {EndTime:HH:mm}";

        /// <summary>
        /// Gets a summary of the reservation
        /// </summary>
        [NotMapped]
        public string Summary => $"{TimeRangeDisplay} ({DurationHours:F1}h) - {Status}";

        // Navigation properties for EF Core
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("BoatId")]
        public virtual Boat? Boat { get; set; }
    }
}
