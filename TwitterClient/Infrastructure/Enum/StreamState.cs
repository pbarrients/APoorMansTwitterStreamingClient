using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterClient.Infrastructure.Enum
{
    /// <summary>
    /// Enumeration listing how the Stream is supposed to behave
    /// </summary>
    public enum StreamState
    {
        Stop = 0,
        Resume = 1,
        Pause = 2
    }
}
