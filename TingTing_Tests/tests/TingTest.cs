using System;
using NUnit.Framework;
using TingTing;
using RelayLib;
using GameTypes;

namespace TingTing.tests
{
	class MyTing : Ting {
		
	}
	
	class MyTingShell {
		Ting _ting;
		public MyTingShell(Ting pTing)
		{
			_ting = pTing;
			_ting.AddDataListener<string>("name", OnNameChanged);
		}
		
		private void OnNameChanged(string pOldValue, string pNewValue) {
			_ting.logger.Log("Name changed from " + pOldValue + " to " + pNewValue);
		}
	}

	[TestFixture]
	public class TingTest
	{
		RelayTwo _relay;
		TableTwo _table;
        TingRunner _tingRunner;
		RoomRunner _roomRunner;
		const string ROOM_NAME = "Room";
		
		[SetUp]
		public void Setup() 
		{
			_relay = new RelayTwo();
			_table = _relay.CreateTable(Ting.TABLE_NAME);
            _relay.CreateTable(Room.TABLE_NAME);
            _roomRunner = new RoomRunner(_relay);
            Room room = _roomRunner.CreateRoom<Room>(ROOM_NAME);
            
			for (int i = 0; i < 100; i++)
            {
                room.AddTile(new PointTileNode(new IntPoint(i % 10, i / 10), room));
            }
            
            _tingRunner = new TingRunner(_relay, _roomRunner);
            _tingRunner.CreateTing<MyTing>("Ting0", new WorldCoordinate(ROOM_NAME, new IntPoint(0, 0)));
            _tingRunner.CreateTing<MyTing>("Ting1", new WorldCoordinate(ROOM_NAME, new IntPoint(1, 0)));
		}
		
        [Test]
        public void AddDuplicateTing()
        {
            Assert.Throws<TingDuplicateException>(() => { _tingRunner.CreateTing<MyTing>("Ting0", WorldCoordinate.NONE); });
        }
		
		[Test]
		public void SettingNameOfTing ()
		{
            Ting t = InstantiatorTwo.Create<MyTing>(_table);
            t.SetInitCreateValues("Monkey", WorldCoordinate.NONE, Direction.UP);
			t.CreateNewRelayEntry(_table, "MyTing");
			Assert.AreEqual("Monkey", t.name);
		}

		[Test]
		public void ShellListeningToValueChange() 
		{
			bool gotChangeMessage = false;
			MyTing t = new MyTing();
            t.SetInitCreateValues("Monkey", WorldCoordinate.NONE, Direction.UP);
			t.CreateNewRelayEntry(_table, "MyTing");
            ValueEntry<string>.DataChangeHandler handler = (before, after) => { gotChangeMessage = true; };
            t.AddDataListener<string>("prefab", handler);
			new MyTingShell(t);
            t.prefab = "some other prefab";
			Assert.IsTrue(gotChangeMessage);
            t.RemoveDataListener<string>("prefab", handler);
		}
		
        [Test]
        public void GetCurrentTile()
        {
            Ting t = _tingRunner.GetTing("Ting0");
            PointTileNode n = t.tile;
            Assert.NotNull(n);
            Assert.AreEqual(t.localPoint, n.localPoint);
        }

		[Test]
        public void TingOccupyingATile()
        {
			IntPoint p = new IntPoint(5, 3);
            Ting t = _tingRunner.GetTing("Ting0");
            t.position = new WorldCoordinate(ROOM_NAME, p);
			Room room = _roomRunner.GetRoom(ROOM_NAME);
			PointTileNode tileNode = room.GetTile(p);
			Ting[] occupants = tileNode.GetOccupants();
			Assert.AreEqual(1, occupants.Length);
			Assert.AreSame(t, occupants[0]);
        }
        
		[Test]
        public void TingChangesTileToOccupy()
        {
            Ting ting = _tingRunner.GetTing("Ting0");
			Room room = _roomRunner.GetRoom(ROOM_NAME);
			
			IntPoint p1 = new IntPoint(2, 2);
			IntPoint p2 = new IntPoint(4, 4);
			
			PointTileNode tileNode1 = room.GetTile(p1);
			PointTileNode tileNode2 = room.GetTile(p2);
			
			Assert.AreEqual(0, tileNode1.GetOccupants().Length);
			Assert.AreEqual(0, tileNode2.GetOccupants().Length);
			
			ting.position = new WorldCoordinate(ROOM_NAME, p1);
			
			Assert.AreEqual(1, tileNode1.GetOccupants().Length);
			Assert.AreEqual(0, tileNode2.GetOccupants().Length);
			
			ting.position = new WorldCoordinate(ROOM_NAME, p2);
			
			Assert.AreEqual(0, tileNode1.GetOccupants().Length);
			Assert.AreEqual(1, tileNode2.GetOccupants().Length);
        }
		
		class WeirdTing : Ting {}
		
		[Test]
        public void GetTingOfTypeOnTile()
        {
			MyTing myTing = _tingRunner.CreateTing<MyTing>("MyTing", new WorldCoordinate(ROOM_NAME, new IntPoint(0, 0)));
			Room room = _roomRunner.GetRoom(ROOM_NAME);
			IntPoint p = new IntPoint(3, 3);
			myTing.position = new WorldCoordinate(ROOM_NAME, p);
			PointTileNode tileNode = room.GetTile(p);
			Assert.AreEqual(null, tileNode.GetOccupantOfType<WeirdTing>());
			Assert.AreSame(myTing, tileNode.GetOccupantOfType<MyTing>());
		}
	}
}

