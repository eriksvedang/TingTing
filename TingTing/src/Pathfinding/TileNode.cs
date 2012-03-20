using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Pathfinding;
using GameTypes;

using RelayLib;
namespace TingTing
{
    public class TileNode : IPathNode
    {


        public TileNode(Room pRoom, IntPoint pLocalPosition)
        {
            room = pRoom;
            localPosition = pLocalPosition;
        
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
		
		public override string ToString()
		{
			return string.Format("[TileNode: localPosition={0}, worldPosition={1}, room={2}, isStartNode={3}, isGoalNode={4}, visited={5}]",
			                     localPosition, worldPosition,  room.name, isStartNode, isGoalNode, visited);
		}
        
        #region IPathNode Members



        public IntPoint localPosition
        {
            set;
            get;
        }

		public IntPoint worldPosition
        {
            get { return room.worldPosition + localPosition; }
        }

        public Room room { get; set; }

        public float pathCostHere { get; set; }

        public float distanceToGoal { get; set; }

        public float baseCost { set; get; }

        public bool isStartNode { get; set; }

        public bool isGoalNode { get; set; }

        public bool visited { get; set; }

        public PathLink linkLeadingHere { get; set; }

        public PathLink[] links
        { get; set; }

        public void AddLink(PathLink pLink) 
        {
            List<PathLink> newLinks = links == null ? new List<PathLink>():new List<PathLink>(links);
            newLinks.Add(pLink);
            links = newLinks.ToArray();
        }
        public void RemoveLink(PathLink pLink)
        {
            List<PathLink> newLinks = links == null ? new List<PathLink>() : new List<PathLink>(links);
            newLinks.Remove(pLink);
            links = newLinks.ToArray();
        }
           
        public PathLink GetLinkTo(IPathNode pNode)
        {
            if (links != null)
            {
                foreach (PathLink p in links)
                {
                    if (p.Contains(pNode))
                        return p;
                }
            }
            return null;
        }
        TileNode _target = null;
        /// <summary>
        /// for use as door and teleport targets
        /// </summary>
        public TileNode teleportTarget
        {
            set
            {
                if (value == this)
                {
                    throw new ArgumentException("Can't set target to self");
                }

                //check for link duplicates
                if (links != null)
                {
                    for (int i = links.Length - 1; i >= 0; i--)
                    {
                        PathLink l = links[i];
                        TileNode d = (TileNode)l.GetOtherNode(this);
                        if (d == value)
                        {
                            return; //return if any duplicate was found.
                        }
                    }

                    //we should remove any old links to other doors
                    for (int i = links.Length - 1; i >= 0; i--)
                    {
                        PathLink l = links[i];
                        TileNode d = (TileNode)l.GetOtherNode(this);
                        if (d != null && d.room != this.room)
                        {
                            RemoveLink(l);
                        }
                    }
                }
                if (value != null)
                {
                    //check if the other node already has made a link for us to use.
                    PathLink pl = value.GetLinkTo(this);
                    if (pl == null)
                    {
                        pl = new PathLink(this, value);
                    }
                    Console.WriteLine("added link between " + (pl.nodeA as TileNode).ToString() + "\nand " + (pl.nodeB as TileNode).ToString());
                    AddLink(pl);
                }
                _target = value;
            }
            get
            {
                return _target;
            }
        }
        #endregion
        #region IPoint Members

        public float DistanceTo(Pathfinding.IPoint pPoint)
        {
            if (pPoint is TileNode)
            {
                TileNode otherNode = pPoint as TileNode;
                return this.worldPosition.DistanceTo(otherNode.worldPosition);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public override int GetHashCode()
        {
            return localPosition.GetHashCode();
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
            if (targetValue > thisValue)
                return 1;
            else if (targetValue == thisValue)
                return 0;
            else
                return -1;
        }

        #endregion
    }
}
