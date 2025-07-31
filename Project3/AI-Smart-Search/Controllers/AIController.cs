using AI_Smart_Search.Dbcontext;
using AI_Smart_Search.Dtos;
using AI_Smart_Search.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AI_Smart_Search.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly AIDbContext dbContext;
        private readonly IConfiguration configuration;

        public AIController(AIDbContext dbContext,IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }
        [AllowAnonymous]
        [Route("Token")]
        [HttpPost]
        public IActionResult Login([FromBody]  LoginDto loginDto)
        {
            var result = dbContext.Users.FirstOrDefault(p => p.Email == loginDto.Email && p.Password == loginDto.Password);
            var tokenDto = new TokenDto();
            if (result != null)
            {
                var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, loginDto.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: configuration["JwtSettings:Issuer"],
                    audience: configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds
                );
                string tokenKey = new JwtSecurityTokenHandler().WriteToken(token);
                if (tokenKey == null)
                {
                    return Unauthorized();
                }
                //tokenDto.Token = tokenKey;
                tokenDto = new TokenDto
                {
                    Token = tokenKey,
                    UserId = result.UserId,
                    RoleId = result.RoleId
                };
            }
            return Ok(tokenDto);
        }
        [AllowAnonymous]
        [HttpPost("PostUser")]
        public IActionResult PostUser(UserDto userDto)
        {
            var roleExists = dbContext.Roles.Any(r => r.RoleId == userDto.RoleId);
            if (!roleExists)
            {
                return BadRequest("RoleId not found. Cannot create user.");
            }
            var result = new User
            {
                UserId = userDto.UserId,
                Name = userDto.Name,
                Password = userDto.Password,
                Email = userDto.Email,
                RoleId = userDto.RoleId,
                DOB = userDto.DOB,
                Gender = userDto.Gender,
                Address = userDto.Address
            };
            dbContext.Users.Add(result);
            dbContext.SaveChanges();
            return Ok(result);
        }
        [Route("GetUsers")]
        [HttpGet]
        public IActionResult GetUser()
        {
            var userList = dbContext.Users.ToList();
            List<UserDto> result = new List<UserDto>();
            foreach (var item in userList)
            {
                result.Add(new UserDto()
                {

                    UserId = item.UserId,
                    Name = item.Name,
                    Password = item.Password,
                    Email = item.Email,
                    RoleId = item.RoleId,
                    DOB = item.DOB,
                    Gender = item.Gender,
                    Address = item.Address
                });
            }
            return Ok(result);
        }
        [Route("GetUser/{id:int}")]
        [HttpGet]
        public IActionResult GetUserById(int id)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            var userDto = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Password = user.Password,
                Email = user.Email,
                RoleId = user.RoleId,
                DOB = user.DOB,
                Gender = user.Gender,
                Address = user.Address
            };
            return Ok(userDto);
        }
        [Route("DeleteUser/{id:int}")]
        [HttpDelete]
        public IActionResult DeleteUser(int id)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            dbContext.Users.Remove(user);
            dbContext.SaveChanges();
            return Ok();
        }
        [HttpPut("PutUser/{id:int}")]
        public IActionResult PutUser([FromRoute] int id, [FromBody] UserDto userDto)
        {
            var post = dbContext.Users.FirstOrDefault(x => x.UserId == id);
            if (post == null)
                return NotFound("User not found");

            // You don't need to update UserId here
            post.Name = userDto.Name;
            post.Password = userDto.Password;
            post.Email = userDto.Email;
            post.DOB = userDto.DOB;
            post.Gender = userDto.Gender;
            post.Address = userDto.Address;

            dbContext.SaveChanges();

            return Ok(post);
        }

        [HttpPost("PostQA")]
        public IActionResult PostQA(QApair qApair)
        {
            var result = new QApair
            {
                QApairId = qApair.QApairId,
                Question = qApair.Question,
                Answer = qApair.Answer
            };
            dbContext.QApairs.Add(result);
            dbContext.SaveChanges();
            return Ok(result);
        }
        [HttpGet("GetQA")]
        public IActionResult GetQA()
        {
            var qalist = dbContext.QApairs.ToList();
            List<QApairDto> result = new List<QApairDto>();
            foreach (var item in qalist)
            {
                result.Add(new QApairDto()
                {
                    QApairId = item.QApairId,
                    Question = item.Question,
                    Answer = item.Answer
                });
            }
            return Ok(result);
        }
        [HttpGet("GetAnswer")]
        public async Task<IActionResult> GetAnswerAsync([FromQuery] string userQuestion)
        {
            var answer = await dbContext.QApairs
                .Where(q => q.Question.ToLower().Contains(userQuestion.ToLower()))
                .Select(q => q.Answer)
                .FirstOrDefaultAsync();

            var result = new { answer = answer ?? "Sorry, I don't know the answer to that question." };
            return Ok(result);
        }
        [HttpPost("PostRole")]
        public IActionResult PostRole(RoleDto roleDto)
        {
            var result = new Role
            {
                RoleId = roleDto.RoleId,
                RoleName = roleDto.RoleName
            };
            dbContext.Roles.Add(result);
            dbContext.SaveChanges();
            return Ok(result);
        }
    }
}
