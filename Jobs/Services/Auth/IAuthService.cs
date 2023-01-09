using Jobs.Models;

namespace Jobs.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterUserAsync(RegiserModel model);
        Task<AuthModel> GetTokenAsync(TokenRequestModel model);
        Task<AuthModel> CreateCompany(CompanyRegisterModel model);
        Task<AuthModel> CreateSupervisor(SupervisorRegisterModel model, string username);
        Task<AuthModel> RefreshTokenAsync(string token);
    }
}