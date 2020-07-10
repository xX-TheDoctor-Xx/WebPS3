using System;
using System.Collections.Generic;
using System.Text;

namespace WebPS3
{
    public enum ErrorCode
    {
        INVALID_API,
        INVALID_TARGET,
        CONNECTION_FAILED,
        CANT_GET_PROCESS_LIST,
        INVALID_PROCESS_ID_FORMAT,
        ATTACH_FAILED,
        INVALID_MEMORY_COMMAND,
        INVALID_COMMAND,
    }
}
