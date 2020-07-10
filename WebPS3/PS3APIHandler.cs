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
            if (e.Data == "API_GET")
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SUCCESS");

                string[] enum_names = Enum.GetNames(typeof(SelectAPI));

                for (var i = 0; i < enum_names.Length; i++)
                    builder.AppendLine(enum_names[i]);

                Send(builder.ToString());
            }
            else if (e.Data.StartsWith("API_SELECT"))
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
                    Send("FAILED " + (int)ErrorCode.INVALID_API);
                }
            }
            // CCAPI console list
            else if (e.Data == "CONNECT_LIST")
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
            // MAPI
            else if (e.Data == "ATTACH_GET")
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
            else if (e.Data == "DISCONNECT")
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
            else if (e.Data.StartsWith("MEMORY"))
            {
                string command = Encoding.UTF8.GetString(e.RawData, 0, 10);

                uint address = BitConverter.ToUInt32(e.RawData, 10);

                if (command == "MEMORY_SET")
                {
                    byte[] bytes = new byte[e.RawData.Length - 14];
                    Buffer.BlockCopy(e.RawData, 13, bytes, 0, bytes.Length);

                    Globals.PS3_API.SetMemory(address, bytes);

                    Send("SUCCESS");
                }
                else if (command == "MEMORY_GET")
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
            else
            {
                Send("FAILED " + (int)ErrorCode.INVALID_COMMAND);
            }
        }
    }
}
