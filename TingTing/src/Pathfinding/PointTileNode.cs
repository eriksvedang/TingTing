using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Pathfinding;
using GameTypes;

namespace TingTing
{
    public class PointTileNode : TileNode
    {
        public Room room { get; set; }

        private List<Ting> _occupants;
        private PointTileNode _target = null;

        public int group { get; set; }

        public PointTileNode(IntPoint pLocalPoint, Room r) : base(pLocalPoint)
        {
            room = r;
            group = -1;
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
     
        public override float DistanceTo(Pathfinding.IPoint pPoint)
        {
            if (pPoint is TileNode) {
                PointTileNode otherNode = pPoint as PointTileNode;
                return worldPoint.EuclidianDistanceTo(otherNode.worldPoint);
            }
            else {
                throw new NotImplementedException();
            }
        }
     
        public override long GetUniqueID()
        {
            return BitCruncher.PackTwoInts(position.roomName.GetHashCode(), position.localPosition.GetHashCode());
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
    }
}
