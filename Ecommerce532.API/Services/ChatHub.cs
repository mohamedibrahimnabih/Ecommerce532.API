using Ecommerce532.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace Ecommerce532.API.Services;

public class ChatHub : Hub
{
    //private readonly IRepository<Message> _repository;

    //public ChatHub(IRepository<Message> repository)
    //{
    //    _repository = repository;
    //}

    public async Task SendPrivateMessage(string receiverUserId, string user, string message)
    {
        await Clients.User(receiverUserId).SendAsync("ReceiveMessage", user, message);

        //await _repository.CreateAsync(new()
        //{
        //    FromUserId = ,
        //    Message = message,
        //    ToUserId = receiverUserId,
        //});

        //await _repository.CommitAsync();
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessage(string groupName, string user, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
    }
}
