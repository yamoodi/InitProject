using Azure.Core;
using Jobs.Data;
using Jobs.Helper;
using Jobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;

namespace Jobs.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWT _JWT;
        private readonly ApplicationDbContext _db;

        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, ApplicationDbContext db)
        {
            _userManager = userManager;
            _JWT = jwt.Value;
            _db = db;
        }
        public async Task<AuthModel> RegisterUserAsync(RegiserModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "email is already taken" };

            if (await _userManager.FindByNameAsync(model.Username) is not null)
                return new AuthModel { Message = "username is already taken" };

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var resutl = await _userManager.CreateAsync(user, model.Password);
            if (!resutl.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in resutl.Errors)
                {
                    errors += $"{error.Description},";
                }
                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, Consts.Roles.User);
            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                IsAuthenticated = true,
                Email = user.Email,
                Username = user.UserName,
                //ExpirationOn = jwtSecurityToken.ValidTo,
                Roles = new List<string> { Consts.Roles.User },
                Token = await CreateJwtToken(user)
            };
        }
        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authmodel = new AuthModel();

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authmodel.Message = "email or password is invlaid";
                return authmodel;
            }

           
            var roles = await _userManager.GetRolesAsync(user);
            authmodel.IsAuthenticated = true;
            authmodel.Token = await CreateJwtToken(user);// CreateJwtToken. nHandler.WriteToken(jwtToken);
            authmodel.Email = user.Email;
            authmodel.Username = user.UserName;
            //authmodel.ExpirationOn = jwtToken.ValidTo;
            authmodel.Roles = roles.ToList();
            if(user.RefreshTokens.Any(x => x.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.IsActive);
                authmodel.RefreshToken = activeRefreshToken.Token;
                authmodel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
            }
            else
            {
                var refreshToken = GenerateRefreshToken();
                authmodel.RefreshToken = refreshToken.Token;
                authmodel.RefreshTokenExpiration = refreshToken.ExpiresOn;
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);

            }
            return authmodel;
        }
        public async Task<AuthModel> CreateCompany(CompanyRegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "email is already taken" };

            if (await _userManager.FindByNameAsync(model.Username) is not null)
                return new AuthModel { Message = "username is already taken" };

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {

                var resutl = await _userManager.CreateAsync(user, model.Password);
                if (!resutl.Succeeded)
                {
                    var errors = string.Empty;
                    foreach (var error in resutl.Errors)
                    {
                        errors += $"{error.Description},";
                    }
                    return new AuthModel { Message = errors };
                }
                await _userManager.AddToRoleAsync(user, Consts.Roles.Comapny);
                _db.Companies.Add(new Company
                {
                    Active = true,
                    CreatedDate = DateTime.UtcNow,
                    Name = model.Company,
                    Username = model.Username
                });
                _db.SaveChanges();
                transaction.Complete();
            }
            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                IsAuthenticated = true,
                Email = user.Email,
                Username = user.UserName,
                //ExpirationOn = jwtSecurityToken.ValidTo,
                Roles = new List<string> { Consts.Roles.Comapny },
                Token = await CreateJwtToken(user)
            };
        }
        public async Task<AuthModel> CreateSupervisor(SupervisorRegisterModel model, string username)
        {

            var company = await _db.Companies.SingleOrDefaultAsync(x => x.Username == username);

            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "email is already taken" };

            if (await _userManager.FindByNameAsync(model.Username) is not null)
                return new AuthModel { Message = "username is already taken" };

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var resutl = await _userManager.CreateAsync(user, model.Password);
            if (!resutl.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in resutl.Errors)
                {
                    errors += $"{error.Description},";
                }
                return new AuthModel { Message = errors };
            }
            await _userManager.AddToRoleAsync(user, Consts.Roles.SuperVisor);

            company.SuperVisors.Add(new SuperVisor
            {
                Active = true,
                CompanyId = company.Id,
                CreatedDate = DateTime.UtcNow,
                Username = model.Username
            });
            await _db.SaveChangesAsync();

            return new AuthModel { IsAuthenticated = true };
        }
        private async Task<string> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleclaims = new List<Claim>();

            foreach (var r in roles)
                roleclaims.Add(new Claim(ClaimTypes.Role, r));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                 new Claim("username", user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            }
            .Union(userClaims)
            .Union(roleclaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JWT.key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            var jwtdesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(20),
                SigningCredentials = signingCredentials
            };
            var jwtToken = jwtSecurityTokenHandler.CreateToken(jwtdesc);
            return jwtSecurityTokenHandler.WriteToken(jwtToken);
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(10),
                CreatedOn = DateTime.UtcNow
            };
        }

        public async Task<AuthModel> RefreshTokenAsync(string token)
        {
            var authModel = new AuthModel();

            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.RefreshTokens.Any(a => a.Token == token));
            if (user == null)
            {
                authModel.IsAuthenticated = false;
                return authModel;
            }

            var refreshToken =  user.RefreshTokens.SingleOrDefault(t => t.Token == token);

            if(!refreshToken.IsActive)
            {
                authModel.IsAuthenticated = false;
                return authModel;
            }

            refreshToken.RevokedOn = DateTime.UtcNow;
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken); 
            await _userManager.UpdateAsync(user);
            var jwtToken = await CreateJwtToken(user);

            authModel.IsAuthenticated = true;
            authModel.Token = jwtToken;
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            var roles = await _userManager.GetRolesAsync(user);
            authModel.Roles = roles.ToList();
            authModel.RefreshToken = newRefreshToken.Token;
            authModel.RefreshTokenExpiration = newRefreshToken.ExpiresOn;
            return authModel;
        }

    }
}
