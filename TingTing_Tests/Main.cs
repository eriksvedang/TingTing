
using System;
using NUnit.Framework;
using GameTypes;
using TingTing;
using RelayLib;
using Pathfinding;
using TingTing.tests;

namespace TingTing_Tests
{
	public class MainClass
	{
		public static void Main(string[] args)
		{
            RoomTest t = new RoomTest();
            t.Setup();
            t.BasicUsage();
            t.TearDown();
		}
	}
}













