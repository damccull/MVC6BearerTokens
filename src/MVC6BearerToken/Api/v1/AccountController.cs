using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Authentication.OAuthBearer;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Framework.OptionsModel;
using MVC6BearerToken.Models;
using MVC6BearerToken.DAL;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MVC6BearerToken.Api.v1 {
    [Route("api/v1/[controller]")]
    public class AccountController : Controller {

        private readonly IAuthRepository authRepo;
        private readonly IOptions<OAuthBearerAuthenticationOptions> bearerOptions;
        private readonly SigningCredentials signingCredentials;

        public AccountController(IAuthRepository authRepo, IOptions<OAuthBearerAuthenticationOptions> bearerOptions, SigningCredentials signingCredentials) {
            this.authRepo = authRepo;
            this.bearerOptions = bearerOptions;
            this.signingCredentials = signingCredentials;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model) {
            if(!ModelState.IsValid) {
                return new BadRequestObjectResult(ModelState);
            }

            var result = await authRepo.RegisterUserAsync(model);
            var errorResult = GetErrorResult(result);

            if(errorResult != null) {
                return errorResult;
            }

            return new CreatedResult("/token", null);//HTTP Created
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Token([FromBody] LoginViewModel model) {
            ClaimsIdentity identity = null;

            //Clean out expired tokens.
            await authRepo.CleanExiredRefreshTokens();


            if(String.IsNullOrWhiteSpace(model.GrantType)) {
                return new BadRequestObjectResult(new { message = "Request must contain a 'grantType' property." });
            }

            //Check if the user is logging in via password or refresh_token
            if(model.GrantType == "password") {
                //We need to authorize the user.
                identity = await authRepo.LoginUserAsync(model);
                if(identity == null) {
                    return new HttpUnauthorizedResult();
                }
            } else if(model.GrantType == "refreshToken") {
                var existingRefreshToken = await authRepo.FindRefreshTokenAsync(Utility.Util.GetHash(model.RefreshToken));
                if(existingRefreshToken == null) {
                    //No refresh token mean's it's invalid or revoked
                    return new HttpUnauthorizedResult();
                }

                if(existingRefreshToken.ClientId != model.ClientId) {
                    return new HttpUnauthorizedResult();
                }

                identity = await authRepo.GetClaimsIdentityByNameAsync(existingRefreshToken.Subject);
                await authRepo.RemoveRefreshTokenAsync(existingRefreshToken);

            } else {
                return new BadRequestObjectResult(new { message = "The 'grantType' must be either 'password' or 'refreshToken'." });
            }

            var client = await authRepo.FindClientAsync(model.ClientId);
            if(client == null) {
                return new HttpNotFoundObjectResult(new { message = "No such ClientId." });
            }
            
            if(string.IsNullOrWhiteSpace(client.AllowedOrigin) || client.AllowedOrigin == "*") {
                //If origin is blank or *, we need to check client secret to ensure we don't allow just any client to spoof its way in
                if(model.ClientSecret != client.Secret) {
                    //Sent secret doesn't match stored client secret...GTFO
                    return new HttpUnauthorizedResult();
                }
            }
            //TODO: Authorized Origins check, and integrate with CORS

            var newRefreshTokenId = Guid.NewGuid().ToString("n");

            var refreshToken = new RefreshToken {
                ClientId = client.Id,
                Id = Utility.Util.GetHash(newRefreshTokenId),
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(5)),
                Subject = identity.Name
            };

            await authRepo.AddRefreshTokenAsync(refreshToken);

            string token = GenerateAccessToken(identity);
            return new JsonResult(new TokenDTO {
                AccessToken = token,
                RefreshToken = newRefreshTokenId,
                ClientId = refreshToken.ClientId,
                ExpiresIn = Convert.ToInt32(refreshToken.ExpiresUtc.Subtract(DateTime.UtcNow).TotalMinutes),
                Expires = refreshToken.ExpiresUtc.ToString(),
                Issued = refreshToken.IssuedUtc.ToString(),
                TokenType = "bearer",
                UserName = refreshToken.Subject
            });
        }

        private string GenerateAccessToken(ClaimsIdentity identity) {
            var handler = bearerOptions
                .Options
                .SecurityTokenValidators
                .OfType<System.IdentityModel.Tokens.JwtSecurityTokenHandler>()
                .First();


            // The identity here is the ClaimsIdentity you want to authenticate the user as. You can add your own custom claims to it if you like.
            // You can get this using the SignInManager if you're using Identity.
            var securityToken = handler.CreateToken(
                issuer: bearerOptions.Options.TokenValidationParameters.ValidIssuer,
                audience: bearerOptions.Options.TokenValidationParameters.ValidAudience,
                signingCredentials: signingCredentials,
                subject: identity);
            var token = handler.WriteToken(securityToken);
            return token;
        }

        private IActionResult GetErrorResult(IdentityResult result) {
            if(result == null) {
                return new HttpStatusCodeResult(500);
            }

            if(!result.Succeeded) {
                if(result.Errors != null) {
                    foreach(var error in result.Errors) {
                        ModelState.AddModelError("", $"{error.Code}: {error.Description}");
                    }
                }
                if(ModelState.IsValid) {
                    return new BadRequestResult();
                }
                return HttpBadRequest(ModelState);
            }
            return null;
        }
    }
}
