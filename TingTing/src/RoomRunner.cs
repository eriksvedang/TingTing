using System;
using System.Collections.Generic;
using System.Reflection;
using GameTypes;
using TingTing;
using RelayLib;
using Pathfinding;

namespace TingTing
{
    public class RoomRunner : IPreloadable
    {
        TableTwo _roomTable;
        Dictionary<string, Room> _rooms = new Dictionary<string, Room>();
     
        public RoomRunner(RelayTwo pRelay)
        {
            D.isNull(pRelay);
            _roomTable = pRelay.GetTable(Room.TABLE_NAME);
            var rooms = InstantiatorTwo.Process<Room>(_roomTable);
            foreach(var room in rooms) {
                _rooms.Add(room.name, room);
            }
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
            _rooms.Add(pName, newRoom);
            return newRoom;
        }
     
        public Room GetRoom(string pName)
        {
            // Unfortunately this seems like it generates a lot of garbage :(
            //Room r = _rooms.Find(o => o.name == pName);

            Room r = null;

            _rooms.TryGetValue(pName, out r);

            //return r;

            if (r != null) {
                return r;
            }
            else {
                throw new TingTingException("Can't find room '" + pName + "' in Room runner");
            }
        }

        public Room GetRoomUnsafe(string pName) {
            Room r = null;
            _rooms.TryGetValue(pName, out r);
            return r;
        }
     
        public bool HasRoom(string pName)
        {
            return _rooms.ContainsKey(pName);
        }
     
        public IEnumerable<Room> rooms { 
            get { 
                return _rooms.Values; 
            }
        }
     
        public void DestroyRoom(string pName)
        {
            Room r = GetRoom(pName);
            int objectId = r.objectId;
            _roomTable.RemoveRowAt(objectId);
            _rooms.Remove(pName);
        }
     
        public void Reset()
        {
            foreach (Room room in _rooms.Values) {
                room.Reset();
            }
        }
     
        #region IPreloadable Members

        public IEnumerable<string> Preload()
        {
            foreach (Room r in _rooms.Values) {
                yield return "Setting up links and groups in room " + r.name;
                r.SetupLinks();
                r.SetupGroups();
            }
        }

        #endregion

        public override string ToString()
        {
            return string.Format("RoomRunner ({0} rooms)", _rooms.Count);
        }
    }
}

