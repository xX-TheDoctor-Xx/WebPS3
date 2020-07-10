using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebPS3
{
    public class AttachPS3Handler : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            // MAPI
            if (e.Data == "GET")
            {
                var ids = Globals.PS3_API.MAPI.Process.GetPidProcesses();

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SUCCESS");

                foreach (var id in ids)
                {
                    builder.AppendLine(id.ToString());
                }

                Send(builder.ToString());
            }
            else if (e.Data.StartsWith("ATTACH"))
            {
                // MAPI
                if (e.Data.Length > 6)
                {
                    if (uint.TryParse(e.Data.Substring(7), out uint id))
                    {
                        Globals.PS3_API.MAPIProcessID = id;
                        Globals.PS3_API.AttachProcess();
                    }
                    else
                    {
                        Send("FAILED " + (int)ErrorCode.INVALID_PROCESS_ID_FORMAT);
                    }
                }
                else
                {
                    if (Globals.PS3_API.AttachProcess())
                        Send("SUCCESS");
                    else
                        Send("FAILED " + (int)ErrorCode.ATTACH_FAILED);
                }
            }
        }
    }
}
