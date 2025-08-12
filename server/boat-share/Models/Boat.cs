using Amazon.DynamoDBv2.DataModel;

namespace boat_share.Models
{
    [DynamoDBTable("Boats")]
    public class Boat
    {
        [DynamoDBHashKey] // Primary Key
        public required string BoatId { get; set; }
        public required string Name { get; set; }
        public int Capacity { get; set; }
		public int AssignedUsersCount { get; set; }
	}
}
