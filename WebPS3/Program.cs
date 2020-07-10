using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebPS3
{
    class Program
    {
        static void Main(string[] args)
        {
            var wss = new WebSocketServer(6969);

            wss.AddWebSocketService<PS3APIHandler>("/api");
            wss.AddWebSocketService<ConnectPS3Handler>("/connect");
            wss.AddWebSocketService<AttachPS3Handler>("/attach");
            wss.AddWebSocketService<DisconnectPS3Handler>("/disconnect");
            wss.AddWebSocketService<MemoryPS3Handler>("/memory");

            wss.Start();
            Console.ReadKey(true);
            wss.Stop();
        }
    }
}
