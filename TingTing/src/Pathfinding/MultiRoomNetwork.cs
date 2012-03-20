using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathfinding;
using GameTypes;

namespace TingTing
{
    class MultiRoomNetwork : IPathNetwork<PointTileNode>
    {

        PointTileNode[] nodes = null;

        public MultiRoomNetwork(IList<Room> pRooms)
        {
            List<PointTileNode> tNodes = new List<PointTileNode>();
            foreach (Room r in pRooms)
            {
                tNodes.AddRange(r._tilesByLocalPositionHash.Values);
            }
            nodes = tNodes.ToArray();
            
        }
        public void Reset()
        {
            foreach (PointTileNode t in nodes)
            {
                t.isGoalNode = false;
                t.isStartNode = false;
                t.distanceToGoal = 0f;
                t.pathCostHere = 0f;
                t.visited = false;
                t.linkLeadingHere = null;
            }
        }

    }
}
