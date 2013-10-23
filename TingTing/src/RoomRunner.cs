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
            // Unfortunately this seems like it generates a lot of garbage :(
            //Room r = _rooms.Find(o => o.name == pName);

            Room r = null;

            foreach (Room room in _rooms) {
                if (room.name == pName) {
                    r = room;
                    break;
                }
            }

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
                ResetRoom(room);
            }
        }
        
        public void ResetRoom(Room pRoom)
        {
            foreach (TileNode t in pRoom._tilesByLocalPositionHash.Values) {
                t.Reset();
            }
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

        public override string ToString()
        {
            return string.Format("RoomRunner ({0} rooms)", _rooms.Count);
        }
    }
}

