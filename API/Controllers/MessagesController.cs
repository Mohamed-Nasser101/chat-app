using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        // private readonly IUserRepository _userRepository;
        // private readonly IMessageRepository _messageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            // _userRepository = userRepository;
            // _messageRepository = messageRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("can't send messages to your self");
            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower());

            if (recipient == null) return NotFound();
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = username,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            _unitOfWork.MessageRepository.AddMessage(message);
            if (await _unitOfWork.Complete())
                return Ok(_mapper.Map<MessageDto>(message));
            return BadRequest("something wrong happened");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser(
            [FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _unitOfWork.MessageRepository.GetMessageForUserAsync(messageParams);
            Response.AddPaginationHeaders(messages.CurrentPage, messages.PageSize,
                messages.TotalCount, messages.TotalPage);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();
            return Ok(await _unitOfWork.MessageRepository.GetMessageThreadAsync(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await _unitOfWork.MessageRepository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username)
                return Unauthorized();

            if (message.Recipient.UserName == username) message.RecipientDelete = true;
            if (message.Sender.UserName == username) message.SenderDelete = true;

            if (message.SenderDelete && message.RecipientDelete)
                _unitOfWork.MessageRepository.DeleteMessage(message);
            if (await _unitOfWork.Complete())
                return Ok();
            
            return BadRequest("failed to delete");
        }
    }
}