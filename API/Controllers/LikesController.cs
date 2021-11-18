using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        // private readonly ILikeRepository _likeRepository;
        // private readonly IUserRepository _userRepository;

        public LikesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            // _likeRepository = likeRepository;
            // _userRepository = userRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _unitOfWork.UserRepository.GetUserByIdAsync(sourceUserId);
            if (likedUser == null) return NotFound("user not found");
            if (sourceUser.UserName == username) return BadRequest("you can't like yourself");
            var userLike = await _unitOfWork.LikeRepository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null) return BadRequest("you already liked this one");
            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };
            sourceUser.LikedUsers.Add(userLike);
            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("failed to save");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var userLikes = await _unitOfWork.LikeRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeaders(userLikes.CurrentPage, userLikes.PageSize, userLikes.TotalCount, userLikes.TotalPage);
            return Ok(userLikes);
        }
    }
}