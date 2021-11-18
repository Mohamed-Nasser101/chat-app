using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        // private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            // _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            //var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            var userGender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
            
            // userParams.CurrentUsername = user.UserName;
            userParams.CurrentUsername = User.GetUsername();
            if (string.IsNullOrEmpty(userParams.Gender)) { userParams.Gender = userGender == "male" ? "male" : "female"; }

            var usersDto = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeaders(usersDto.CurrentPage, usersDto.PageSize, usersDto.TotalCount,
                usersDto.TotalPage);
            return Ok(usersDto);
        }

        [HttpGet("{username}", Name = "username")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await _unitOfWork.UserRepository.GetMemberAsync(username);

            if (user == null)
                return NotFound();

            //var userDto = _mapper.Map<MemberDto>(user);
            return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUsername();
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDto, user);
            _unitOfWork.UserRepository.Update(user);

            if (await _unitOfWork.Complete())
                return NoContent();

            return BadRequest("failed to update the information");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (!user.Photos.Any()) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _unitOfWork.Complete())
            {
                // return _mapper.Map<PhotoDto>(photo);
                return CreatedAtRoute("username", new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("problem adding the photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo.IsMain) return BadRequest("photo is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);

            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("something went wrong");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo.IsMain) return BadRequest("can't delete main photo");
            if (photo.Id == 0) return NotFound();
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("something went wrong");
        }
    }
}