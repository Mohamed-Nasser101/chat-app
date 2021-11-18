using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        // private readonly IMessageRepository _messageRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        // private readonly IUserRepository _userRepository;
        private readonly PresenceTracker _tracker;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public MessageHub(IUnitOfWork unitOfWork, IMapper mapper, PresenceTracker tracker,
            IHubContext<PresenceHub> presenceHub)
        {
            // _messageRepository = messageRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            // _userRepository = userRepository;
            _tracker = tracker;
            _presenceHub = presenceHub;
        }

        public override async Task OnConnectedAsync()
        {
            var context = Context.GetHttpContext();
            var otherUser = context.Request.Query["user"].ToString();
            var groupName = GetGroupName(context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await AddConnectionToGroup(groupName);
            var messages = await _unitOfWork.MessageRepository
                .GetMessageThreadAsync(Context.User.GetUsername(), otherUser);

            if (_unitOfWork.HasChanges()) await _unitOfWork.Complete();

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await RemoveConnectionFromGroup();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("can't send messages to your self");
            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient =
                await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower());

            if (recipient == null) throw new HubException("user not found");
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = username,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            if (group.Collections.Any(c => c.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _tracker.GetUserConnection(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections)
                        .SendAsync("NewMessageReceived", new {username = sender.UserName, sender.KnownAs});
                }
            }

            _unitOfWork.MessageRepository.AddMessage(message);
            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName)
                    .SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private async Task<bool> AddConnectionToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Collections.Add(connection);
            return await _unitOfWork.Complete();
        }

        private async Task RemoveConnectionFromGroup()
        {
            var connection = await _unitOfWork.MessageRepository.GetConnection(Context.ConnectionId);
            _unitOfWork.MessageRepository.RemoveConnection(connection);
            await _unitOfWork.Complete();
        }

        private string GetGroupName(string caller, string other)
        {
            var compare = string.CompareOrdinal(caller, other) < 0;
            return compare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}