using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RelayLib;
using TingTing;
namespace TingTing_Tests
{
    [TestFixture]
    public class TingSpecialForcesTest
    {
        public class TerreTingThongDong : TingTing.Ting
        {
            ValueEntry<string> funkeyField;
            protected override void SetupCells()
            {
                funkeyField = EnsureCell<string>("funkeyField", "Something");
                base.SetupCells();
            }
            public string funk
            {
                get { return funkeyField.data; }
            }
        }

        public class SomeLesserTing : TingTing.Ting
        {
            protected override void SetupCells()
            {
                base.SetupCells();
            }
        }
        [Test]
        public void NonExistingFieldTest()
        {
            RelayLib.RelayTwo r2 = new RelayLib.RelayTwo();
            r2.CreateTable(TingTing.Ting.TABLE_NAME);
            r2.CreateTable(TingTing.Room.TABLE_NAME);
            RoomRunner rr =  new TingTing.RoomRunner(r2);
            rr.CreateRoom<Room>(WorldCoordinate.UNDEFINED_ROOM);
            TingTing.TingRunner tr = new TingTing.TingRunner(r2,rr);
            tr.CreateTing<SomeLesserTing>("TingA", TingTing.WorldCoordinate.NONE);
            r2.GetTable(TingTing.Ting.TABLE_NAME)[0].Set<string>(TingTing.Ting.CSHARP_CLASS_FIELD_NAME, "TerreTingThongDong");
            List<TingTing.Ting> list = InstantiatorTwo.Process<TingTing.Ting>(r2.GetTable(TingTing.Ting.TABLE_NAME));
            Console.WriteLine("list length" + list.Count);
            Assert.NotNull((list[0] as TerreTingThongDong));
            Assert.AreSame("Something", (list[0] as TerreTingThongDong).funk);
        }
    }
}
