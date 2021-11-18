using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var MinDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var MaxDob = DateTime.Today.AddYears(-userParams.MinAge);
            var OrderBy = userParams.OrderBy;

            var query = _context.Users
                .Where(u => u.UserName != userParams.CurrentUsername && u.Gender == userParams.Gender)
                .Where(u => u.DateOfBirth >= MinDob && u.DateOfBirth <= MaxDob)
                .OrderByDescending(u => OrderBy == "lastActive" ? u.LastActive : u.Created)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking();
            return await PagedList<MemberDto>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users
                .Where(u => u.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(u => u.Photos).ToListAsync();
        }

        // public async Task<bool> SaveAllAsync()
        // {
        //     return await _context.SaveChangesAsync() > 0;
        // }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users.Where(u => u.UserName == username)
                .Select(u=>u.Gender).FirstOrDefaultAsync();
        }
    }
}