//#define LOG

using System;
using RelayLib;
using GameTypes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathfinding;
using System.Diagnostics;
using System.Text;

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

        internal Dictionary<int, PointTileNode> _tilesByLocalPositionHash = new Dictionary<int, PointTileNode>();
        private ValueEntry<string> CELL_name = null;
        private ValueEntry<IntPoint[]> CELL_tiles = null;
        private ValueEntry<bool> CELL_exterior = null;
        private ValueEntry<IntPoint> CELL_worldPosition = null;

        // Optimization
        private ValueEntry<string> CELL_optiGrid; // a string of 0:s and 1:s that makes up a grid of where the IntPoints should be created
        private ValueEntry<IntPoint> CELL_optiGridSize; // the width and height of the grid
        private ValueEntry<IntPoint> CELL_optiGridOffset; // where the top left corner of the grid starts

        protected override void SetupCells()
        {
            CELL_name = EnsureCell<string>("name", "undefined");
            CELL_exterior = EnsureCell<bool>("exterior", false);
            CELL_worldPosition = EnsureCell<IntPoint>("worldPosition", IntPoint.Zero);

            CELL_optiGrid = EnsureCell<string>("optiGrid", "");
            CELL_optiGridSize = EnsureCell<IntPoint>("optiGridSize", new IntPoint(0, 0));
            CELL_optiGridOffset = EnsureCell<IntPoint>("optiGridOffset", new IntPoint(0, 0));

            CELL_tiles = EnsureCell<IntPoint[]>("tiles", new IntPoint[] { });

            if (optiGrid == "") {
                // no opti grid for this room yet
                IntPoint[] points = CELL_tiles.data;
                if (points.Length > 0) {
                    SetTiles(points);
                }
            } else {
                //Console.WriteLine("Room " + name + " has an optimized grid. Will load it now!");
                LoadTilesFromOptigrid();
            }
        }
     
        #region ACCESSORS

        public void SetupLinks()
        {
            foreach (PointTileNode ptn in _tilesByLocalPositionHash.Values) {
                AddTileLinks(ptn);
            }
                
            RefreshTileData();
        }

        public void SetupGroups()
        {
            var tiles = _tilesByLocalPositionHash.Values;
            int groupId = 0;

            foreach(var tile in tiles) {
                if(tile.group != -1) {
                    continue;
                }

                tile.group = groupId;

                var neighbours = GetNeighbours(tile);
                while(neighbours.Count > 0) {
                    var newNeighbours = new List<PointTileNode>();
                    foreach(var node in neighbours) {
                        if(node.group != -1) continue;
                        //Console.WriteLine("Setting " + node + " to group " + groupId);
                        node.group = groupId;
                        newNeighbours.AddRange(GetNeighbours(node));
                    }
                    neighbours = newNeighbours;
                }

                groupId++;
            }
        }

        List<PointTileNode> GetNeighbours(PointTileNode pNode) {
            var neighbours = new List<PointTileNode>();
            foreach(var link in pNode.links) {
                var newNode = link.GetOtherNode(pNode) as PointTileNode;
                if(newNode.group == -1) {
                   neighbours.Add(newNode);
                }
            }
            return neighbours;
        }

        void RecursiveGroupSet(PointTileNode pNode, int pGroupId) {
            if(pNode.group != -1) {
                return;
            }
            pNode.group = pGroupId;
            foreach(var link in pNode.links) {
                RecursiveGroupSet(link.GetOtherNode(pNode) as PointTileNode, pGroupId);
            }
        }

        public string name {
            get {
                return CELL_name.data;
            }
            set {
                CELL_name.data = value;
            }
        }

        public bool exterior {
            get {
                return CELL_exterior.data;
            }
            set {
                CELL_exterior.data = value;
            }
        }

        public string optiGrid {
            get {
                return CELL_optiGrid.data;
            }
            set {
                CELL_optiGrid.data = value;
            }
        }

        public IntPoint optiGridSize {
            get {
                return CELL_optiGridSize.data;
            }
            set {
                CELL_optiGridSize.data = value;
            }
        }

        public IntPoint optiGridOffset {
            get {
                return CELL_optiGridOffset.data;
            }
            set {
                CELL_optiGridOffset.data = value;
            }
        }

        #endregion

        public void Reset()
        {
            foreach (TileNode t in _tilesByLocalPositionHash.Values) {
                t.Reset();
            }
        }

        public void SetTiles(IList<IntPoint> pPoints)
        {
            #if DEBUG && LOG
            Console.WriteLine("Called Room.SetTiles() WARNING, this is really slow. Callstack: " + Environment.StackTrace);
            #endif

            _tilesByLocalPositionHash.Clear();
            foreach (IntPoint t in pPoints) {
                PointTileNode newNode = new PointTileNode(t, this);
                _tilesByLocalPositionHash[newNode.localPoint.GetHashCode()] = newNode; // used to be Add() so that duplicate keys would throw an exception, now it just overwrites
            }
            ApplyTileData();
            UpdateBounds();
        }

        public void SetOptigrid(IList<IntPoint> pPoints) {

            // INVALIDATE CACHE?!
            _tilePointsOptigridCache = null;

            Console.WriteLine("Will set optigrid for room " + name);

            if (pPoints.Count == 0) {
                optiGrid = "";
                optiGridSize = new IntPoint(0, 0);
                optiGridOffset = new IntPoint(0, 0);
                Console.WriteLine("No tiles in room " + name + ", will return");
                return;
            }

            int lowestX = int.MaxValue;
            int lowestY = int.MaxValue;
            int highestX = int.MinValue;
            int highestY = int.MinValue;

            foreach (var p in pPoints) {
                if (p.x < lowestX) {
                    lowestX = p.x;
                }
                if (p.y < lowestY) {
                    lowestY = p.y;
                }
                if (p.x > highestX) {
                    highestX = p.x;
                }
                if (p.y > highestY) {
                    highestY = p.y;
                }
            }

            IntPoint upperLeft = new IntPoint(lowestX, lowestY);
            IntPoint lowerRight = new IntPoint(highestX, highestY);

            //Console.WriteLine(name + " upper left: " + upperLeft + ", lower right: " + lowerRight);

            optiGridOffset = upperLeft;
            optiGridSize = (lowerRight - upperLeft) + new IntPoint(1, 1);
            int totalPointsInGrid = optiGridSize.x * optiGridSize.y;
            //Console.WriteLine("Size: " + optiGridSize + " total points in grid: " + totalPointsInGrid);

            char[] positions = new char[totalPointsInGrid];

            for (int i = 0; i < positions.Length; i++) {
                positions[i] = '0';
            }

            int gridWidth = optiGridSize.x;              
            foreach (var p in pPoints) {
                int normalizedX = p.x - optiGridOffset.x;
                int normalizedY = p.y - optiGridOffset.y;
                int index = normalizedY * gridWidth + normalizedX;
                //Console.WriteLine("Placing tile at index " + index + " (based on point " + p + ")");
                positions[index] = '1';
            }

            optiGrid = new string(positions);
            //Console.WriteLine("Final opti grid: " + optiGrid);

            // Remove any tiles that are saved the old (SLOW) way
            CELL_tiles.data = new IntPoint[] { };

            LoadTilesFromOptigrid();
        }

        void LoadTilesFromOptigrid()
        {
            _tilesByLocalPositionHash.Clear();

            string gridString = optiGrid;
            int gridWidth = optiGridSize.x;
            int gridHeight = optiGridSize.y;

            /*
            int offsetX = optiGridOffset.x;
            int offsetY = optiGridOffset.y;

            int index = 0;
            foreach (char c in gridString) {
                if (c == '1') {
                    int x = (index % gridWidth);
                    int y = (index / gridWidth);
                    int xx = x + offsetX;
                    int yy = y + offsetY;
                    PointTileNode newNode = new PointTileNode(new IntPoint(xx, yy), this);
                    _tilesByLocalPositionHash[newNode.localPoint.GetHashCode()] = newNode;
                }
                index++;
            }
            if (index != gridWidth * gridHeight) {
                throw new Exception("Index doesn't match size of grid");
            }*/

            for (int y = 0; y < gridHeight; y++) {
                for (int x = 0; x < gridWidth; x++) {
                    int index = y * gridWidth + x;
                    int xx = x + optiGridOffset.x;
                    int yy = y + optiGridOffset.y;
                    if (gridString[index] == '1') {
                        //Console.WriteLine("Will create (optimized) tile at room pos " + xx + ", " + yy + " !!!");
                        PointTileNode newNode = new PointTileNode(new IntPoint(xx, yy), this);
                        _tilesByLocalPositionHash[newNode.localPoint.GetHashCode()] = newNode;
                    } else {
                        //Console.WriteLine("No tile at room pos " + xx + ", " + yy);
                    }
                }
            }
           
            UpdateBounds();
        }

        /// <summary>
        /// Warning, too slow to run for each tile during load
        /// </summary>
        public void ApplyTileData()
        {
            #if DEBUG && LOG
            Console.WriteLine("Called Room.ApplyTileData() WARNING, this is SUPER SLOW. Callstack: " + Environment.StackTrace);
            #endif

            if (optiGrid != "") {
                return;
            }

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
            if (optiGrid != "") {
                // Should not have to refresh tile data if there is an optimized grid in this room
                return;
            }

            CELL_tiles.data = (from PointTileNode n in _tilesByLocalPositionHash.Values select n.localPoint).ToArray();
        }

        public void AddTile(PointTileNode pTileNode)
        {
#if DEBUG && LOG
            Console.WriteLine("Called Room.AddTile() Warning, this is slow. Callstack: " + Environment.StackTrace);
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

        /// <returns>Can return null if there is no tile at the position!</returns>
        public PointTileNode GetTile(int x, int y)
        {
            // Console.WriteLine("x" + x + ", y" + y);
            PointTileNode t = null;
            _tilesByLocalPositionHash.TryGetValue(BitCruncher.PackTwoShorts(x, y), out t);
            return t;
        }

        public PointTileNode[] tiles {
            get {
                return _tilesByLocalPositionHash.Values.ToArray();
            }
        }

        private float ManhattanDistance(IntPoint pPosition1, IntPoint pPosition2)
        {
            return Math.Abs(pPosition1.x - pPosition2.x) + Math.Abs(pPosition1.y - pPosition2.y);
        }

        public PointTileNode FindClosestTile (IntPoint pPosition)
        {
            PointTileNode closest = null;
            float smallestManhattanDistance = float.MaxValue;

            foreach(var tile in _tilesByLocalPositionHash.Values) {
                float distance = ManhattanDistance(pPosition, tile.localPoint);
                if(distance < smallestManhattanDistance) {
                    closest = tile;
                    smallestManhattanDistance = distance;
                }
            }

            return closest;
        }

        public PointTileNode FindClosestFreeTile (IntPoint pPosition, int tileGroup)
        {
            var tilesInSameGroup = _tilesByLocalPositionHash.Values.Where(t => t.group == tileGroup);
            var closestTile = tilesInSameGroup.OrderBy(t => ManhattanDistance(pPosition, t.localPoint)).First();
            return closestTile;
        }

//        public PointTileNode[] FindNClosestTiles (IntPoint pPosition, int n)
//        {
//            var tilesByDistance = _tilesByLocalPositionHash.Values.OrderBy(t => ManhattanDistance(pPosition, t.localPoint));
//            return tilesByDistance.Take(n).ToArray();
//        }

        public IntPoint WorldToLocalPoint(IntPoint pSource)
        {
            return pSource - worldPosition;
        }

        IntPoint[] _tilePointsOptigridCache;

        public IntPoint[] points {
            get { 
                if (optiGrid == "") {
                    return CELL_tiles.data;
                } else {
                    if (_tilePointsOptigridCache == null) {
                        _tilePointsOptigridCache = _tilesByLocalPositionHash.Values.Select(t => t.localPoint).ToArray();
                    }
                    return _tilePointsOptigridCache;
                }
            }
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

        public IntPoint worldPosition {
            set {
                CELL_worldPosition.data = value;
            }
            get {
                return CELL_worldPosition.data;
            }
        }

        public List<Ting> GetTings() {
            var tingsInRoom = new List<Ting>();
            foreach (var tile in _tilesByLocalPositionHash.Values) {
                tingsInRoom.AddRange(tile.GetOccupants());
            }
            return tingsInRoom;
        }

        public List<T> GetTingsOfType<T>() where T : Ting  {
            var tingsInRoom = new List<T>();
            foreach (var tile in _tilesByLocalPositionHash.Values) {
                tingsInRoom.AddRange(tile.GetOccupantsOfType<T>());
            }
            return tingsInRoom;
        }

        public bool HasTinyTileGroup (int pLimit)
        {
            Dictionary<int, int> _groupCounts = new Dictionary<int, int>();

            foreach (var tile in tiles) {
                if(_groupCounts.ContainsKey(tile.group)) {
                    _groupCounts[tile.group]++;
                } else {
                    _groupCounts[tile.group] = 1;
                }
            }

            foreach (int g in _groupCounts.Keys) {
                if (_groupCounts[g] <= pLimit) {
                    return true;
                }
            }

            return false;
        }
    }
}

