using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class Account : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public Account(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO UserDto)
        {
            // if (!ModelState.IsValid)
            //     return BadRequest();  done automatically I guess

            if (await UserExists(UserDto.Username)) return BadRequest("username already exists");

            var user = _mapper.Map<AppUser>(UserDto);

            // using var hm = new HMACSHA512();

            user.UserName = UserDto.Username.ToLower();
            
            var result = await _userManager.CreateAsync(user, UserDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            
            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            // user.PasswordHash = hm.ComputeHash(Encoding.UTF8.GetBytes(UserDto.Password));
            // user.PasswordSalt = hm.Key;

            // _context.Users.Add(user);
            // await _context.SaveChangesAsync();

            var newUser = new UserDTO
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };

            return Created("https://localhost:5001/api/user/" + user.Id, newUser);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO user)
        {
            var userInDb = await _userManager.Users.Include(u => u.Photos)
                .SingleOrDefaultAsync(u => u.UserName == user.Username.ToLower());

            if (userInDb == null) return Unauthorized("invalid username");

            //using var hm = new HMACSHA512(userInDb.PasswordSalt);
            // var computedHash = hm.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
            //
            // if (computedHash.Where((t, i) => t != userInDb.PasswordHash[i]).Any())
            // {
            //     return Unauthorized("Invalid password");
            // }
            var result = await _signInManager.CheckPasswordSignInAsync(userInDb, user.Password, false);
            if (!result.Succeeded) return Unauthorized();
            var newUser = new UserDTO
            {
                UserName = userInDb.UserName,
                Token = await _tokenService.CreateToken(userInDb),
                PhotoUrl = userInDb.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                KnownAs = userInDb.KnownAs,
                Gender = userInDb.Gender
            };

            return newUser;
        }

        private async Task<bool> UserExists(string UserName)
        {
            return await _userManager.Users.AnyAsync(u => u.UserName == UserName.ToLower());
        }
    }
}