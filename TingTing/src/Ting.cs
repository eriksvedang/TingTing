using System;
using System.Collections.Generic;
using System.Diagnostics;
using RelayLib;
using GameTypes;
using System.Runtime.CompilerServices;

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
		
		public delegate void OnNewAction(string pOldAction, string pNewAction);
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
		
        public virtual void Update(float dt) {}
		
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
		}
		
		#endregion
		
		#region ACTIONS

		public virtual bool CanInteractWith(Ting pTingToInteractWith) { return false; }
		public virtual void InteractWith(Ting pTingToInteractWith) { throw new NotImplementedException(); }
		protected virtual void ActionTriggered(Ting pOtherTing) {}
		
		public void UpdateAction(float pTime)
		{
			if(actionName != "")
			{
				if(!actionHasFired && pTime >= actionTriggerTime) {
#if DEBUG
					if(actionOtherObject == null) {
						logger.Log("Triggering action '" + actionName + "' at time " + _tingRunner.gameClock);
					}
					else {
						logger.Log("Triggering action '" + actionName + "' with other ting '" + actionOtherObject.name + "' at time " + _tingRunner.gameClock);
					}
#endif
					ActionTriggered(actionOtherObject);
					actionHasFired = true;
				}
				if(pTime > actionEndTime) {
#if DEBUG
					logger.Log("pTime (" + pTime + ") > actionEndTime (" + actionEndTime + ")");
#endif
					StopAction();
				}
			}
		}
		
		public void StartAction(string pActionName, Ting pOtherObject, float pLengthUntilTrigger, float pActionLength)
		{
			string oldAction = actionName;
#if DEBUG
			logger.Log("Starting action '" + pActionName + "' at time " + _tingRunner.gameClock);
#endif
			actionName = pActionName;
            float aStartTime = _tingRunner.actionTime;
			actionStartTime = aStartTime;
			actionEndTime = aStartTime + pActionLength;
			actionTriggerTime = aStartTime + pLengthUntilTrigger;
			actionHasFired = false;
			actionOtherObject = pOtherObject;
			if(onNewAction != null) onNewAction(oldAction, pActionName);
		}
		
		public void StopAction()
		{
#if DEBUG
			logger.Log("Stopping action '" + actionName + "' at time " + _tingRunner.actionTime);
#endif
			actionName = "";
			actionOtherObject = null;
		}
		
		#endregion
		
		#region ACCESSORS
		
		public string name {
			get {
				return CELL_name.data;
			}
			private set {
				CELL_name.data = value;
			}
		}
		
        [ShowInEditor]
        public WorldCoordinate position
        {
            get { 
				return CELL_position.data; 
			}
            set 
            {
                if (!_roomRunner.HasRoom(value.roomName))
                {
                    throw new WorldCoordinateException("Can't place a ting in a undefined room: " + value.roomName);
                }
				
				if(_isOccupyingTile) {
					DisconnectFromCurrentTile();
				}
				CELL_position.data = value; 
				ConnectToCurrentTile();
            }
        }

		protected void DisconnectFromCurrentTile()
		{
			room.GetTile(position.localPosition).RemoveOccupant(this);
			_isOccupyingTile = false;
		}

		protected void ConnectToCurrentTile()
		{
			D.isNull(room, "room is null");
			PointTileNode tile = room.GetTile(position.localPosition);
			if(tile == null) {
				//D.Log("Found no tile for Ting " + name);
			}
			else {
				tile.AddOccupant(this);
				_isOccupyingTile = true;
			}
		}

		public Room room
        {
			get {
                return _roomRunner.GetRoom(CELL_position.data.roomName);
			}
		}
					
        public PointTileNode tile
        {
            get { return room.GetTile(localPoint); }
        }
		
		public IntPoint localPoint
        {
			get { return CELL_position.data.localPosition; }
		}
		
        public IntPoint worldPoint
        {
            get { return room.worldPosition + this.localPoint; }
        }
		
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
			}
		}
		
		[EditableInEditor]
		public string actionName {
			get {
				return CELL_actionName.data;
			}
			set {
				CELL_actionName.data = value;
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
		
		[ShowInEditor]
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
		
		public Ting actionOtherObject {
			get
            {
				if(CELL_actionOtherObjectName.data == "") 
					return null;
				else
                    return _tingRunner.GetTing(CELL_actionOtherObjectName.data);
			}
			set
            {
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
		
		public virtual string tooltipName
		{
			get {
				return "Ting";
			}
		}
		
		public virtual string verbDescription
		{
			get {
				return "Use [NAME]";
			}
		}
		
        protected GameTime gameClock { get { return _tingRunner.gameClock; } }
		
		#endregion
	}
}