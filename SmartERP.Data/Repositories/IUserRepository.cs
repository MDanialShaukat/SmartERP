using System.Threading.Tasks;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> AuthenticateAsync(string username, string password);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}
