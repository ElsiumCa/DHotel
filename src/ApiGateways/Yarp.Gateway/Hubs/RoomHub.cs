

using Microsoft.AspNetCore.SignalR;

namespace Yarp.Gateway.Hubs;

public class RoomHub : Hub
{

    public async Task JoinRoomGroup(string RoomNumber)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, RoomNumber);
    }
}