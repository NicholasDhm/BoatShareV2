using System.ComponentModel.DataAnnotations.Schema;
using Amazon.DynamoDBv2.DataModel;

namespace boat_share.Models
{
    [DynamoDBTable("Users")]
    public class User
    {
        [DynamoDBHashKey]
        public required string UserId { get; set; }

        [DynamoDBProperty]
        public required string Email { get; set; }

        [DynamoDBProperty]
        public required string Name { get; set; }

        [DynamoDBProperty]
        public required string Role { get; set; }

		[DynamoDBProperty]
		public int StandardQuota { get; set; }

		[DynamoDBProperty]
		public int SubstitutionQuota { get; set; }

		[DynamoDBProperty]
		public int ContingencyQuota { get; set; }

		[DynamoDBProperty]
        public required string BoatId { get; set; }

        [DynamoDBProperty]
        public required string PasswordHash { get; set; }
    }

    public class UserDTO
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string BoatId { get; set; }
        public required string Password{ get; set; }
    }

}
