using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var userName = User.GetUserName();
        if (userName == createMessageDto.RecipientUserName.ToLower())
            return BadRequest("You cannot message yourself");

        var sender = await userRepository.GetUserByUserNameAsync(userName);
        var recipient = await userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null)
            return BadRequest("Cannot send the message");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUserName = sender.UserName,
            RecipientUserName = recipient.UserName,
            Content = createMessageDto.Content
        };

        messageRepository.AddMessage(message);
        if (await messageRepository.SaveAllAsync())
            return Ok(mapper.Map<MessageDto>(message));

        return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams) 
    {
        messageParams.UserName = User.GetUserName();

        var messages = await messageRepository.GetMessagesForUser(messageParams);
        Response.AddPaginationHeader(messages);
        return messages;
    }

    [HttpGet("thread/{userName}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string userName) 
    {
        var currentUserName = User.GetUserName();

        return Ok(await messageRepository.GetMessageThread(currentUserName, userName));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var userName = User.GetUserName();

        var message = await messageRepository.GetMessage(id);

        if (message == null) return BadRequest("Cannot delete this message");

        if (message.SenderUserName != userName && message.RecipientUserName != userName) return Forbid();

        if (message.SenderUserName == userName) message.SenderDeleted = true;
        if (message.RecipientUserName == userName) message.RecipientDeleted = true;

        if (message is {SenderDeleted: true, RecipientDeleted: true})
            messageRepository.DeleteMessage(message);

        if (await messageRepository.SaveAllAsync()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}
