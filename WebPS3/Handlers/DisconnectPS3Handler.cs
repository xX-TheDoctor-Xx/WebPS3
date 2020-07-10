using PS3Lib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebPS3
{
    public class DisconnectPS3Handler : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                Globals.PS3_API.DisconnectTarget();
            }
            finally
            {
                Send("SUCCESS");
            }
        }
    }
}
