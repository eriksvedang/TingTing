using System;
using NUnit.Framework;
using GameTypes;
using TingTing;
namespace TingTing_Tests
{
	[TestFixture]
	public class WorldCoordinateTests
	{
		[SetUp]
		public void Setup ()
		{
		}
		
		[Test]
		public void BasicUsage()
        {
            WorldCoordinate a = new WorldCoordinate("ladeda", new IntPoint(13, 37));
            WorldCoordinate b = new WorldCoordinate("ladeda", new IntPoint(13, 37));
            Assert.IsTrue(a == b);
            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());

            WorldCoordinate c = new WorldCoordinate("ladeda", new IntPoint(13, 37));
            WorldCoordinate d = new WorldCoordinate("ladeda", new IntPoint(13, 31));
            Assert.IsTrue(c != d);
            Assert.IsTrue(c.GetHashCode() != d.GetHashCode());

            WorldCoordinate e = new WorldCoordinate("ladeda", new IntPoint(13, 37));
            WorldCoordinate f = new WorldCoordinate("ladeba", new IntPoint(13, 37));
            Assert.IsTrue(e != f);
            Assert.IsTrue(e.GetHashCode() != f.GetHashCode());
			
		}
	}
}

