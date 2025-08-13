using System.ComponentModel.DataAnnotations;

namespace boat_share.DTOs
{
    public class UserDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }

        [Required]
        public required int BoatId { get; set; }

        public string Role { get; set; } = "Member";
    }

    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    public class AuthResponseDTO
    {
        public required string Token { get; set; }
        public required UserInfoDTO User { get; set; }
    }

    public class UserInfoDTO
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Role { get; set; }
        public int BoatId { get; set; }
        public string? BoatName { get; set; }
        public int StandardQuota { get; set; }
        public int SubstitutionQuota { get; set; }
        public int ContingencyQuota { get; set; }
        public int TotalQuotas { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserCreateDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }

        [Required]
        public required int BoatId { get; set; }

        public string Role { get; set; } = "Member";
    }

    public class UserUpdateDTO
    {
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        public int? BoatId { get; set; }

        public string? Role { get; set; }

        public int? StandardQuota { get; set; }

        public int? SubstitutionQuota { get; set; }

        public int? ContingencyQuota { get; set; }

        public bool? IsActive { get; set; }
    }

    public class UserListDTO
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Role { get; set; }
        public int BoatId { get; set; }
        public string? BoatName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
