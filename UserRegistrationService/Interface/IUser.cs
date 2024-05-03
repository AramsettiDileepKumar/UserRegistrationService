using UserManagementService.DTO;
using UserManagementService.Entity;
using UserManagementService.Model;

namespace UserManagementService.Interface
{
    public interface IUser
    {
        public Task<bool> RegisterUser(UserEntity User);
        public Task<string> UserLogin(UserLoginModel userLogin);
        public Task<IEnumerable<UserResponse>> GetUsers();
        public Task<UserResponse> GetUserByUserId(int UserId);

    }
}
