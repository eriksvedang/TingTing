using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameTypes;
namespace TingTing
{
	public class SimpleRoomBuilder 
	{	
		RoomRunner _roomRunner;
		
		public SimpleRoomBuilder(RoomRunner pRoomRunner) 
		{
			_roomRunner = pRoomRunner;
		}
				
		public Room CreateRoomWithSize(string pName, int pWidth, int pHeight)
		{
			Room newRoom = _roomRunner.CreateRoom<Room>(pName);
			
			for(int x = 0; x < pWidth; x++)
			{
				for(int y = 0; y < pHeight; y++)
				{
					newRoom.AddTile(new PointTileNode( new IntPoint(x,y), newRoom));
				}
			}
			
			return newRoom;
		}
	}
	
	
    /*
	class RoomBuilder
    {
        List<Room> _rooms = new List<Room>();
        public Room currentRoom = null;
        bool roomHasBeenMoved = false;
        public int yValue = 0;
		
        internal void AppendWallsAndDoors(string p)
        {
            for (int x = 0; x < p.Length; x++)
            {
                if (p[x] == '#')
                    currentRoom.AddTile(new PointTileNode(currentRoom, x,yValue, TileType.WALL));
                else if (p[x] == ' ')
                    currentRoom.AddTile(new PointTileNode(currentRoom, x, yValue, TileType.FLOOR));
                else if (Regex.IsMatch("" + p[x], "[0-9]"))
                {
                    DoorNode d = new DoorNode(currentRoom, "door" + p[x], new GameTypes.IntPoint(x, yValue));
                    currentRoom.AddDoor(d);
                    foreach (Room r in _rooms)
                    {
                        DoorNode otherDoor = r.GetDoor(d.name);    
                        if (otherDoor != null)
                        {
                            d.target = otherDoor; //door target is set here!
                            otherDoor.target = d;
                            if (!roomHasBeenMoved)
                            {
                
                                currentRoom.worldPosition = otherDoor.worldPosition - d.localPosition;
                                Console.WriteLine("moving room to " + currentRoom.worldPosition.ToString());
                                roomHasBeenMoved = true;
                            }
                        }
                    }
                
                }
            }
            yValue++;
        }

        internal void BeginNewRoom(string pRoomName)
        {
            if (currentRoom != null)
                _rooms.Add(currentRoom);
            currentRoom = new Room();
			currentRoom.name = pRoomName;
            roomHasBeenMoved = false;
            yValue = 0;
        }
        
        public Room[] GetRooms()
        {
            if (currentRoom != null)
                _rooms.Add(currentRoom);
            return _rooms.ToArray();
        }
		
        public void Clear()
        {
            _rooms.Clear();
            yValue = 0;
            currentRoom = null;
        }
    }
     * */
}

