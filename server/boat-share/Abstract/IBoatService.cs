using boat_share.Models;
using boat_share.DTOs;

namespace boat_share.Abstract
{
	public interface IBoatService
	{
		Task<List<BoatDTO>> GetBoatsAsync();
		Task<BoatDTO?> GetBoatByIdAsync(int boatId);
		Task<Boat> CreateBoatAsync(BoatCreateDTO boatCreateDto);
		Task<BoatDTO?> UpdateBoatAsync(int boatId, BoatUpdateDTO boatUpdateDto);
		Task<bool> DeleteBoatAsync(int boatId);
		Task<bool> AssignUserToBoatAsync(int boatId, int userId);
	}
}
