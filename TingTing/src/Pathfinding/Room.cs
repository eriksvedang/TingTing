using System;
using RelayLib;
using GameTypes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathfinding;
using System.Diagnostics;

namespace TingTing
{
    public enum TileType
    { 
        NOT_SET,
        WALL,
        FLOOR,
        DOOR,
    }
 
    public class Room : RelayObjectTwo
    {
        public const string TABLE_NAME = "Rooms";

        public IntPoint worldPosition { set; get; }

        internal Dictionary<int, PointTileNode> _tilesByLocalPositionHash = new Dictionary<int, PointTileNode>();
        private ValueEntry<string> CELL_name = null;
        private ValueEntry<IntPoint[]> CELL_tiles = null;
        private ValueEntry<bool> CELL_exterior = null;

        protected override void SetupCells()
        {
            CELL_name = EnsureCell<string>("name", "undefined");
            CELL_tiles = EnsureCell<IntPoint[]>("tiles", new IntPoint[] { });
            IntPoint[] points = CELL_tiles.data;
            if (points.Length > 0) {
                SetTiles(points);
            }
            CELL_exterior = EnsureCell<bool>("exterior", false);
        }
     
        #region ACCESSORS

        public void SetupLinks()
        {
            foreach (PointTileNode ptn in _tilesByLocalPositionHash.Values) {
                AddTileLinks(ptn);
            }
            RefreshTileData();
        }

        public string name {
            get {
                return CELL_name.data;
            }
            set {
                CELL_name.data = value;
            }
        }

        public IntPoint[] tilePoints {
            get { return CELL_tiles.data; }
        }

        public bool exterior {
            get {
                return CELL_exterior.data;
            }
            set {
                CELL_exterior.data = value;
            }
        }

        #endregion

        public void SetTiles(IList<IntPoint> pPoints)
        {
            _tilesByLocalPositionHash.Clear();
            foreach (IntPoint t in pPoints) {
                PointTileNode newNode = new PointTileNode(t, this);
                _tilesByLocalPositionHash.Add(newNode.localPoint.GetHashCode(), newNode);
            }
            ApplyTileData();
            UpdateBounds();
        }

        /// <summary>
        /// Warning, too slow to run for each tile during load
        /// </summary>
        public void ApplyTileData()
        {
            CELL_tiles.data = (from PointTileNode n in _tilesByLocalPositionHash.Values select n.localPoint).ToArray();
        }

        private void AddTileLinks(PointTileNode tileNode)
        {
            PointTileNode start = tileNode;
            int x = start.localPoint.x;
            int y = start.localPoint.y;
            PointTileNode outputNode;

            if (_tilesByLocalPositionHash.TryGetValue(new IntPoint(x + 1, y).GetHashCode(), out outputNode)) {
                ConnectNodes(start, outputNode);
            }
            if (_tilesByLocalPositionHash.TryGetValue(new IntPoint(x - 1, y).GetHashCode(), out outputNode)) {
                ConnectNodes(start, outputNode);
            }
            if (_tilesByLocalPositionHash.TryGetValue(new IntPoint(x, y + 1).GetHashCode(), out outputNode)) {
                ConnectNodes(start, outputNode);
            }
            if (_tilesByLocalPositionHash.TryGetValue(new IntPoint(x, y - 1).GetHashCode(), out outputNode)) {
                ConnectNodes(start, outputNode);
            }
        }

        private void RefreshTileData()
        {
            CELL_tiles.data = (from PointTileNode n in _tilesByLocalPositionHash.Values select n.localPoint).ToArray();
        }

        public void AddTile(PointTileNode pTileNode)
        {
#if DEBUG
            //Console.WriteLine("Called Room.AddTile() Warning, this is slow"); //. Callstack: " + Environment.StackTrace);
#endif
            try {
                _tilesByLocalPositionHash.Add(pTileNode.localPoint.GetHashCode(), pTileNode);
            }
            catch (Exception e) {
                throw new Exception("Could not add tileNode at: " + pTileNode.localPoint.ToString() + " hashcode " + pTileNode.GetHashCode(), e);
            }
            AddTileLinks(pTileNode);
            RefreshTileData();
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            localMinBoundrary = IntPoint.Max;
            localMaxBoundrary = IntPoint.Min;

            foreach (PointTileNode t in _tilesByLocalPositionHash.Values) {
                if (t.localPoint.x > localMaxBoundrary.x)
                    localMaxBoundrary = new IntPoint(t.localPoint.x, localMaxBoundrary.y);
                if (t.localPoint.y > localMaxBoundrary.y)
                    localMaxBoundrary = new IntPoint(localMaxBoundrary.x, t.localPoint.y);
                if (t.localPoint.x < localMinBoundrary.x)
                    localMinBoundrary = new IntPoint(t.localPoint.x, localMinBoundrary.y);
                if (t.localPoint.y < localMinBoundrary.y)
                    localMinBoundrary = new IntPoint(localMinBoundrary.x, t.localPoint.y);
            }
        }

        public IntPoint localMinBoundrary { get; private set; }

        public IntPoint localMaxBoundrary { get; private set; }

        public IntPoint worldMinBoundrary { get { return worldPosition + localMinBoundrary; } }

        public IntPoint worldMaxBoundrary { get { return worldPosition + localMaxBoundrary; } }

        public PointTileNode GetTile(IntPoint pPoint)
        {
            return GetTile(pPoint.x, pPoint.y);
        }

        public PointTileNode GetTile(int x, int y)
        {
            // Console.WriteLine("x" + x + ", y" + y);
            PointTileNode t = null;
            _tilesByLocalPositionHash.TryGetValue(BitCruncher.PackTwoShorts(x, y), out t);
            return t;
        }

        public IntPoint WorldToLocalPoint(IntPoint pSource)
        {
            return pSource - worldPosition;
        }

        public IntPoint[] points {
            get { return CELL_tiles.data; }
        }

        private void ConnectNodes(PointTileNode pA, PointTileNode pB)
        {
            PathLink l = pB.GetLinkTo(pA);
            if (l == null) {
                l = new PathLink(pA, pB);
                l.distance = 1f;
                pA.AddLink(l);
                pB.AddLink(l);
            }
        }
    }
}

