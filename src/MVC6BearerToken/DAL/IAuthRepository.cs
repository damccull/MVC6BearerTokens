using Microsoft.AspNet.Identity;
using MVC6BearerToken.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVC6BearerToken.DAL {
    public interface IAuthRepository {
        Task<IdentityResult> RegisterUserAsync(RegisterViewModel user);
        Task<ClaimsIdentity> LoginUserAsync(LoginViewModel model);
        Task<ClaimsIdentity> GetClaimsIdentityByNameAsync(string email);
        Task<Client> FindClientAsync(string clientId);
        Task<bool> AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<bool> RemoveRefreshTokenAsync(RefreshToken refreshToken);
        Task<bool> RemoveRefreshTokenAsync(string refreshTokenId);
        Task<RefreshToken> FindRefreshTokenAsync(string refreshTokenId);
        Task<IEnumerable<RefreshToken>> GetAllRefreshTokensAsync();
        Task CleanExiredRefreshTokens();
    }
}
