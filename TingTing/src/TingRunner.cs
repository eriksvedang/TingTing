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
		protected Dictionary<string,  Ting> _tings = new  Dictionary<string, Ting>();
		GameTime _gameClock;
        protected RoomRunner _roomRunner = null;
        RelayTwo _relay = null;
        Dictionary<string, TableTwo> _loadedTingTables = new Dictionary<string, TableTwo>();
        public IEnumerable<string> loadedTingTables { get { return _loadedTingTables.Keys; } }
		
		public TingRunner (RelayTwo pRelay, RoomRunner pRoomRunner)
		{
			D.isNull(pRelay);
            D.isNull(pRoomRunner);
            _roomRunner = pRoomRunner;
            _relay = pRelay;

            Type[] classes = InstantiatorTwo.GetSubclasses(typeof(Ting));
            List<Ting> tingsToAdd = new List<Ting>();
            
            foreach(Type t in classes)
            {
                TableTwo table = AssertTable(t, pRelay);
                
                if (!_loadedTingTables.ContainsKey(table.name))
                {
                    //Console.WriteLine("##=> TableName " + table.name + " for type " + t.Name);
                    tingsToAdd.AddRange(InstantiatorTwo.Process<Ting>(table));
                    _loadedTingTables.Add(table.name, table);
                }
            }
			// _tings = InstantiatorTwo<Ting>.Process( _tingTable);
            
            foreach (Ting t in tingsToAdd)
            {
                AddTing(t);        
                t.SetupBaseRunners(this, _roomRunner);   
			}
            actionTime = 0;
		}
      
		private void AddTing(Ting t)
        {
            if (!_tings.ContainsKey(t.name))
            {
                _tings.Add(t.name, t);
            }
            else
            {
                throw new TingDuplicateException(" can't have two tings with the same name: " + t.name);
            }
        }
      
		private string GetTableName(Type t)
        {

            FieldInfo f = t.GetField("TABLE_NAME", BindingFlags.Public | BindingFlags.Static );
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
		
		public virtual T CreateTing<T>(string pName, WorldCoordinate pPosition, Direction pDirection) where T : Ting
		{
		    Type t = typeof(T);
            TableTwo table  = AssertTable(t, _relay);
            //Console.WriteLine("##=> create table " + table.name + " for type " + t.Name);
            if (!_loadedTingTables.ContainsKey(table.name))
                _loadedTingTables.Add(table.name, table);

			T newTing = Activator.CreateInstance(t) as T;
            newTing.SetupBaseRunners(this, _roomRunner);
            newTing.SetInitCreateValues(pName, pPosition, pDirection);
            newTing.CreateNewRelayEntry(table, t.Name);
            AddTing(newTing);
			return newTing;
		}
		
		// If you don't care about direction, use this one
		public T CreateTing<T>(string pName, WorldCoordinate pWorldCoordinate) where T : Ting
		{
			return CreateTing<T>(pName, pWorldCoordinate, Direction.RIGHT);
		}
		
		/// <returns>
		/// Returns the first Ting with the name (there might be several)
		/// </returns>
		public Ting GetTing(string pName) 
		{
			D.isNull(_tings);
            Ting result;
            if (_tings.TryGetValue(pName, out result))
            {
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
			foreach(Ting t in _tings.Values)
			{
				if(t.position.roomName == pRoomName)
				{
					tingsInRoom.Add(t);
				}
			}
			return tingsInRoom.ToArray();
		}
		
		public TingType[] GetTingsOfType<TingType>() where TingType : Ting
        {
            List<TingType> tingsOfType = new List<TingType>();
			foreach(Ting t in _tings.Values) {
				if(t is TingType) tingsOfType.Add(t as TingType);
			}
			return tingsOfType.ToArray();
        }
		
		public bool HasTing(string pName)
		{
            
			return _tings.ContainsKey(pName);
		}

		public void RemoveTing(string pName)
		{
			Ting tingToRemove = GetTing(pName);
            tingToRemove.table.RemoveRowAt(tingToRemove.objectId);
            _tings.Remove(pName);
		}
		
		public IEnumerable<Ting> GetTings()
		{
			return _tings.Values;
		}
		
		public void Update(float dt, GameTime pGameClock, float pActionTime)
		{
            gameClock = pGameClock;
            actionTime = pActionTime;
			foreach(Ting t in _tings.Values) {
				t.Update(dt);
                t.UpdateAction(pActionTime);
			}
		}

        public GameTime gameClock{  get;  private set;   }
        public float actionTime { get; private set; }
        
		public override string ToString()
		{
			return string.Format("TingRunner ({0} tings)", _tings.Count);
		}


    }
}

