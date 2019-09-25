using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingSite.API.Data;
using DatingSite.API.Dtos;
using DatingSite.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingSite.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repos;

        public IConfiguration _config { get; }

        public AuthController(IAuthRepository repos, IConfiguration config)
        {
            this._repos = repos;
            _config = config;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserForReisterDto userDto)
        {
            userDto.UserName = userDto.UserName.ToLower();
            if (await _repos.UserExists(userDto.UserName))
                return BadRequest("User name already exists");

            var userToCreate = new User() { UserName = userDto.UserName };
            var createdUser = await _repos.Register(userToCreate, userDto.Password);
            return StatusCode(201);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto user)
        {
            var userForRepro = await _repos.Login(user.Username.ToLower(), user.Password);
            if (user == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,userForRepro.Id.ToString()),
                new Claim(ClaimTypes.Name, userForRepro.UserName)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDesc);
            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
        }

    }
}