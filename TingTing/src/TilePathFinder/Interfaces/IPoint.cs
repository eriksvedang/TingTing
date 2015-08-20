using System;
using System.Collections.Generic;

namespace Pathfinding
{
    public interface IPoint
    {
        float DistanceTo(IPoint pPoint);
    }
}
