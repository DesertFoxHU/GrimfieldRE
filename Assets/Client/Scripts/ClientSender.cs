using ClientSide;
using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ClientSender
{
    public static void SendBuildingFetchRequest(string rawGuid, MouseClickType clickType)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerPacket.FetchBuildingData);
        message.Add(rawGuid);
        message.Add(clickType.ToString());
        NetworkManager.Instance.Client.Send(message);
    }
}
