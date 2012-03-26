using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameTypes;

namespace TingTing
{
    public class WorldCoordinateException : Exception
    {
        public WorldCoordinateException(string pMessage, Exception pInnerException) : base(pMessage, pInnerException)
        {
        }

        public WorldCoordinateException(string pMessage) : base(pMessage)
        {
        }
    }

    public struct WorldCoordinate
    {
        public static readonly WorldCoordinate NONE = new WorldCoordinate(UNDEFINED_ROOM, IntPoint.Zero);
        public const string UNDEFINED_ROOM = "undefined_room";
        
        public string roomName;
        public IntPoint localPosition;

        public WorldCoordinate(string pRoomName, IntPoint pLocalPosition)
        {
            localPosition = pLocalPosition;
            roomName = pRoomName;
        }

        public WorldCoordinate(string pRoomName, int pX, int pY)
        {
            localPosition = new IntPoint(pX, pY);
            roomName = pRoomName;
        }

        public override bool Equals(object obj)
        {
            if (obj is WorldCoordinate) {
                WorldCoordinate w = (WorldCoordinate)obj;
                if (w.localPosition == localPosition && w.roomName == roomName)
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return roomName.GetHashCode() ^ localPosition.GetHashCode();
        }

        public static bool operator  ==(WorldCoordinate a, WorldCoordinate b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(WorldCoordinate a, WorldCoordinate b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
            return "Room: " + roomName + ", pos: " + localPosition;
        }
    }
}
