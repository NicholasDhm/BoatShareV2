using boat_share.Models;
using boat_share.DTOs;

namespace boat_share.Abstract
{
	public interface IUserService
	{
		Task<List<UserListDTO>> GetUsersAsync();
		Task<UserInfoDTO?> GetUserByIdAsync(int userId);
		Task<User?> CreateUserAsync(UserCreateDTO userCreateDto);
		Task<UserInfoDTO?> UpdateUserAsync(int userId, UserUpdateDTO userUpdateDto);
		Task<bool> DeleteUserAsync(int userId);
		Task<UserInfoDTO?> AddQuotaBackAsync(int userId, string reservationType);
		Task<List<UserListDTO>> SearchUsersByNameAsync(string partialName);
	}
}
