using System;
using NUnit.Framework;
using GameTypes;
using TingTing;
using RelayLib;
using Pathfinding;

namespace TingTing.tests
{
	[TestFixture()]
	public class RoomTest
	{
        [SetUp]
        public void Setup()
        {
            D.onDLog += new D.LogHandler(Console.WriteLine);
        }
		
        [TearDown]
        public void TearDown()
        {
            D.onDLog -= new D.LogHandler(Console.WriteLine);
        }
		
		[Test()]
		public void BasicUsage()
		{
			{
				RelayTwo relay = new RelayTwo();
				relay.CreateTable(Room.TABLE_NAME);
				RoomRunner roomRunner = new RoomRunner(relay);
                foreach (string s in roomRunner.Preload()) ;

				Room r1 = roomRunner.CreateRoom<Room>("r1");

                PointTileNode door1 = null;
				
				for(int i = 0; i < 100; i++)
				{
					int x = i % 10;
					int y = i / 10;
					
					if(x == 9 && y == 9) {
                        door1 = new PointTileNode(new IntPoint(9, 9), r1);
                        r1.AddTile(door1);
					}
					else {
                        r1.AddTile(new PointTileNode(new IntPoint(x, y), r1 ));
					}
				}

				r1.worldPosition = new IntPoint(50, 0);

				relay.SaveAll("room_test.json");
			}
			
			{
                RelayTwo relay = new RelayTwo("room_test.json");
				RoomRunner roomRunner = new RoomRunner(relay);
                foreach (string s in roomRunner.Preload()) ;
				Room r1 = roomRunner.GetRoom("r1");

				PointTileNode start = r1.GetTile(new IntPoint(0, 5));
				PointTileNode goal = r1.GetTile(new IntPoint(9, 5));
				
				D.isNull(start);
				D.isNull(goal);
				
				PathSolver pathSolver = new PathSolver();
				Path path = pathSolver.FindPath(start, goal, roomRunner, true);
                
				Console.WriteLine("path resolved using " + path.pathSearchTestCount + " node tests");
                Console.WriteLine("path tile count " + path.nodes.Length);

				Assert.AreEqual(PathStatus.FOUND_GOAL, path.status);
			}
		}
		
		[Test()]
		public void CreatingEmptyRoom()
		{
			string saveName = "MyEmptyRoomSave.json";
			{
				RelayTwo relay = new RelayTwo();
				relay.CreateTable(Room.TABLE_NAME);
				RoomRunner roomRunner = new RoomRunner(relay);
				roomRunner.CreateRoom<Room>("MyRoom");
				relay.SaveAll(saveName);
			}
			{
				RelayTwo relay = new RelayTwo(saveName);
				RoomRunner roomRunner = new RoomRunner(relay);
				Room room = roomRunner.GetRoom("MyRoom");
				Assert.IsNotNull(room);
				Assert.AreEqual("MyRoom", room.name);
				Assert.AreEqual(new PointTileNode[] {}, room.tiles);
			}
		}
		
#if DEBUG
		[Test()]
#endif
		public void CreateRoomsWithDuplicateNames()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(Room.TABLE_NAME);
			RoomRunner roomRunner = new RoomRunner(relay);
			roomRunner.CreateRoom<Room>("Hallway");
			Assert.Throws<TingTingException>(() => {
				roomRunner.CreateRoom<Room>("Hallway");
			});
		}
		
		[Test()]
		public void DeleteRoomAndCreateNewOneAfterThat()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(Room.TABLE_NAME);
			RoomRunner roomRunner = new RoomRunner(relay);
			
			{
				Room bathroom = roomRunner.CreateRoom<Room>("Bathroom");
				bathroom.AddTile(new PointTileNode(new IntPoint(7, 9), bathroom));
                bathroom.AddTile(new PointTileNode( new IntPoint(8, 9), bathroom));
				Assert.AreEqual(2, bathroom.tiles.Length);
				Assert.AreEqual(new PointTileNode(new IntPoint(7, 9), bathroom), bathroom.tiles[0]);
			}
			
			{
				roomRunner.DestroyRoom("Bathroom");
				Room bathroomAgain = roomRunner.CreateRoom<Room>("Bathroom");
				Assert.AreEqual(0, bathroomAgain.tiles.Length);
				bathroomAgain.AddTile(new PointTileNode(new IntPoint(7, 9),bathroomAgain)); // should be able to add node
				Assert.AreEqual(1, bathroomAgain.tiles.Length);
			}
		}
		
		[Test()]
		public void CreatingRoomConveniently()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(Room.TABLE_NAME);
			RoomRunner roomRunner = new RoomRunner(relay);
			
			SimpleRoomBuilder srb = new SimpleRoomBuilder(roomRunner);
			Room closet = srb.CreateRoomWithSize("Closet", 4, 3);
			Assert.AreEqual(12, closet.tiles.Length);
			
			for(int x = 0; x < 4; x++)
			{
				for(int y = 0; y < 3; y++)
				{
					Assert.IsNotNull(closet.GetTile(new IntPoint(x, y)));
				}
			}
		}
	}
}
















