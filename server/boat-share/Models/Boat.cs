using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boat_share.Models
{
    public class Boat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BoatId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public required string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
        public string Type { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Range(1, 50, ErrorMessage = "Capacity must be between 1 and 50")]
        public int Capacity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal HourlyRate { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public int AssignedUsersCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updates the UpdatedAt timestamp
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the boat is available for new assignments
        /// </summary>
        [NotMapped]
        public bool IsAvailableForAssignment => IsActive && AssignedUsersCount < Capacity;

        /// <summary>
        /// Gets the remaining capacity for user assignments
        /// </summary>
        [NotMapped]
        public int RemainingCapacity => Math.Max(0, Capacity - AssignedUsersCount);

        /// <summary>
        /// Gets the boat's status as a string
        /// </summary>
        [NotMapped]
        public string Status => IsActive ? "Active" : "Inactive";

        /// <summary>
        /// Gets a summary of the boat's assignment status
        /// </summary>
        [NotMapped]
        public string AssignmentSummary => $"{AssignedUsersCount}/{Capacity} users assigned";
    }
}
