using PS3Lib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebPS3
{
    public class MemoryPS3Handler : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            string command = Encoding.UTF8.GetString(e.RawData, 0, 10);

            uint address = BitConverter.ToUInt32(e.RawData, 10);

            if (command == "SET_MEMORY")
            {
                byte[] bytes = new byte[e.RawData.Length - 14];
                Buffer.BlockCopy(e.RawData, 13, bytes, 0, bytes.Length);

                Globals.PS3_API.SetMemory(address, bytes);

                Send("SUCCESS");
            }
            else if (command == "GET_BYTES")
            {
                int length = BitConverter.ToInt32(e.RawData, 10);

                var bytes = Globals.PS3_API.GetBytes(address, length);

                Send(bytes);
            }
            else
            {
                Send("FAILED " + (int)ErrorCode.INVALID_MEMORY_COMMAND);
            }
        }
    }
}
