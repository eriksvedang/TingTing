using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Pathfinding;
using GameTypes;

namespace Pathfinding
{
    /*
    public class TileNode : IPathNode
    {
        public TileNode(IntPoint pLocalPoint)
        {
            links = new List<PathLink>(5);
            localPoint = pLocalPoint;
        }

        public void Reset()
        {
            distanceToGoal = 0f;
            isGoalNode = false;
            isStartNode = false;
            linkLeadingHere = null;
            pathCostHere = 0f;
            visited = false;
        }
        
        public IntPoint localPoint {
            set;
            get;
        }
        
        public float pathCostHere {
            get;
            set;
        }
        
        public float distanceToGoal {
            get;
            set;
        }
        
        public float baseCost {
            set;
            get;
        }
        
        public bool isStartNode {
            get;
            set;
        }
        
        public bool isGoalNode {
            get;
            set;
        }
        
        public bool visited {
            get;
            set;
        }
        
        public PathLink linkLeadingHere {
            get;
            set;
        }
        
        public List<PathLink> links {
            get;
            set;
        }
        
        public void AddLink(PathLink pLink)
        {
            links.Add(pLink);
        }

        public void RemoveLink(PathLink pLink)
        {
            links.Remove(pLink);
        }


		public void RemoveAllLinks ()
		{
			links.Clear ();
		}
        
        public PathLink GetLinkTo(IPathNode pNode)
        {
            if (links != null) {
                foreach (PathLink p in links) {
                    if (p.Contains(pNode)) {
                        return p;
                    }
                }
            }
            
            return null;
        }

		public bool isIsolated() {
			return links.Count == 0;
		}
        
        #region IPoint Members

        public virtual float DistanceTo(Pathfinding.IPoint pPoint)
        {
            if (pPoint is TileNode) {
                TileNode otherNode = pPoint as TileNode;
                return localPoint.EuclidianDistanceTo(otherNode.localPoint);
            }
            else {
                throw new NotImplementedException();
            }
        }
        
        #endregion

        #region IComparable Members
        
        public int CompareTo(object obj)
        {
#if DEBUG
            D.assert(obj is TileNode);
#endif
            TileNode target = obj as TileNode;
            float targetValue = target.pathCostHere + target.distanceToGoal;
            float thisValue = pathCostHere + distanceToGoal;
            
            if (targetValue > thisValue) {
                return 1;
            }
            else if (targetValue == thisValue) {
                return 0;
            }
            else {
                return -1;
            }
        }
        
        #endregion
        
        #region IPathNode Members

        public virtual long GetUniqueID()
        {
            return BitCruncher.PackTwoInts(localPoint.x, localPoint.y);
        }

        #endregion
    }
    */
}
