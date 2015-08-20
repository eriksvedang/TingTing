using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathfinding;
using TingTing;

namespace Pathfinding.Datastructures
{
    public class AStarStack
    {
        Dictionary<long, PointTileNode> _nodes = new Dictionary<long, PointTileNode>();
        
        public void Push(PointTileNode pNode)
        {
            _nodes[pNode.GetUniqueID()] = pNode;
        }

        public PointTileNode Pop()
        {
            PointTileNode result = null;
            
            foreach (PointTileNode p in _nodes.Values) {
                if (result == null || p.CompareTo(result) == 1) {
                    result = p;    //p has a shorter distance than result
                }
            }
            
            if (result == null) {
                return null;
            }
            else {
                _nodes.Remove(result.GetUniqueID());
                return result;  
            }
        }

        public int Count {
            get {
                return _nodes.Values.Count;
            }
        }
    }
}
