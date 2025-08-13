using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boat_share.Models
{
    /// <summary>
    /// Represents a user in the boat sharing system
    /// </summary>
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public required string Name { get; set; }

        [Required]
        [RegularExpression("^(Admin|Member)$", ErrorMessage = "Role must be either 'Admin' or 'Member'")]
        public required string Role { get; set; }

        [Range(0, 365, ErrorMessage = "StandardQuota must be between 0 and 365")]
        public int StandardQuota { get; set; }

        [Range(0, 365, ErrorMessage = "SubstitutionQuota must be between 0 and 365")]
        public int SubstitutionQuota { get; set; }

        [Range(0, 365, ErrorMessage = "ContingencyQuota must be between 0 and 365")]
        public int ContingencyQuota { get; set; }

        [Required]
        public required int BoatId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "PasswordHash cannot exceed 500 characters")]
        public required string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets the total number of quotas available to the user
        /// </summary>
        [NotMapped]
        public int TotalQuotas => StandardQuota + SubstitutionQuota + ContingencyQuota;

        /// <summary>
        /// Checks if the user has admin privileges
        /// </summary>
        [NotMapped]
        public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Updates the UpdatedAt timestamp
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates if the user has sufficient quotas for a reservation type
        /// </summary>
        public bool HasSufficientQuota(string reservationType)
        {
            return reservationType switch
            {
                "Standard" => StandardQuota > 0,
                "Substitution" => SubstitutionQuota > 0,
                "Contingency" => ContingencyQuota > 0,
                _ => false
            };
        }

        /// <summary>
        /// Deducts quota for a specific reservation type
        /// </summary>
        public bool DeductQuota(string reservationType)
        {
            if (!HasSufficientQuota(reservationType))
                return false;

            switch (reservationType)
            {
                case "Standard":
                    StandardQuota--;
                    break;
                case "Substitution":
                    SubstitutionQuota--;
                    break;
                case "Contingency":
                    ContingencyQuota--;
                    break;
                default:
                    return false;
            }

            MarkAsUpdated();
            return true;
        }

        /// <summary>
        /// Restores quota for a specific reservation type (e.g., when a reservation is cancelled)
        /// </summary>
        public void RestoreQuota(string reservationType)
        {
            switch (reservationType)
            {
                case "Standard":
                    StandardQuota++;
                    break;
                case "Substitution":
                    SubstitutionQuota++;
                    break;
                case "Contingency":
                    ContingencyQuota++;
                    break;
            }

            MarkAsUpdated();
        }

        /// <summary>
        /// Validates the user data
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Role) &&
                   !string.IsNullOrWhiteSpace(PasswordHash) &&
                   BoatId > 0 &&
                   StandardQuota >= 0 &&
                   SubstitutionQuota >= 0 &&
                   ContingencyQuota >= 0;
        }

        /// <summary>
        /// Resets all quotas to their default values
        /// </summary>
        public void ResetQuotas(int standardQuota = 0, int substitutionQuota = 0, int contingencyQuota = 0)
        {
            StandardQuota = standardQuota;
            SubstitutionQuota = substitutionQuota;
            ContingencyQuota = contingencyQuota;
            MarkAsUpdated();
        }

        /// <summary>
        /// Gets a summary of the user's quota status
        /// </summary>
        [NotMapped]
        public string QuotaSummary => $"Standard: {StandardQuota}, Substitution: {SubstitutionQuota}, Contingency: {ContingencyQuota}, Total: {TotalQuotas}";

        /// <summary>
        /// Checks if the user can make a reservation based on their quotas
        /// </summary>
        public bool CanMakeReservation()
        {
            return TotalQuotas > 0 && IsActive;
        }

        /// <summary>
        /// Gets the display name for the user
        /// </summary>
        [NotMapped]
        public string DisplayName => Name;

        /// <summary>
        /// Gets the user's status as a string
        /// </summary>
        [NotMapped]
        public string Status => IsActive ? "Active" : "Inactive";

        // Navigation property for EF Core
        [ForeignKey("BoatId")]
        public virtual Boat? Boat { get; set; }
    }
}
