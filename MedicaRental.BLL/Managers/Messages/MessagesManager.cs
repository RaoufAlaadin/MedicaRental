﻿using MedicaRental.BLL.Dtos;
using MedicaRental.DAL.Models;
using MedicaRental.DAL.Repositories;
using MedicaRental.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MedicaRental.BLL.Managers;

public class MessagesManager : IMessagesManager
{
    private readonly IUnitOfWork _unitOfWork;

    public MessagesManager(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StatusDto> DeleteMessage(string userId, Guid messageId)
    {
        var succeeded = await _unitOfWork.Messages.DeleteOneById(messageId);

        if (succeeded)
        {
            _unitOfWork.Save();
            return new("Message deleted successfully", HttpStatusCode.NoContent);
        }

        return new("Message couldn't be deleted", HttpStatusCode.BadRequest);
    }

    public async Task<IEnumerable<MessageDto>> GetChat(string firstUserId, string secondUserId, DateTime dateOpened)
    {
        var succeeded = await UpdateSeenStatus(firstUserId, secondUserId, dateOpened);

        if (succeeded) _unitOfWork.Save();
        else throw new Exception("WTF");

        return await ((IMessagesRepo)_unitOfWork.Messages).GetChat<MessageDto>(firstUserId, secondUserId, m => new(m.Id, m.Content, m.SenderId, m.Timestamp, m.MesssageStatus));
    }

    public async Task<IEnumerable<ChatDto>> GetUserChats(string userId, int upTo)
    {
        return await ((IMessagesRepo)_unitOfWork.Messages).GetUserChats
            (
                userId,
                upTo,
                g =>
                    new ChatDto
                    (
                        g.First().ReceiverId == userId ? g.First().Sender!.User!.FirstName : g.First()!.Receiver!.User!.FirstName,
                        g.OrderByDescending(m => m.Timestamp).FirstOrDefault()!.Content,
                        g.OrderByDescending(m => m.Timestamp).FirstOrDefault()!.Timestamp,
                        g.OrderByDescending(m => m.Timestamp).FirstOrDefault()!.MesssageStatus,
                        g.Count(m => m.MesssageStatus != MessageStatus.Seen && m.ReceiverId == userId),
                        g.First().ReceiverId == userId ? g.First().Sender!.ProfileImage : g.First()!.Receiver!.ProfileImage
                    )
           );

    }

    public async Task<bool> UpdateSeenStatus(string firstUserId, string secondUserId, DateTime dateOpened)
    {
        var messages = await _unitOfWork.Messages.FindAllAsync
                            (
                                predicate: m => m.ReceiverId == firstUserId && m.SenderId == secondUserId,
                                disableTracking: false
                            );

        foreach (var msg in messages)
        {
            msg.MesssageStatus = MessageStatus.Seen;
        }

        return _unitOfWork.Messages.UpdateRange(messages);
    }
}
