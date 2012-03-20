using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TingTing
{
    public interface IPreloadable
    {
        IEnumerable<string> Preload();
    }
}
