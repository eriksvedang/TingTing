//#define LOG_ACTIONS
#define CACHING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using RelayLib;
using GameTypes;
using System.Runtime.CompilerServices;
using System.Text;

namespace TingTing
{
	public abstract class Ting : RelayObjectTwo
	{
		public static readonly string TABLE_NAME = "Ting_Base";
		public Logger logger = new Logger();
		protected TingRunner _tingRunner;
		protected RoomRunner _roomRunner;
		private bool _isOccupyingTile = false;

		public bool isDeleted { get; set; }

		public delegate void OnNewAction(string pOldAction,string pNewAction);

		public OnNewAction onNewAction;

		#region PRE_TING_CREATION

		/// <summary>
		/// When a new Ting is created we need to have it created at the correct starting position,
		/// otherwise the SetupCells can't resolve their appropriate room, (used functionality in subclass Door).
		/// </summary>
		private WorldCoordinate _startingPosition = WorldCoordinate.NONE;
		private Direction _startingDirection = Direction.RIGHT;
		private string _startingName = "unnamed";

		internal void SetInitCreateValues(string pName, WorldCoordinate pPosition, Direction pDirection)
		{
			_startingName = pName;
			_startingPosition = pPosition;
			_startingDirection = pDirection;
		}

		#endregion

        public override int GetHashCode()
        {
            return CELL_name.data.GetHashCode();
        }

		public virtual void Update(float dt)
		{
		}

		public virtual void FixBeforeSaving()
		{
		}
		// this function can be used to fix things about the ting based on its properties, to make certain automatic adjustments (helps when editing levels)

		#region CELLS

		ValueEntry<string> CELL_name;
		ValueEntry<WorldCoordinate> CELL_position;
		ValueEntry<Direction> CELL_direction;
		ValueEntry<string> CELL_dialogueLine;
		ValueEntry<string> CELL_actionName;
		ValueEntry<bool> CELL_actionHasFired;
		ValueEntry<float> CELL_actionStartTime;
		ValueEntry<float> CELL_actionTriggerTime;
		ValueEntry<float> CELL_actionEndTime;
		ValueEntry<string> CELL_actionOtherObjectName;
		ValueEntry<string> CELL_prefab;
		ValueEntry<bool> CELL_isBeingHeld;

        protected bool _dialogueLineIsEmpty_Cache;
        protected string _actionName_Cache;

		protected override void SetupCells()
		{
			CELL_name = EnsureCell("name", _startingName);
			CELL_position = EnsureCell("position", _startingPosition);
			CELL_direction = EnsureCell("direction", _startingDirection);
			CELL_dialogueLine = EnsureCell("dialogueLine", "");
			CELL_actionName = EnsureCell("action", "");
			CELL_actionHasFired = EnsureCell("actionHasFired", false);
			CELL_actionStartTime = EnsureCell("startTime", 0f);
			CELL_actionTriggerTime = EnsureCell("triggerTime", 0f);
			CELL_actionEndTime = EnsureCell("endTime", 0f);
			CELL_actionOtherObjectName = EnsureCell("otherObjectName", "");
			CELL_prefab = EnsureCell("prefab", "unspecified");
			CELL_isBeingHeld = EnsureCell("isBeingHeld", false);

            _dialogueLineIsEmpty_Cache = (CELL_dialogueLine.data == "");
            _actionName_Cache = CELL_actionName.data;
		}

		#endregion

		#region ACTIONS

		public virtual bool CanInteractWith(Ting pTingToInteractWith)
		{
			return false;
		}

		public virtual void InteractWith(Ting pTingToInteractWith)
		{
			throw new NotImplementedException();
		}

		protected virtual void ActionTriggered(Ting pOtherTing)
		{
		}

		public void UpdateAction(float pTime)
		{
			if (actionName != "") {
				if (!actionHasFired && pTime >= actionTriggerTime) {
#if DEBUG && LOG_ACTIONS
					if (actionOtherObject == null) {
						logger.Log("Triggering action '" + actionName + "' at time " + _tingRunner.gameClock);
					} else {
						logger.Log("Triggering action '" + actionName + "' with other ting '" + actionOtherObject.name + "' at time " + _tingRunner.gameClock);
					}
#endif
					actionHasFired = true;
					ActionTriggered(actionOtherObject);
				}
				if (pTime > actionEndTime) {
#if DEBUG && LOG_ACTIONS
					logger.Log("pTime (" + pTime + ") > actionEndTime (" + actionEndTime + ")");
#endif
					StopAction();
				}
			}
		}

		public void StartAction(string pActionName, Ting pOtherObject, float pLengthUntilTrigger, float pActionLength)
		{
			string oldAction = actionName;
#if DEBUG && LOG_ACTIONS
			logger.Log("Starting action '" + pActionName + "' at time " + _tingRunner.gameClock);
#endif
			actionName = pActionName;
			float aStartTime = _tingRunner.actionTime;
			actionStartTime = aStartTime;
			actionEndTime = aStartTime + pActionLength;
			actionTriggerTime = aStartTime + pLengthUntilTrigger;
			actionHasFired = false;
			actionOtherObject = pOtherObject;
			if (onNewAction != null) {
				onNewAction(oldAction, pActionName);
            }
		}

		public void StopAction()
		{
#if DEBUG && LOG_ACTIONS
			logger.Log("Stopping action '" + actionName + "' at time " + _tingRunner.gameClock);
#endif
			string oldActionName = actionName;
			actionName = "";
			actionOtherObject = null;
			if (onNewAction != null)
				onNewAction(oldActionName, "");
		}

		#endregion

		#region ACCESSORS

		public override string ToString()
		{
			return name;
		}

		public string name {
			get {
				return CELL_name.data;
			}
			private set {
				CELL_name.data = value;
			}
		}

		public bool HasInteractionPointHere(WorldCoordinate finalTargetPosition)
		{
			if (room.name == finalTargetPosition.roomName) {
				foreach (IntPoint pos in interactionPoints) {
					if (finalTargetPosition.localPosition == pos) {
						return true;
					}
				}
			}
			return false;
		}

		[ShowInEditor]
		public WorldCoordinate position {
			get { 
				return CELL_position.data; 
			}
			set {
				//logger.Log("Position of " + name + " is being set to " + value);
                
				string prevRoomName = CELL_position.data.roomName;
                
#if DEBUG
				if (!_roomRunner.HasRoom(value.roomName)) {
					throw new WorldCoordinateException("Can't place a ting in a undefined room: " + value.roomName);
				}
#endif
				
				if (_isOccupyingTile) {
					DisconnectFromCurrentTile();
				}
                
				CELL_position.data = value; 
				ConnectToCurrentTile();

#if CACHING
                SetCachedTile();
#endif
                
				if (prevRoomName != CELL_position.data.roomName && _tingRunner.onTingHasNewRoom != null) {
					_tingRunner.onTingHasNewRoom(this, value.roomName);
				}
			}
		}

		protected void DisconnectFromCurrentTile()
		{
			room.GetTile(position.localPosition).RemoveOccupant(this);
			_isOccupyingTile = false;
		}

		protected void ConnectToCurrentTile()
		{
			//D.isNull(room, "room is null");

#if CACHING
            SetCachedRoom(); // got to update this!!!
#endif

			PointTileNode tile = room.GetTile(position.localPosition);
			if (tile == null) {
				//D.Log("Found no tile for Ting " + name);
			} else {
				tile.AddOccupant(this);
				_isOccupyingTile = true;
			}
		}

		public Room room {
			get {
#if CACHING
                if (_cachedRoom == null) {
                    SetCachedRoom();
                }
                return _cachedRoom;
#else
                return _roomRunner.GetRoom(CELL_position.data.roomName);
#endif
			}
		}

#if CACHING
        Room _cachedRoom = null;

        void SetCachedRoom() {
            _cachedRoom = _roomRunner.GetRoom(CELL_position.data.roomName);
        }
#endif

		/// <summary>
		/// Gets the tile under the Ting. Can return null if the position of the Ting is outside the tile grid.
		/// </summary>
		public PointTileNode tile {
			get {
#if CACHING
                if (!_hasSetCachedTile) {
                    SetCachedTile();
                }
                return _cachedTile;
#else 
                return room.GetTile(localPoint);
#endif
            }
		}

#if CACHING
        protected void SetCachedTile() {
            _cachedTile = room.GetTile(localPoint);
            _hasSetCachedTile = true;
        }

        private PointTileNode _cachedTile = null;
        private bool _hasSetCachedTile = false;
#endif

		public IntPoint localPoint {
			get { return CELL_position.data.localPosition; }
		}

		public IntPoint worldPoint {
			get { 
                return room.worldPosition + this.localPoint; 
            }
		}

        IntPoint _worldPointCache = IntPoint.Zero;


		[ShowInEditor]
		public Direction direction {
			get {
				return CELL_direction.data;
			}
			set {
				CELL_direction.data = value;
			}
		}

		[ShowInEditor]
		public string dialogueLine {
			get {
				return CELL_dialogueLine.data;
			}
			set {
				CELL_dialogueLine.data = value;
                _dialogueLineIsEmpty_Cache = (value == "");
			}
		}

		[EditableInEditor]
		public string actionName {
			get {
                return _actionName_Cache;
				//return CELL_actionName.data;
			}
			set {
				CELL_actionName.data = value;
                _actionName_Cache = value;
			}
		}

		[ShowInEditor]
		public bool actionHasFired {
			get {
				return CELL_actionHasFired.data;
			}
			set {
				CELL_actionHasFired.data = value;
			}
		}
		//[ShowInEditor]
		public float actionStartTime {
			get {
				return CELL_actionStartTime.data;
			}
			set {
				CELL_actionStartTime.data = value;
			}
		}
		//[ShowInEditor]
		public float actionTriggerTime {
			get {
				return CELL_actionTriggerTime.data;
			}
			set {
				CELL_actionTriggerTime.data = value;
			}
		}
		//[ShowInEditor]
		public float actionEndTime {
			get {
				return CELL_actionEndTime.data;
			}
			set {
				CELL_actionEndTime.data = value;
			}
		}

		[EditableInEditor]
		public string prefab {
			get {
				return CELL_prefab.data;
			}
			set {
				CELL_prefab.data = value;
			}
		}

		[ShowInEditor]
		public bool isBeingHeld {
			get {
				return CELL_isBeingHeld.data;
			}
			set {
				CELL_isBeingHeld.data = value;
			}
		}

		[ShowInEditor]
		public Ting actionOtherObject {
			get {
				if (CELL_actionOtherObjectName.data == "")
					return null;
				else
					return _tingRunner.GetTing(CELL_actionOtherObjectName.data);
			}
			set {
				if (value == null)
					CELL_actionOtherObjectName.data = "";
				else
					CELL_actionOtherObjectName.data = value.name;
			}
		}

		internal void SetupBaseRunners(TingRunner pTingRunner, RoomRunner pRoomRunner)
		{
			_roomRunner = pRoomRunner;
			_tingRunner = pTingRunner;
		}

		public virtual bool canBePickedUp {
			get {
				return false;
			}
		}

		public virtual IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { 
					localPoint + IntPoint.Down, 
					localPoint + IntPoint.Up, 
					localPoint + IntPoint.Left, 
					localPoint + IntPoint.Right
				};
			}
		}

        public virtual IntPoint[] interactionPointsTryTheseFirst {
            get {
                return null;
            }
        }

		[ShowInEditor]
		public virtual bool isBeingUsed {
			get {
				return false;
			}
		}

		public bool AtLeastOneInteractionPointIsOccupied()
		{
			if (room == null) {
				D.Log("Room of " + name + " is null, can't check for occupied interaction points.");
				return false;
			}
			if (interactionPoints.Length == 0) {
				D.Log("Length of interactionPoints[] " + name + " is 0, can't check for occupied interaction points.");
				return false;
			}
			PointTileNode tile = room.GetTile(interactionPoints[0]);
			if (tile == null) {
				D.Log("No tile at interaction point, can't check for occupied interaction points.");
				return false;
			}
			return tile.HasOccupants();
		}

		public bool AnotherTingSharesTheTile()
		{
			if (room == null) {
				//D.Log("Room of " + name + " is null, can't check for occupants.");
				return false;
			}
			if (this.tile == null) {
				//D.Log(name + ": Tile at self position is null, can't check for occupants.");
				return false;
			}
			return this.tile.HasOccupants(this);
		}

		[ShowInEditor]
		public string occupantsOnTile {
			get {
				if (tile == null) {
					return "Not on a tile";
				}
				Ting[] occupants = tile.GetOccupants();
				List<string> occupantNames = new List<string>();
				foreach (var occupant in occupants) {
					occupantNames.Add(occupant.name);
				}
				return string.Join(",", occupantNames.ToArray());
			}
		}

		public virtual string tooltipName {
			get {
				return "Ting";
			}
		}

		public virtual string verbDescription {
			get {
				return "Use [NAME]";
			}
		}
        
        public virtual string UseTingOnTingDescription(Ting pOtherTing) {
            return "use " + tooltipName + " on " + pOtherTing.tooltipName;
        }

		[ShowInEditor]
		public float actionPercentage {
			get {
				//D.Log("pTime: " + _tingRunner.actionTime + ", actionStartTime: " + actionStartTime + ", actionEndTime: " + actionEndTime);

				float answer = (_tingRunner.actionTime - actionStartTime) / (actionEndTime - actionStartTime);
				if (answer < 0f)
					answer = 0f;
				if (answer > 1f)
					answer = 1f;
				return answer;
			}
		}

		protected GameTime gameClock { get { return _tingRunner.gameClock; } }

		#endregion

	}
}