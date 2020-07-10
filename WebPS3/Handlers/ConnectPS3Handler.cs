using PS3Lib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebPS3
{
    public class ConnectPS3Handler : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            // CCAPI console list
            if (e.Data == "LIST")
            {
                var list = Globals.PS3_API.CCAPI.GetConsoleList();

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SUCCESS");

                for (var i = 0; i < list.Count; i++)
                    builder.AppendLine(list[i].Name + " " + list[i].Ip);

                Send(builder.ToString());
            }
            // TMAPI
            else if (e.Data.StartsWith("CONNECT_TARGET"))
            {
                if (int.TryParse(e.Data.Substring(15), out int target))
                {
                    Globals.PS3_API.ConnectTarget(target);
                    Send("SUCCESS");
                }
                else
                {
                    Send("FAILED " + (int)ErrorCode.INVALID_TARGET);
                }
            }
            else if (e.Data.StartsWith("CONNECT_IP"))
            {
                string ip = e.Data.Substring(11);
                if (Globals.PS3_API.ConnectTarget(ip))
                {
                    Send("SUCCESS");
                }
                else
                {
                    Send("FAILED " + (int)ErrorCode.CONNECTION_FAILED);
                }
            }
        }
    }
}
