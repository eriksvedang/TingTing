using System;
using TingTing;
using RelayLib;

namespace TingTingProfiling
{
	class MainClass
	{
		const int COUNTER = 1000000;
		
		public static void Main(string[] args)
		{
			if(args.Length == 0)
			{
				Console.WriteLine("Must send argument 1 or 2 to program");
				return;
			}
			
			if(args[0] == "1") {
				UsingCellId();
			}
			else if(args[0] == "2") {
				UsingNormalAccessor();
			}
		}
		
		private static void UsingCellId()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(Ting.TABLE_NAME);
			TingRunner tingRunner = new TingRunner(relay, new RoomRunner(relay));
			TestTing1UsingCellIdWithConvenienceFunctions t = tingRunner.CreateTing<TestTing1UsingCellIdWithConvenienceFunctions>("TestTing", WorldCoordinate.NONE);
			Console.WriteLine("Using class " + t.ToString());
			for(int i = 0; i < COUNTER; i++)
			{
				float a = t.awesome;
				a += 1.0f;
				t.awesome = a;
			}
			Console.WriteLine("1. awesome = " + t.awesome);
		}
		
		private static void UsingNormalAccessor()
		{
			RelayTwo relay = new RelayTwo();			
			relay.CreateTable(Ting.TABLE_NAME);
			TingRunner tingRunner = new TingRunner(relay, new RoomRunner(relay));
			TestTing2UsingNormalAccessor t = tingRunner.CreateTing<TestTing2UsingNormalAccessor>("TestTing", WorldCoordinate.NONE);
			Console.WriteLine("Using class " + t.ToString());
			for(int i = 0; i < COUNTER; i++)
			{
				float a = t.awesome;
				a += 1.0f;
				t.awesome = a;
			}
			Console.WriteLine("2. awesome = " + t.awesome);
		}
	}
	
	class TestTing1UsingCellIdWithConvenienceFunctions : Ting
	{
		ValueEntry<float> CELL_awesome;
	
		protected override void SetupCells()
		{
			base.SetupCells();
		    CELL_awesome = EnsureCell<float>("awesome", 0f);
		}
		
		public float awesome
		{
			get {
				return CELL_awesome.data;
			}
			set {
				
				CELL_awesome.data = value;
			}
		}
	}
	
	class TestTing2UsingNormalAccessor : Ting
	{
		private float _awesome;
		
		public float awesome
		{
			get {
				return _awesome;
			}
			set {
				_awesome = value;
			}
		}
	}
}
