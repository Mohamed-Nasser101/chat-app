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
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(x => x.Recipient)
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessageForUserAsync(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.RecipientUsername == messageParams.Username && !m.RecipientDelete),
                "Outbox" => query.Where(m => m.SenderUsername == messageParams.Username && !m.SenderDelete),
                _ => query.Where(m =>
                    m.RecipientUsername == messageParams.Username && !m.RecipientDelete && m.DateRead == null)
            };
            // var message = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername,
            string recipientUsername)
        {
            var messages = await _context.Messages
                // .Include(m => m.Sender).ThenInclude(s => s.Photos)
                // .Include(m => m.Recipient).ThenInclude(s => s.Photos)
                .Where(m =>
                    m.Recipient.UserName == currentUsername && m.Sender.UserName == recipientUsername &&
                    !m.RecipientDelete ||
                    m.Sender.UserName == currentUsername && m.Recipient.UserName == recipientUsername &&
                    !m.SenderDelete)
                .OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            // var unreadMessages = messages
            //     .Where(m => m.DateRead == null && m.Recipient.UserName == currentUsername).ToList();
            var unreadMessages = messages
                .Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                //await _context.SaveChangesAsync();
            }

            // return _mapper.Map<IEnumerable<MessageDto>>(messages);
            return messages;
        }

        // public async Task<bool> SaveAllAsync()
        // {
        //     return await _context.SaveChangesAsync() > 0;
        // }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Collections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }
    }
}