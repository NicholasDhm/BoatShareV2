using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using boat_share.Models;

namespace boat_share.Services
{
    public class UserService
    {
        private readonly DynamoDBContext _context;

        public UserService(IAmazonDynamoDB dynamoDBClient)
        {
            _context = new DynamoDBContext(dynamoDBClient);
        }

        // Update user
        public async Task UpdateUser(User user)
        {
            await _context.SaveAsync(user);
        }

        // Get all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            var conditions = new List<ScanCondition>(); // No conditions means scan everything
            return await _context.ScanAsync<User>(conditions).GetRemainingAsync();
        }

        // Get a user by ID
        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _context.LoadAsync<User>(userId);
        }

        // Add quotas from past reservations
        public async Task AddQuotasBack(User user, Reservation reservation)
        {
            if (reservation.Type == "Contingency")
            {
                user.ContingencyQuota += 1;
            } else if (reservation.Type == "Substitution")
            {
                user.SubstitutionQuota += 1;
            } else if (reservation.Type == "Standard")
            {
                user.StandardQuota += 1;
            } else
            {
                throw new Exception("Reservation without a correct type: " + reservation.Type);
            }
			await UpdateUser(user);
		}

		// Get a certain user by their email
		public async Task<User> GetUserByEmailAsync(string email)
        {
            var searchConditions = new List<ScanCondition>
            {
                new ScanCondition(nameof(User.Email), ScanOperator.Equal, email)
            };

            var searchResults = await _context.ScanAsync<User>(searchConditions).GetRemainingAsync();

            return searchResults.FirstOrDefault();
        }

        // Get users by name
        public async Task<List<User>> GetUsersByPartialNameAsync(string partialName)
        {
            var searchConditions = new List<ScanCondition>
            {
                new ScanCondition(nameof(User.Name), ScanOperator.Contains, partialName)
            };

            return await _context.ScanAsync<User>(searchConditions).GetRemainingAsync();
        }


        // Delete a user by ID
        public async Task DeleteUserAsync(string userId)
        {
            await _context.DeleteAsync<User>(userId);
        }
	}
}
