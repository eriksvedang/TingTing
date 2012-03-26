using System;
using RelayLib;
using NUnit.Framework;
using System.Reflection;
using GameTypes;

namespace TingTing.tests
{
    [TestFixture()]
    public class TingRunnerTest
    {
        public class Animal : Ting
        {
            ValueEntry<string> CELL_species;
            ValueEntry<int> CELL_age;
         
            protected override void SetupCells()
            {
                base.SetupCells();
                CELL_species = EnsureCell("species", "unknown species");
                CELL_age = EnsureCell("age", 0);
            }
         
            public string species {
                get {
                    return CELL_species.data;
                }
                set {
                    CELL_species.data = value;
                }
            }
         
            public int age {
                get {
                    return CELL_age.data;
                }
                set {
                    CELL_age.data = value;
                }
            }
        }
     
        [Test()]
        public void InstantiateFromDatabase()
        {
            RelayTwo relay = new RelayTwo();
            TableTwo table = relay.CreateTable(Ting.TABLE_NAME);
            relay.CreateTable(Room.TABLE_NAME);
            table.AddField<string>(RelayObjectTwo.CSHARP_CLASS_FIELD_NAME);
            table.AddField<string>("species");
            table.AddField<string>("name");
            table.AddField<int>("age");
            int row0 = table.CreateRow().row;
            int row1 = table.CreateRow().row;


            table.SetValue(row0, RelayObjectTwo.CSHARP_CLASS_FIELD_NAME, "Animal");
            table.SetValue(row0, "species", "Monkey");
            table.SetValue(row0, "name", "Herr Nilsson");
            table.SetValue(row0, "age", 78);

            table.SetValue(row1, RelayObjectTwo.CSHARP_CLASS_FIELD_NAME, "Animal");
            table.SetValue(row1, "species", "Horse");
            table.SetValue(row1, "name", "Lilla Gubben");
            table.SetValue(row1, "age", 50);
         
            TingRunner tingRunner = new TingRunner(relay, new RoomRunner(relay));
         
            Animal herrNilsson = tingRunner.GetTing("Herr Nilsson") as Animal;
            Animal lillaGubben = tingRunner.GetTing("Lilla Gubben") as Animal;
         
            Assert.IsNotNull(herrNilsson);
            Assert.IsNotNull(lillaGubben);
            Assert.AreEqual(78, herrNilsson.age);
            Assert.AreEqual(50, lillaGubben.age);
        }
     
        [Test()]
        public void TryingToInstantiateNonexistingClass()
        {
            RelayTwo relay = new RelayTwo();
            TableTwo table = relay.CreateTable(Ting.TABLE_NAME);
            relay.CreateTable(Room.TABLE_NAME);
            table.AddField<string>(RelayObjectTwo.CSHARP_CLASS_FIELD_NAME);
            table.CreateRow().Set(RelayObjectTwo.CSHARP_CLASS_FIELD_NAME, "invalid class name");
            Assert.Throws<CantFindClassWithNameException>(() =>
            {
                new TingRunner(relay, new RoomRunner(relay));
            });
        }

        [Test()]
        public void TryingToGetTingWithWrongName()
        {
            RelayTwo relay = new RelayTwo();
            relay.CreateTable(Ting.TABLE_NAME);
            relay.CreateTable(Room.TABLE_NAME);
            TingRunner tingRunner = new TingRunner(relay, new RoomRunner(relay));
         
            Assert.Throws<CantFindTingException>(() =>
            {
                tingRunner.GetTing("wrong ting name");
            });
        }

        RelayTwo relay = null;

        TingRunner CreateTingRunnerWithSomeRoom()
        {
            relay = new RelayTwo();
            relay.CreateTable(Ting.TABLE_NAME);
            relay.CreateTable(Room.TABLE_NAME);
            RoomRunner rr = new RoomRunner(relay);
            rr.CreateRoom<Room>("SomeRoom");
            TingRunner tingRunner = new TingRunner(relay, rr);
            return tingRunner;
        }

        [Test()]
        public void CreateNewTingDuringRuntime()
        {
            TingRunner tingRunner = CreateTingRunnerWithSomeRoom();
            tingRunner.CreateTing<Animal>("Joe", new WorldCoordinate("SomeRoom", IntPoint.Zero));
         
            Animal a = tingRunner.GetTing("Joe") as Animal;
            Assert.IsNotNull(a);
        }
     
        [Test()]
        public void SetupTingsThenSaveAndLoadFromDisk()
        {
            {
                TingRunner tingRunner = CreateTingRunnerWithSomeRoom();

                Animal bo = tingRunner.CreateTing<Animal>("Bo", new WorldCoordinate("SomeRoom", IntPoint.Zero));
                bo.species = "cow";
                bo.age = 10;
                Animal howly = tingRunner.CreateTing<Animal>("Howly", new WorldCoordinate("SomeRoom", IntPoint.Zero));
                howly.species = "owl";
             
                Assert.AreEqual("cow", bo.species);
                Assert.AreEqual(10, bo.age);
                Assert.AreEqual("owl", howly.species);
                Assert.AreEqual(0, howly.age); // <- default value
             
                howly.age = 35;
             
                relay.SaveAll("farm.json");
            }
         
            {
                relay = new RelayTwo();
                relay.LoadAll("farm.json");
                TingRunner tingRunner = new TingRunner(relay, new RoomRunner(relay));
             
                Animal bo = tingRunner.GetTing("Bo") as Animal;
                Animal howly = tingRunner.GetTing("Howly") as Animal;
             
                Assert.AreEqual("cow", bo.species);
                Assert.AreEqual(10, bo.age);
                Assert.AreEqual("owl", howly.species);
                Assert.AreEqual(35, howly.age);
            }
        }
     
        [Test()]
        public void GetTingFromObjectId()
        {
            TingRunner tingRunner = CreateTingRunnerWithSomeRoom();
            Ting puma = tingRunner.CreateTing<Animal>("Puma", new WorldCoordinate("SomeRoom", IntPoint.Zero));
            Ting samePuma = tingRunner.GetTing(puma.name);

            Assert.AreSame(puma, samePuma);
        }
     
        [Test()]
        public void HasTing()
        {
            TingRunner tingRunner = CreateTingRunnerWithSomeRoom();
            tingRunner.CreateTing<Animal>("Puma", new WorldCoordinate("SomeRoom", IntPoint.Zero));
         
            Assert.IsTrue(tingRunner.HasTing("Puma"));
            Assert.IsFalse(tingRunner.HasTing("Donkey"));
        }
     
        [Test()]
        public void RemoveTingUsingObjectId()
        {
            TingRunner tingRunner = CreateTingRunnerWithSomeRoom();
            tingRunner.CreateTing<Animal>("Bee", new WorldCoordinate("SomeRoom", IntPoint.Zero));
            tingRunner.CreateTing<Animal>("Spider", new WorldCoordinate("SomeRoom", IntPoint.Zero));
            tingRunner.CreateTing<Animal>("Ant", new WorldCoordinate("SomeRoom", IntPoint.Zero));

            Assert.IsTrue(tingRunner.HasTing("Bee"));
            Assert.IsTrue(tingRunner.HasTing("Ant"));
        }
     
        [Test()]
        public void RemoveTingUsingName()
        {
            TingRunner tingRunner = CreateTingRunnerWithSomeRoom();
            tingRunner.CreateTing<Animal>("Bee", new WorldCoordinate("SomeRoom", IntPoint.Zero));
            tingRunner.CreateTing<Animal>("Spider", new WorldCoordinate("SomeRoom", IntPoint.Zero));
            tingRunner.CreateTing<Animal>("Ant", new WorldCoordinate("SomeRoom", IntPoint.Zero));
         
            tingRunner.RemoveTing("Bee");
         
            Assert.IsFalse(tingRunner.HasTing("Bee"));
            Assert.IsTrue(tingRunner.HasTing("Spider"));
            Assert.IsTrue(tingRunner.HasTing("Ant"));
        }
    }
}