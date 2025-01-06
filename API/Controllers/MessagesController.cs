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
public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var userName = User.GetUserName();
        if (userName == createMessageDto.RecipientUserName.ToLower())
            return BadRequest("You cannot message yourself");

        var sender = await unitOfWork.UserRepository.GetUserByUserNameAsync(userName);
        var recipient = await unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

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

        unitOfWork.MessageRepository.AddMessage(message);
        if (await unitOfWork.Complete())
            return Ok(mapper.Map<MessageDto>(message));

        return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams) 
    {
        messageParams.UserName = User.GetUserName();

        var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
        Response.AddPaginationHeader(messages);
        return messages;
    }

    [HttpGet("thread/{userName}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string userName) 
    {
        var currentUserName = User.GetUserName();

        return Ok(await unitOfWork.MessageRepository.GetMessageThread(currentUserName, userName));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var userName = User.GetUserName();

        var message = await unitOfWork.MessageRepository.GetMessage(id);

        if (message == null) return BadRequest("Cannot delete this message");

        if (message.SenderUserName != userName && message.RecipientUserName != userName) return Forbid();

        if (message.SenderUserName == userName) message.SenderDeleted = true;
        if (message.RecipientUserName == userName) message.RecipientDeleted = true;

        if (message is {SenderDeleted: true, RecipientDeleted: true})
            unitOfWork.MessageRepository.DeleteMessage(message);

        if (await unitOfWork.Complete()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}
