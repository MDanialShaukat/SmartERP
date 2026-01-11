using SmartERP.Models.Entities;

namespace SmartERP.Core.Services
{
    public interface IAuthenticationService
    {
        Task<User?> LoginAsync(string username, string password);
        void Logout();
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }
        bool HasPermission(string permission);
    }
}
