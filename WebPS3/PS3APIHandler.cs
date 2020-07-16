using PS3Lib;
using System;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebPS3
{
    public class PS3APIHandler : WebSocketBehavior
    {
        private object NotConnectedMethods(MessageEventArgs e)
        {
            if (e.Data == "API_GET")
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SUCCESS");

                string[] enum_names = Enum.GetNames(typeof(SelectAPI));

                for (var i = 0; i < enum_names.Length; i++)
                    builder.AppendLine(enum_names[i]);

                return builder.ToString();
            }
            else if (e.Data.StartsWith("API_SELECT"))
            {
                string api = e.Data.Substring(6);

                SelectAPI apiEnum;

                if (Enum.TryParse(api, out apiEnum))
                {
                    Globals.PS3_API.CurrentAPI = apiEnum;
                    return "SUCCESS";
                }
                else
                    return "FAILED" + (int)ErrorCode.INVALID_API;
            }
            // CCAPI console list
            else if (e.Data == "CONNECT_LIST")
            {
                var list = Globals.PS3_API.CCAPI.GetConsoleList();

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SUCCESS");

                for (var i = 0; i < list.Count; i++)
                    builder.AppendLine(list[i].Name + " " + list[i].Ip);

                return builder.ToString();
            }
            // TMAPI
            else if (e.Data.StartsWith("CONNECT_TARGET"))
            {
                if (int.TryParse(e.Data.Substring(15), out int target))
                {
                    Globals.PS3_API.ConnectTarget(target);
                    return "SUCCESS";
                }
                else
                    return "FAILED" + (int)ErrorCode.INVALID_TARGET;
            }
            else if (e.Data.StartsWith("CONNECT_IP"))
            {
                string ip = e.Data.Substring(11);
                return (Globals.PS3_API.ConnectTarget(ip)) ? "SUCCESS" : "FAILED" + (int)ErrorCode.CONNECTION_FAILED;
            }
            else
                return null;
        }

        private object ConnectedNotAttachedMethods(MessageEventArgs e)
        {
            // MAPI
            if (e.Data == "ATTACH_GET")
            {
                var ids = Globals.PS3_API.MAPI.Process.GetPidProcesses();

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("SUCCESS");

                foreach (var id in ids)
                    builder.AppendLine(id.ToString());

                return builder.ToString();
            }
            else if (e.Data.StartsWith("ATTACH"))
            {
                // MAPI
                if (e.Data.Length > 6)
                {
                    if (uint.TryParse(e.Data.Substring(7), out uint id))
                    {
                        Globals.PS3_API.MAPIProcessID = id;
                        return (Globals.PS3_API.AttachProcess()) ? "SUCCESS" : "FAILED" + (int)ErrorCode.ATTACH_FAILED;
                    }
                    else
                        return "FAILED" + (int)ErrorCode.INVALID_PROCESS_ID_FORMAT;
                }
                else
                    return (Globals.PS3_API.AttachProcess()) ? "SUCCESS" : "FAILED" + (int)ErrorCode.ATTACH_FAILED;
            }
            else
                return null;
        }

        private object ConnectedAttachedMethods(MessageEventArgs e)
        {
            if (e.Data == "DISCONNECT")
            {
                try
                {
                    Globals.PS3_API.DisconnectTarget();
                }
                finally
                {
                }

                return "SUCCESS";
            }
            else if (e.Data.StartsWith("SHUTDOWN"))
            {
                if (int.TryParse(e.Data.Substring(8), out int shutdownType))
                {
                    if (Globals.PS3_API.CurrentAPI == SelectAPI.ControlConsole)
                    {
                        // try catch if shutdownType doesnt exist
                        Globals.PS3_API.CCAPI.ShutDown((RebootFlags)shutdownType);
                    }
                    else if (Globals.PS3_API.CurrentAPI == SelectAPI.ManagerAPI)
                    {
                        // same here
                        Globals.PS3_API.MAPI.PS3.Power((PowerFlags)shutdownType);
                    }
                    else // TMAPI
                    {
                        // no need here
                        Globals.PS3_API.TMAPI.PowerOff(shutdownType != 0);
                    }

                    return "SUCCESS";
                }
                else
                    return "FAILED" + (int)ErrorCode.INVALID_SHUTDOWN_OPERATION;
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

                    return "SUCCESS";
                }
                else if (command == "MEMORY_GET")
                {
                    int length = BitConverter.ToInt32(e.RawData, 10);

                    var bytes = Globals.PS3_API.GetBytes(address, length);

                    return bytes;
                }
                else
                    return "FAILED " + (int)ErrorCode.INVALID_MEMORY_COMMAND;
            }
            else
                return null;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            bool sent = false;

            if (!Globals.PS3_API.IsConnected)
            {
                var output = NotConnectedMethods(e);
                if (output is string)
                {
                    Send(Encoding.UTF8.GetBytes((string)output));
                    sent = true;
                }
                else if (output is byte[])
                {
                    Send((byte[])output);
                    sent = true;
                }
            }

            if (Globals.PS3_API.IsConnected)
            {
                if (!Globals.PS3_API.IsAttached)
                {
                    var output = ConnectedNotAttachedMethods(e);
                    if (output is string)
                    {
                        Send(Encoding.UTF8.GetBytes((string)output));
                        sent = true;
                    }
                    else if (output is byte[])
                    {
                        Send((byte[])output);
                        sent = true;
                    }
                }
                else
                {
                    var output = ConnectedAttachedMethods(e);
                    if (output is string)
                    {
                        Send(Encoding.UTF8.GetBytes((string)output));
                        sent = true;
                    }
                    else if (output is byte[])
                    {
                        Send((byte[])output);
                        sent = true;
                    }
                }
            }

            if (!sent)
                Send("FAILED" + (int)ErrorCode.INVALID_COMMAND);
        }
    }
}
