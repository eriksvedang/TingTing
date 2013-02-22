using System;
using System.Collections.Generic;
using System.Reflection;
using RelayLib;
using GameTypes;
using TingTing;

namespace TingTing
{
    public class TingRunner
    {
        public GameTime gameClock { get; private set; }
        public float actionTime { get; private set; }

        protected Dictionary<string,  Ting> _tings = new  Dictionary<string, Ting>();
        protected RoomRunner _roomRunner = null;

        private RelayTwo _relay = null;
        private Dictionary<string, TableTwo> _loadedTingTables = new Dictionary<string, TableTwo>();

        private List<Ting> _tingsToAddAfterUpdate = new List<Ting>();
        private List<string> _tingsToRemoveAfterUpdate = new List<string>();

        public TingRunner(RelayTwo pRelay, RoomRunner pRoomRunner)
        {
            D.isNull(pRelay);
            D.isNull(pRoomRunner);

            _roomRunner = pRoomRunner;
            _relay = pRelay;

            Type[] classes = InstantiatorTwo.GetSubclasses(typeof(Ting));
            List<Ting> tingsToAdd = new List<Ting>();

            foreach (Type t in classes) {
                TableTwo table = AssertTable(t, pRelay);

                if (!_loadedTingTables.ContainsKey(table.name)) {
                    //Console.WriteLine("Adding table " + table.name + " for type " + t.Name);
                    tingsToAdd.AddRange(InstantiatorTwo.Process<Ting>(table));
                    _loadedTingTables.Add(table.name, table);
                }
            }

            foreach (Ting t in tingsToAdd) {
                AddTing(t);
                t.SetupBaseRunners(this, _roomRunner);
            }

            actionTime = 0;
        }
      
        private void AddTing(Ting t)
        {
            if (!_tings.ContainsKey(t.name)) {
                _tings.Add(t.name, t);
            }
            else {
                throw new TingDuplicateException(" can't have two tings with the same name: " + t.name);
            }
        }
      
        private string GetTableName(Type t)
        {

            FieldInfo f = t.GetField("TABLE_NAME", BindingFlags.Public | BindingFlags.Static);
            if (f == null)
                return Ting.TABLE_NAME;
            else
                return (string)f.GetValue(null);
        }
       
        private TableTwo AssertTable(Type t, RelayTwo pRelay)
        {
            string tableName = GetTableName(t);
            if (!pRelay.tables.ContainsKey(tableName))
                pRelay.CreateTable(tableName);
            return pRelay.tables[tableName];
        }

        protected T CreateTingWithoutAddingItToList<T>(string pName, WorldCoordinate pPosition, Direction pDirection) where T : Ting
        {
            Type t = typeof(T);
            TableTwo table = AssertTable(t, _relay);
            //Console.WriteLine("##=> create table " + table.name + " for type " + t.Name);
            if (!_loadedTingTables.ContainsKey(table.name))
                _loadedTingTables.Add(table.name, table);

            T newTing = Activator.CreateInstance(t) as T;
            newTing.SetupBaseRunners(this, _roomRunner);
            newTing.SetInitCreateValues(pName, pPosition, pDirection);
            newTing.CreateNewRelayEntry(table, t.Name);
            return newTing;
        }
     
        public virtual T CreateTing<T>(string pName, WorldCoordinate pPosition, Direction pDirection) where T : Ting
        {
            T newTing = CreateTingWithoutAddingItToList<T>(pName, pPosition, pDirection);
            AddTing(newTing);
            return newTing;
        }
     
        // If you don't care about direction, use this one
        public T CreateTing<T>(string pName, WorldCoordinate pWorldCoordinate) where T : Ting
        {
            return CreateTing<T>(pName, pWorldCoordinate, Direction.RIGHT);
        }

        public virtual T CreateTingAfterUpdate<T>(string pName, WorldCoordinate pWorldCoordinate, Direction pDirection) where T : Ting
        {
            T newTing = CreateTingWithoutAddingItToList<T>(pName, pWorldCoordinate, pDirection);
            _tingsToAddAfterUpdate.Add(newTing);
            return newTing;
        }
     
        /// <returns>
        /// Returns the first Ting with the name (there might be several)
        /// </returns>
        public Ting GetTing(string pName)
        {
            D.isNull(_tings);
            Ting result;
            if (_tings.TryGetValue(pName, out result)) {
                return result;
            }
            else {
                throw new CantFindTingException("Can't find Ting with name " + pName + " in TingRunner");
            }
        }
     
        /// <returns>
        /// Returns the first Ting with the name (there might be several)
        /// </returns>
        public TingType GetTing<TingType>(string pName) where TingType : Ting
        {
            return GetTing(pName) as TingType;
        }

        public Ting GetTingUnsafe(string pName)
        {
            Ting result;
            _tings.TryGetValue(pName, out result);
            return result;
        }
     
        public Ting[] GetTingsInRoom(string pRoomName)
        {
            List<Ting> tingsInRoom = new List<Ting>();
            foreach (Ting t in _tings.Values) {
                if (t.position.roomName == pRoomName) {
                    tingsInRoom.Add(t);
                }
            }
            return tingsInRoom.ToArray();
        }
     
        public TingType[] GetTingsOfType<TingType>() where TingType : Ting
        {
            List<TingType> tingsOfType = new List<TingType>();
            foreach (Ting t in _tings.Values) {
                if (t is TingType)
                    tingsOfType.Add(t as TingType);
            }
            return tingsOfType.ToArray();
        }

        public TingType[] GetTingsOfTypeInRoom<TingType>(string pRoomName) where TingType : Ting
        {
            List<TingType> tingsInRoomOfType = new List<TingType>();
            foreach (Ting t in _tings.Values) {
                if (t is TingType && t.position.roomName == pRoomName) {
                    tingsInRoomOfType.Add(t as TingType);
                }
            }
            return tingsInRoomOfType.ToArray();
        }
     
        public bool HasTing(string pName)
        {
            
            return _tings.ContainsKey(pName);
        }

        public void RemoveTing(string pName)
        {
            Ting tingToRemove = GetTing(pName);
            tingToRemove.table.RemoveRowAt(tingToRemove.objectId);
            tingToRemove.isDeleted = true;
            _tings.Remove(pName);
        }

        public void RemoveTingAfterUpdate(string pName)
        {
            _tingsToRemoveAfterUpdate.Add(pName);
        }
     
        public IEnumerable<Ting> GetTings()
        {
            return _tings.Values;
        }
     
        public void Update(float dt, GameTime pGameClock, float pActionTime)
        {
            gameClock = pGameClock;
            actionTime = pActionTime;
            foreach (Ting t in _tings.Values) {
                t.Update(dt);
                t.UpdateAction(pActionTime);
            }
            foreach(Ting t in _tingsToAddAfterUpdate) {
                AddTing(t);
            }
            _tingsToAddAfterUpdate.Clear();
            foreach(string name in _tingsToRemoveAfterUpdate) {
                RemoveTing(name);
            }
            _tingsToRemoveAfterUpdate.Clear();
        }

        public override string ToString()
        {
            return string.Format("TingRunner ({0} tings)", _tings.Count);
        }

        public IEnumerable<string> loadedTingTables {
            get {
                return _loadedTingTables.Keys;
            }
        }
    }
}

