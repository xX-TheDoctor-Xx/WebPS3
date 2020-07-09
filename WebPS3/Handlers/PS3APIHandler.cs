using PS3Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebPS3
{
    public class PS3APIHandler : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data == "GET")
            {
                StringBuilder builder = new StringBuilder();

                string[] enum_names = Enum.GetNames(typeof(SelectAPI));

                for (var i = 0; i < enum_names.Length; i++)
                    builder.AppendLine(enum_names[i]);

                Send(builder.ToString());
            }
            else if (e.Data.StartsWith("SELECT"))
            {
                string api = e.Data.Substring(6);

                SelectAPI apiEnum;

                if (Enum.TryParse(api, out apiEnum))
                {
                    Globals.PS3_API.CurrentAPI = apiEnum;
                    Send("SUCCESS");
                }
                else
                {
                    Send("FAILED" + (int)ErrorCode.InvalidAPI);
                }
            }
        }
    }
}
