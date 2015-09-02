using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using MVC6BearerToken.Models;

namespace MVC6BearerToken.DAL {
    public class AuthRepository : IAuthRepository {

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;


        public AuthRepository(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) {
            this.db = context;
            this.userManager = userManager;
            this.signInManager = signInManager;

        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model) {
            ApplicationUser user = new ApplicationUser {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);
            return result;
        }

        public async Task<ClaimsIdentity> LoginUserAsync(LoginViewModel model) {
            var user = await userManager.FindByEmailAsync(model.Email);
            if(user == null) {
                return null;
            }
            var result = await userManager.CheckPasswordAsync(user, model.Password);
            if(!result) {
                return null;
            }
            var identity = (await signInManager.CreateUserPrincipalAsync(user)).Identity as ClaimsIdentity;
            //TODO: Add any claims to the user identity here.
            return identity;
        }


        public async Task<ClaimsIdentity> GetClaimsIdentityByNameAsync(string username) {
            var user = await userManager.FindByNameAsync(username);
            var identity = (await signInManager.CreateUserPrincipalAsync(user)).Identity as ClaimsIdentity;
            return identity;
        }

        public async Task<Client> FindClientAsync(string clientId) {
            return await db.Clients.SingleOrDefaultAsync(c => c.Id == clientId);
        }

        public async Task<bool> AddRefreshTokenAsync(RefreshToken refreshToken) {
            var existingToken = await db.RefreshTokens.SingleOrDefaultAsync(r => r.Subject == refreshToken.Subject && r.Id == refreshToken.Id);
            if(existingToken != null) {
                var result = await RemoveRefreshTokenAsync(existingToken);
            }

            db.RefreshTokens.Add(refreshToken);

            return await db.SaveChangesAsync() > 0; // True if number rows inserted > 0
        }

        public async Task<bool> RemoveRefreshTokenAsync(RefreshToken refreshToken) {
            db.RefreshTokens.Remove(refreshToken);
            return await db.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveRefreshTokenAsync(string refreshTokenId) {
            var token = await db.RefreshTokens.SingleOrDefaultAsync(r => r.Id == refreshTokenId);
            if(token == null) {
                return false;
            }
            return await RemoveRefreshTokenAsync(token);
        }

        public async Task<RefreshToken> FindRefreshTokenAsync(string refreshTokenId) {
            return await db.RefreshTokens.SingleOrDefaultAsync(r => r.Id == refreshTokenId);
        }

        public async Task<IEnumerable<RefreshToken>> GetAllRefreshTokensAsync() {
            return await db.RefreshTokens.ToListAsync();
        }

        public async Task CleanExiredRefreshTokens() {
            var tokens = db.RefreshTokens.Where(t => t.ExpiresUtc < DateTime.UtcNow)
                .ForEachAsync(t => {
                    db.RefreshTokens.Remove(t);
                });
            await db.SaveChangesAsync();
        }
    }
}
