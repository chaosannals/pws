using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws
{
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
}
