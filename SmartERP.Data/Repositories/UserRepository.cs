using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartERP.Models.Entities;

namespace SmartERP.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(SmartERPDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            
            if (user == null || !user.IsActive)
                return null;

            // Verify password using BCrypt
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            
            if (!isValidPassword)
                return null;

            // Update last login date
            user.LastLoginDate = DateTime.Now;
            await UpdateAsync(user);

            return user;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            
            if (user == null)
                return false;

            // Verify old password
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
            
            if (!isValidPassword)
                return false;

            // Hash and update new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await UpdateAsync(user);

            return true;
        }
    }
}
