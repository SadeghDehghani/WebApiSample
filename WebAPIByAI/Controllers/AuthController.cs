using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPIByAI.Interfaces;
using WebAPIByAI.Models;

namespace WebAPIByAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshService;
        private readonly IHttpContextAccessor _httpContextAccessor ;
        private readonly IConfiguration _configuration ;

        public AuthController(IUserService userService, IRefreshTokenService refreshService,
            IHttpContextAccessor httpContextAccessor,IConfiguration configuration)
        {
            _userService = userService;
            _refreshService = refreshService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }


        [HttpPost(template: "register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            var user = new User { Username = model.Username };
            _userService.Register(user, model.Password);

            // تولید JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings").GetSection("Key").Value);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // تولید Refresh Token
            _refreshService.RemoveOldTokens(user);
            var refreshToken = _refreshService.GenerateRefreshToken(user);

            return Ok(new
            {
                accessToken,
                refreshToken = refreshToken.Token
            });
        }
  
        [HttpPost(template: "login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);
            if (user == null) return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();


            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings").GetSection("Key").Value);
           // var key = Encoding.ASCII.GetBytes("SuperSecretKey@3456789012345678901234");


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user.Username)
            }),

                //Issuer = "YourIssuer", 
                //Audience = "YourAudience", 
       
                Expires = DateTime.UtcNow.AddMinutes(value: 30),
                SigningCredentials = new SigningCredentials(key: new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            _refreshService.RemoveOldTokens(user);

            var refreshToken = _refreshService.GenerateRefreshToken(user);

            return Ok(new
            {
                accessToken,
                refreshToken = refreshToken.Token
            });
        }

        [HttpPost(template: "refresh")]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            var storedToken = _refreshService.GetRefreshToken(refreshToken);
            if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow)
                return Unauthorized();

            var user = storedToken.User;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection(key: "JwtSettings").GetSection("Key").Value);

            //var key = Encoding.ASCII.GetBytes("SuperSecretKey@3456789012345678901234");


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user!.Username)
            }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var newToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(newToken);

            return Ok(new { accessToken });
        }
    }
}
