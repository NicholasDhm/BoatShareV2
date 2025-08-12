using Amazon.DynamoDBv2.DataModel;

namespace boat_share.Models
{
    [DynamoDBTable("Reservations")]
    public class Reservation
    {
        [DynamoDBHashKey] // Partition Key
        public required string ReservationId { get; set; }

        [DynamoDBProperty]
        public required string UserId { get; set; }

        [DynamoDBProperty]
        public required string BoatId { get; set; }

        [DynamoDBProperty]
        public required int Year { get; set; }

        [DynamoDBProperty]
        public required int Month { get; set; }

        [DynamoDBProperty]
        public required int Day { get; set; }

        [DynamoDBProperty]
        public required string Status { get; set; } // Status: Confirmed, Unconfirmed, Pending

		[DynamoDBProperty]
		public required string Type { get; set; } // Type: Standard, Substitution, Contingency

		[DynamoDBProperty]
		public required string CreatedAtIsoDate { get; set; }
	}

    public class ReservationDBO
    {

        public required string UserId { get; set; }
        public required string BoatId { get; set; }
        public required int Year { get; set; }
        public required int Month { get; set; }
        public required int Day { get; set; }
        public required string Type { get; set; } // Type: Standard, Substitution, Contingency

    }
}
