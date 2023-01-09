using Jobs.Models;
using Jobs.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.Xml;

namespace Jobs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class authController : ControllerBase
    {
        private readonly IAuthService _authService;

        public authController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterAsync(RegiserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterUserAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            SetRefreshTokenInCookie(result.Token, result.RefreshTokenExpiration);
            return Ok(new { token = result.Token, roles = result.Roles });

        }

        //[HttpPost("register")]
        //public async Task<ActionResult> RegisterAdminAsync(RegiserModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var result = await _authService.RegisterUserAsync(model);

        //    if (!result.IsAuthenticated)
        //        return BadRequest(result.Message);

        //    return Ok(new { token = result.Token, roles = result.Roles });

        //}

        [HttpPost("token")]
        public async Task<ActionResult> GetTokenAsync(TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.GetTokenAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            if (!string.IsNullOrEmpty(result.RefreshToken))
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(new { token = result.Token, roles = result.Roles });

        }

        [HttpPost("company")]
        public async Task<ActionResult> company(CompanyRegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.CreateCompany(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);
            SetRefreshTokenInCookie(result.Token, result.RefreshTokenExpiration);
            return Ok(new { token = result.Token,  roles = result.Roles, company = model.Company });
        }

        [Authorize(Roles = "company")]
        //[Authorize]
        [HttpPost("supervisor")]
        public async Task<ActionResult> supervisor(SupervisorRegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = User.FindFirstValue("username");
            var role = User.FindFirstValue(ClaimTypes.Role);

            var result = await _authService.CreateSupervisor(model, user);

            if (result.IsAuthenticated)
                return Ok();
            return BadRequest();
        }

        [HttpGet("refreshToken")]
        public async Task<ActionResult> RefreshToken()
        {
            var token = Request.Cookies["refreshToken"];
            if (token != null)
            {
                var result = await _authService.RefreshTokenAsync(token);
                if (result.IsAuthenticated)
                {
                    SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
                    return Ok(result);
                }
            }
            return BadRequest();
        }

        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    
    }
}
