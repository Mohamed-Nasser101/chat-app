using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace API.Data
{
    public class LikeRepository : ILikeRepository
    {
        private readonly DataContext _context;

        public LikeRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(u => u.LikedUsers)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(l => l.SourceUserId == likesParams.UserId);
                users = likes.Select(u => u.LikedUser);
            } 
            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(l => l.LikedUserId == likesParams.UserId);
                users = likes.Select(u => u.SourceUser);
            }

            var likedUsers = users.Select(user => new LikeDto
            {
                Username = user.UserName,
                Age = user.DateOfBirth.CalculateAge(),
                KnownAs = user.KnownAs,
                City = user.City,
                Id = user.Id,
                PhotoUrl = user.Photos.FirstOrDefault(p=>p.IsMain).Url
                
            });
            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }
    }
}