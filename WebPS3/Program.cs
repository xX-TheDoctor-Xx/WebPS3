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

            wss.AddWebSocketService<PS3APIHandler>("/");

            wss.Start();
            Console.ReadKey(true);
            wss.Stop();
        }
    }
}
