using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws
{
    public enum FastCgiStatus
    {
        RequestComplete = 0,
        CantMpxConnection = 1,
        Overloaded = 2,
        UnknownRole = 3,
    }
}
