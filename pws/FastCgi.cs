using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws
{
    /// <summary>
    /// 报文类型
    /// </summary>
    public enum FastCgiType
    {
        BeginRequest = 1,
        AbortRequest = 2,
        EndRequest = 3,
        Params = 4,
        StdIn = 5,
        StdOut = 6,
        StdErr = 7,
        Data = 8,
        GetValues = 9,
        GetValuesResult = 10,
        UnknownType = 11,
    }

    /// <summary>
    /// 请求标识
    /// </summary>
    public enum FastCgiFlag
    {
        KeepConnection = 1,//保持链接
    }

    /// <summary>
    /// 请求角色
    /// </summary>
    public enum FastCgiRole
    {
        Responder = 1,
        Authorizer = 2,
        Filter = 3,
    }

    /// <summary>
    /// 协议状态
    /// </summary>
    public enum FastCgiProtocolStatus
    {
        RequestComplete = 0,
        CantMpxConnection = 1,
        Overloaded = 2,
        UnknownRole = 3,
    }
}
