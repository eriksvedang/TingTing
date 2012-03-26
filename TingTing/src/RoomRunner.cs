using System;
using System.Collections.Generic;
using System.Reflection;
using GameTypes;
using TingTing;
using RelayLib;
using Pathfinding;

namespace TingTing
{
    public class RoomRunner : IPathNetwork<PointTileNode>, IPreloadable
    {
        TableTwo _roomTable;
        List<Room> _rooms;
        PathSolver<PointTileNode> _pathSolver = new PathSolver<PointTileNode>();
     
        public RoomRunner(RelayTwo pRelay)
        {
            D.isNull(pRelay);
            _roomTable = pRelay.GetTable(Room.TABLE_NAME);
            _rooms = InstantiatorTwo.Process<Room>(_roomTable);
        }
     
        public T CreateRoom<T>(string pName) where T : Room
        {
#if DEBUG
            if(HasRoom(pName)) {
             throw new TingTingException("There is already a room called '" + pName + "' in Room Runner");
            }
#endif
            T newRoom = InstantiatorTwo.Create<T>(_roomTable);
            newRoom.name = pName;
            _rooms.Add(newRoom);
            return newRoom;
        }
     
        public Room GetRoom(string pName)
        {
            Room r = _rooms.Find(o => o.name == pName);
            if (r != null) {
                return r;
            }
            else {
                throw new TingTingException("Can't find room '" + pName + "' in Room runner");
            }
        }
     
        public bool HasRoom(string pName)
        {
            return (_rooms.Find(o => o.name == pName) != null);
        }
     
        public IEnumerable<Room> rooms { 
            get { 
                return _rooms; 
            }
        }
     
        public void DestroyRoom(string pName)
        {
            Room r = GetRoom(pName);
            int objectId = r.objectId;
            _roomTable.RemoveRowAt(objectId);
            _rooms.Remove(r);
        }
     
        public void Reset()
        {
            foreach (Room room in _rooms) {
                foreach (TileNode t in room._tilesByLocalPositionHash.Values) {
                    t.Reset();
                }
            }
        }
     
        public Path<PointTileNode> FindPath(WorldCoordinate pStart, WorldCoordinate pEnd)
        { 
            TileNode start = GetRoom(pStart.roomName).GetTile(pStart.localPosition);
            TileNode end = GetRoom(pEnd.roomName).GetTile(pEnd.localPosition);
            return _pathSolver.FindPath(start, end, this);
        }

        #region IPreloadable Members

        public IEnumerable<string> Preload()
        {
            foreach (Room r in _rooms) {
                yield return "Setting up links in room " + r.name;
                r.SetupLinks();
            }
        }

        #endregion
    }
}

