using System;
using System.Collections.Generic;
using TingTing;

namespace Pathfinding
{
    public class PathLink : IEnumerable<PointTileNode>
    {
        public float distance;
        public PointTileNode nodeA;
        public PointTileNode nodeB;

        public PathLink(PointTileNode pNodeA, PointTileNode pNodeB)
        {
            distance = pNodeA.DistanceTo(pNodeB);
            nodeA = pNodeA;
            nodeB = pNodeB;
        }

        public PointTileNode GetOtherNode(PointTileNode pSelf)
        {
            if (nodeA == pSelf) {
                return nodeB;
            }
            else if (nodeB == pSelf) {
                return nodeA;
            }
            else {
                throw new Exception("Function must be used with a parameter that's contained by the link");
            }
        }

        public int IndexOf(PointTileNode item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, PointTileNode item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public PointTileNode this[int index] {
            get {
                if (index == 0) {
                    return nodeA;
                }
                
                if (index == 1) {
                    return nodeB;
                }
                
                return null;
            }
            set {
                if (index == 0) {
                    nodeA = value;
                }
                
                if (index == 1) {
                    nodeB = value;
                }
            }
        }

        public void Add(PointTileNode item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            nodeA = null;
            nodeB = null;
        }

        public bool Contains(PointTileNode item)
        {
            if (nodeA == item || nodeB == item) {
                return true;
            }
            
            return false;
        }

        public int Count {
            get {
                return 2;
            }
        }

        public IEnumerator<PointTileNode> GetEnumerator()
        {
            yield return nodeA;
            yield return nodeB;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield return nodeA;
            yield return nodeB;
        }
    }
}
