using System;
using System.Threading.Tasks;
using SmartERP.Data.Repositories;
using SmartERP.Models.Entities;

namespace SmartERP.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private User? _currentUser;

        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? CurrentUser => _currentUser;

        public bool IsAuthenticated => _currentUser != null;

        public bool IsAdmin => _currentUser?.Role == "Admin";

        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _userRepository.AuthenticateAsync(username, password);
            
            if (user != null)
            {
                _currentUser = user;
            }

            return user;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public bool HasPermission(string permission)
        {
            if (!IsAuthenticated || _currentUser == null)
                return false;

            // Admin has all permissions
            if (IsAdmin)
                return true;

            // Define User role permissions
            var userPermissions = new[]
            {
                "Inventory.View",
                "Inventory.Update",
                "Customer.View",
                "Billing.View",
                "Billing.Add",
                "Billing.Update"
            };

            return Array.Exists(userPermissions, p => p.Equals(permission, StringComparison.OrdinalIgnoreCase));
        }
    }
}
