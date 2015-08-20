using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Pathfinding;
using GameTypes;

namespace TingTing
{
    public class PointTileNode
    {
        public Room room { get; set; }

        private List<Ting> _occupants;
        private PointTileNode _target = null;
        public int group { get; set; }

        // moved down from TileNode
        public List<PathLink> links;
        public IntPoint localPoint;
        public float pathCostHere;
        public float distanceToGoal;
        public float targetValue;
        public bool isStartNode;
        public bool isGoalNode;
        public bool visited;
        public PathLink linkLeadingHere;
        public float baseCost;

        public void Reset()
        {
            distanceToGoal = 0f;
            isGoalNode = false;
            isStartNode = false;
            linkLeadingHere = null;
            pathCostHere = 0f;
            visited = false;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is PointTileNode)) return false;

            var other = obj as PointTileNode;

            return 
                    (room == other.room) &&
                    (group == other.group) &&
                    (_target == other._target) &&
                    (_occupants == other._occupants);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public PointTileNode(IntPoint pLocalPoint, Room r)
        {
            room = r;
            group = -1;
            links = new List<PathLink>(5);
            localPoint = pLocalPoint;
        }

        public override string ToString()
        {
            //return string.Format("[TileNode: localPosition={0}, worldPosition={1}, room={2}, isStartNode={3}, isGoalNode={4}, visited={5}]",
            //                   localPoint, worldPoint, room.name, isStartNode, isGoalNode, visited);

            return string.Format("[{2} ({0}, {1}, group {3})]", localPoint.x, localPoint.y, room.name, group);
        }

        public WorldCoordinate position {
            get { return new WorldCoordinate(room.name, localPoint); }
        }

        public IntPoint worldPoint {
            get { return room.worldPosition + localPoint; }
        }
     
        /// <summary>
        /// For doors, portals, etc
        /// </summary>
        public PointTileNode teleportTarget {
            set {
                if (value == this) {
                    throw new ArgumentException("Can't set target to self");
                }

                // Check for link duplicates
                if (links != null) {
                    for (int i = links.Count - 1; i >= 0; i--) {
                        PathLink l = links[i];
                        PointTileNode d = (PointTileNode)l.GetOtherNode(this);
                        if (d == value) {
                            return; //return if any duplicate was found.
                        }
                    }

                    // Remove any old links to other doors
                    for (int i = links.Count - 1; i >= 0; i--) {
                        PathLink l = links[i];
                        PointTileNode d = (PointTileNode)l.GetOtherNode(this);
                        if (d != null && d.room != this.room) {
                            RemoveLink(l);
                        }
                    }
                }
                if (value != null) {
                    // Check if the other node already has made a link for us to use
                    PathLink pl = value.GetLinkTo(this);
                    if (pl == null) {
                        pl = new PathLink(this, value);
                    }
                    //Console.WriteLine("added link between " + (pl.nodeA as TileNode).ToString() + "\nand " + (pl.nodeB as TileNode).ToString());
                    AddLink(pl);
                }
                _target = value;
            }
            get {
                return _target;
            }
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
        
        public PathLink GetLinkTo(PointTileNode pNode)
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
 
        public virtual float DistanceTo(PointTileNode pPoint)
        {
            if (pPoint is PointTileNode) {
                PointTileNode otherNode = pPoint;
                return localPoint.EuclidianDistanceTo(otherNode.localPoint);
            }
            else {
                throw new NotImplementedException();
            }
        }

        public int CompareTo(object obj)
        {
            PointTileNode target = obj as PointTileNode;
            targetValue = target.pathCostHere + target.distanceToGoal;
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

        public virtual long GetUniqueID()
        {
            return BitCruncher.PackTwoInts(localPoint.x, localPoint.y);
        }
     
        public void AddOccupant(Ting pTing)
        {
            EnsureOccapantList();
            _occupants.Add(pTing);
        }
     
        public void RemoveOccupant(Ting pTing)
        {
            //D.assert(HasOccupants(), "No occupants");
            if (_occupants != null && _occupants.Contains(pTing)) {
                _occupants.Remove(pTing);
            }
        }
     
        public bool HasOccupants()
        {
            return (_occupants != null) && (_occupants.Count > 0);
        }

        public bool HasOccupants(Ting pIgnoreThisTing)
        {
            if (_occupants == null) {
                return false;
            } else if (_occupants.Count == 0) {
                return false;
            } else if (_occupants.Count == 1) {
                return (_occupants[0] != pIgnoreThisTing);
            } else {
                //Console.WriteLine("Tile " + this.ToString() + " has " + _occupants.Count + " occupants:" + string.Join(", ", _occupants.Select(o => o.ToString()).ToArray()));
                return true;
            }
        }

        public bool HasOccupants<T>(Ting pIgnoreThisTing)
        {
            if (_occupants == null || _occupants.Count == 0) {
                return false;
            }
            
            foreach (var occupant in _occupants) {
                if(occupant == pIgnoreThisTing) {
                    continue;
                }
                else if(occupant.GetType() == typeof(T)) {
                    return true;
                }
            }
            
            return false;
        }

        public bool HasOccupantsButIgnoreSomeTypes(Type[] pTypesToIgnore)
        {
            if (_occupants == null || _occupants.Count == 0) {
                return false;
            }

            foreach (var occupant in _occupants) {
                if(!pTypesToIgnore.Contains(occupant.GetType())) {
                    return true;
                }
            }

            return false;
        }
     
        public Ting[] GetOccupants()
        {
            if (_occupants == null) {
                return new Ting[] {};
            }
            else { 
                return _occupants.ToArray();
            }
        }
     
        public T GetOccupantOfType<T>() where T : Ting
        {
            foreach (Ting t in GetOccupants()) {
                if (t.GetType() == typeof(T))
                    return t as T;
            }
            return null;
        }

        public IEnumerable<T> GetOccupantsOfType<T>() where T : Ting
        {
            foreach (Ting t in GetOccupants()) {
                if (t.GetType() == typeof(T))
                    yield return t as T;
            }
        }
     
        private void EnsureOccapantList()
        {
            if (_occupants == null)
                _occupants = new List<Ting>();
        }

        public string GetOccupantsAsString ()
        {
            return string.Join(", ", GetOccupants().Select(o => o.ToString()).ToArray());
        }
    }
}
