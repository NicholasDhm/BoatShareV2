using boat_share.Models;

namespace boat_share.Abstract
{
	public interface IBoatService
	{
		Task<List<Boat>> GetBoatsAsync();

		Task<Boat> GetBoatByIdAsync(string boatId);

		Task AddBoatAsync(Boat boat);

		Task DeleteBoatAsync(string boatId);

		Task UpdateBoatAsync(Boat boat);

		Task AssignUserToBoatAsync(string boatId, string userId);
	}
}
