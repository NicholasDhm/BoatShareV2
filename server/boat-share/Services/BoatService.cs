using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using boat_share.Abstract;
using boat_share.Models;

namespace boat_share.Services
{
    public class BoatService : IBoatService
	{
        private readonly DynamoDBContext _context;

        public BoatService(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        // Get all boats
        public async Task<List<Boat>> GetBoatsAsync()
        {
            var boats = await _context.ScanAsync<Boat>(new List<ScanCondition>()).GetRemainingAsync();
            return boats;
        }

        // Get boat by ID
        public async Task<Boat> GetBoatByIdAsync(string boatId)
        {
            return await _context.LoadAsync<Boat>(boatId);
        }

        // Add new boat
        public async Task AddBoatAsync(Boat boat)
        {
            await _context.SaveAsync(boat);
        }

        // Delete boat by ID
        public async Task DeleteBoatAsync(string boatId)
        {
            await _context.DeleteAsync<Boat>(boatId);
        }

        // Update boat (if needed)
        public async Task UpdateBoatAsync(Boat boat)
        {
            await _context.SaveAsync(boat);
		}

		public async Task AssignUserToBoatAsync(string boatId, string userId)
		{
			var boat = await GetBoatByIdAsync(boatId);
			if (boat == null)
			{
				throw new Exception($"Boat with ID {boatId} not found.");
			}

			// Add the user to the boat
			boat.AssignedUsersCount += 1;
			await UpdateBoatAsync(boat);
		}

	}
}
