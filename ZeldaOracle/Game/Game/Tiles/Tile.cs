﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ZeldaOracle.Common.Content;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Scripting;
using ZeldaOracle.Game.Main;
using ZeldaOracle.Game.Control;
using ZeldaOracle.Game.Entities;
using ZeldaOracle.Game.Entities.Effects;
using ZeldaOracle.Game.Entities.Projectiles;
using ZeldaOracle.Game.Items;
using ZeldaOracle.Game.Items.Drops;
using ZeldaOracle.Game.Items.Weapons;
using ZeldaOracle.Game.Worlds;
using ZeldaOracle.Common.Audio;
using ZeldaOracle.Game.Entities.Projectiles.Seeds;

namespace ZeldaOracle.Game.Tiles {
	
	public class Tile : IPropertyObject, ZeldaAPI.Tile {

		// Internal
		private RoomControl			roomControl;
		private bool				isAlive;
		private bool				isInitialized;
		private Point2I				location;		// The tile location in the room.
		private int					layer;			// The layer this tile is in.
		private Point2I				moveDirection;
		private int					moveDistance;
		private int					currentMoveDistance;
		private bool				isMoving;
		private float				movementSpeed;
		private Vector2F			offset;			// Offset in pixels from its tile location (used for movement).
		protected AnimationPlayer	animationPlayer;
		private bool				hasMoved;
		protected TilePath			path;			// The path the tile is currently following.
		private int					pathTimer;
		private int					pathMoveIndex;
		protected bool				fallsInHoles;
		private Vector2F			velocity;
		protected Sound				soundMove;
		private Vector2F			conveyorVelocity;

		// Settings
		private TileDataInstance	tileData;		// The tile data used to create this tile.
		private TileFlags			flags;
		private Point2I				size;			// How many tile spaces this tile occupies. NOTE: this isn't supported yet.
		private CollisionModel		collisionModel;
		private SpriteAnimation		customSprite;
		private SpriteAnimation		spriteAsObject;	// The sprite for the tile if it were picked up, pushed, etc.
		private Animation			breakAnimation;	// The animation to play when the tile is broken.
		private Sound				breakSound;	// The sound to play when the tile is broken.
		private int					pushDelay;		// Number of ticks of pushing before the player can move this tile.
		private DropList			dropList;
		private bool				isSolid;
		private Properties			properties;
		
		public bool IsUpdated { get; set; } // This is to make sure tiles are only updated once per frame.


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------
		
		// Use Tile.CreateTile() instead of this constructor.
		protected Tile() {
			isAlive				= false;
			isInitialized		= false;
			location			= Point2I.Zero;
			layer				= 0;
			offset				= Point2I.Zero;
			size				= Point2I.One;
			customSprite		= new SpriteAnimation();
			spriteAsObject		= new SpriteAnimation();
			isSolid				= false;
			isMoving			= false;
			pushDelay			= 20;
			properties			= new Properties(this);
			tileData			= null;
			moveDirection		= Point2I.Zero; 
			dropList			= null;
			animationPlayer		= null;
			hasMoved			= false;
			path				= null;
			pathTimer			= 0;
			pathMoveIndex		= 0;
			fallsInHoles		= true;
			soundMove			= GameData.SOUND_BLOCK_PUSH;
			conveyorVelocity	= Vector2F.Zero;
		}


		//-----------------------------------------------------------------------------
		// Initialization
		//-----------------------------------------------------------------------------
		
		public void Initialize(RoomControl control) {
			this.roomControl	= control;
			this.isAlive		= true;

			if (!isInitialized) {
				isInitialized	= true;
				hasMoved		= false;
				velocity		= Vector2F.Zero;
				
				// Begin a path if there is one.
				string pathString = properties.GetString("path", "");
				TilePath p = TilePath.Parse(pathString);
				BeginPath(p);

				// Set the solid state.
				isSolid = (SolidType != TileSolidType.NotSolid);

				// Setup default drop list.
				if (IsDigable && !IsSolid)
					dropList = RoomControl.GameControl.DropManager.GetDropList("dig");
				else
					dropList = RoomControl.GameControl.DropManager.GetDropList("default");

				// Call the virtual initialization method.
				OnInitialize();
			}
		}


		//-----------------------------------------------------------------------------
		// Flags
		//-----------------------------------------------------------------------------

		// Returns true if the tile has the normal flags.
		public bool HasFlag(TileFlags flags) {
			return Flags.HasFlag(flags);
		}
		
		public void SetFlags(TileFlags flagsToSet, bool enabled) {
			if (enabled)
				flags |= flagsToSet;
			else
				flags &= ~flagsToSet;
		}
		

		//-----------------------------------------------------------------------------
		// Movement
		//-----------------------------------------------------------------------------

		// Begin following a path.
		public void BeginPath(TilePath path) {
			this.path = path;
			pathMoveIndex = 0;
			pathTimer = 0;
		}

		// Move over a distance.
		protected bool Move(int direction, int distance, float movementSpeed) {
			if (isMoving)
				return false;

			int newLayer;
			if (IsMoveObstructed(direction, out newLayer))
				return false;

			this.movementSpeed	= movementSpeed;
			this.moveDistance	= distance;
			this.moveDirection	= Directions.ToPoint(direction);
			this.isMoving		= true;
			this.hasMoved		= true;
			this.currentMoveDistance	= 0;
			
			// Move the tile one step forward.
			Point2I oldLocation = location;
			RoomControl.MoveTile(this, location + moveDirection, newLayer);
			offset = -Directions.ToVector(direction) * GameSettings.TILE_SIZE;

			// Uncover the tile this was located over.
			// TODO: larger sized tiles, moving more than 1 distance.
			Tile unconveredTile = roomControl.GetTopTile(oldLocation);
			if (unconveredTile != null)
				unconveredTile.OnUncover(this);

			// Fire the OnMove event.
			GameControl.ExecuteScript(properties.GetString("on_move", ""));

			return true;
		}

		protected bool IsMoveObstructed(int direction, out int newLayer) {
			Point2I newLocation = location + Directions.ToPoint(direction);
			return IsMoveObstructed(newLocation, out newLayer);
		}

		protected bool IsMoveObstructed(Point2I newLocation, out int newLayer) {
			newLayer = -1;

			// Check if the move will keep the tile in the room bounds.
			Rectangle2I roomRect = new Rectangle2I(0, 0, roomControl.Room.Width, roomControl.Room.Height);
			Rectangle2I tileRect = new Rectangle2I(newLocation, size);
			if (!roomRect.Contains(tileRect))
				return true;

			// Check for tile obstructions and find an empty layer to put the tile in.
			for (int i = 0; i < RoomControl.Room.LayerCount; i++) {
				for (int x = 0; x < size.X; x++) {
					for (int y = 0; y < size.Y; y++) {
						Point2I loc = newLocation + new Point2I(x, y);
						Tile t = RoomControl.GetTile(newLocation + new Point2I(x, y), i);

						if (t == this) {
							newLayer = i;
						}
						else {
							if (t != null && !t.IsCoverableByBlock)
								return true;
							if (t == null && newLayer != layer)
								newLayer = i;
						}
					}
				}
			}

			return (newLayer < 0);
		}


		//-----------------------------------------------------------------------------
		// Interaction Methods
		//-----------------------------------------------------------------------------
		
		// Called when a seed of the given type hits this tile.
		public virtual void OnSeedHit(SeedType type, SeedEntity seed) { }
		
		// Called when the tile is hit by one of the player's projectile.
		public virtual void OnHitByProjectile(Projectile projectile) {
			if (projectile is SeedEntity) {
				SeedEntity seed = (SeedEntity) projectile;
				OnSeedHit(seed.SeedType, seed);
			}
		}

		// Called when the player presses A on this tile, when facing the given direction.
		// Return true if player controls should be disabled for the rest of the frame.
		public virtual bool OnAction(int direction) { return false; }

		// Called when the player touches any part of the tile area.
		public virtual void OnTouch() { }

		// Called when the player touches the collision box of the tile.
		public virtual void OnCollide() { }

		// Called when the player hits this tile with the sword.
		public virtual void OnSwordHit(ItemWeapon swordItem) {
			int minLevel = properties.GetInteger("cuttable_sword_level", Item.Level1);
			if (!isMoving && flags.HasFlag(TileFlags.Cuttable) &&
				(!(swordItem is ItemSword) || swordItem.Level >= minLevel))
			{
				Break(true);
			}
		}

		// Called when the player hits this tile with the sword.
		public virtual void OnBombExplode() {
			if (!isMoving && flags.HasFlag(TileFlags.Bombable))
				Break(true);
		}

		// Called when the tile is burned by a fire.
		public virtual void OnBurn() {
			if (!isMoving && flags.HasFlag(TileFlags.Burnable)) {
				SpawnDrop();
				roomControl.RemoveTile(this);
				if (properties.GetBoolean("disable_on_destroy", false))
					Properties.SetBase("enabled", false);
			}
		}

		// Called when the tile is hit by the player's boomerang.
		public virtual void OnBoomerang() {
			if (!isMoving && flags.HasFlag(TileFlags.Boomerangable))
				Break(true);
		}

		// Called when the player wants to push the tile.
		public virtual bool OnPush(int direction, float movementSpeed) {
			if (!HasFlag(TileFlags.Movable))
				return false;
			if (properties.GetBoolean("move_once", false) && hasMoved)
				return false;
			int moveDir = properties.GetInteger("move_direction", -1);
			if (moveDir >= 0 && direction != moveDir)
				return false;
			if (Move(direction, 1, movementSpeed)) {
				if (soundMove != null)
					AudioSystem.PlaySound(soundMove);
				return true;
			}
			return false;
		}

		// Called when the player digs the tile with the shovel.
		public virtual bool OnDig(int direction) {
			if (isMoving || !IsDigable)
				return false;

			// Remove/dig the tile.
			if (layer == 0) {
				roomControl.RemoveTile(this);
					
				// Spawn the a "dug" tile in this tile's place.
				TileData data = Resources.GetResource<TileData>("dug");
				Tile dugTile = Tile.CreateTile(data);
				roomControl.PlaceTile(dugTile, location, layer);
				customSprite = GameData.SPR_TILE_DUG;
			}
			else {
				roomControl.RemoveTile(this);
			}

			if (properties.GetBoolean("disable_on_destroy", false))
				Properties.SetBase("enabled", false);

			// Spawn drops.
			Entity dropEntity = SpawnDrop();
			if (dropEntity != null) {
				if (dropEntity is Collectible)
					(dropEntity as Collectible).PickupableDelay = GameSettings.COLLECTIBLE_DIG_PICKUPABLE_DELAY;
				dropEntity.Physics.Velocity = Directions.ToVector(direction) * GameSettings.DROP_ENTITY_DIG_VELOCITY;
			}

			return true;
		}

		// Called while the player is trying to push the tile but before it's actually moved.
		public virtual void OnPushing(int direction) { }

		// Called when the player jumps and lands on the tile.
		public virtual void OnLand(Point2I startTile) { }
			
		// Called when the tile completes a movement (like after being pushed).
		public virtual void OnCompleteMovement() {
			// Check if we are over a hazard tile (water, lava, hole).
			// TEMP: Only movable tiles can fall in hazards.
			if (HasFlag(TileFlags.Movable)) {
				Tile tile = null;
				for (int i = layer - 1; i >= 0 && tile == null; i--)
					tile = roomControl.GetTile(location, i);

				if (tile != null) {
					if (tile.IsWater)
						OnFallInWater();
					else if (tile.IsLava)
						OnFallInLava();
					else if (tile.IsHole)
						OnFallInHole();
					else
						tile.OnCover(this);
				}
			}
		}

		// Called when the tile is pushed into a hole.
		public virtual void OnFallInHole() {
			if (fallsInHoles) {
				RoomControl.SpawnEntity(new EffectFallingObject(), Center);
				AudioSystem.PlaySound(GameData.SOUND_OBJECT_FALL);
				RoomControl.RemoveTile(this);
			}
		}

		// Called when the tile is pushed into water.
		public virtual void OnFallInWater() {
			if (fallsInHoles) {
				RoomControl.SpawnEntity(new Effect(GameData.ANIM_EFFECT_WATER_SPLASH, DepthLayer.EffectSplash, true), Center);
				AudioSystem.PlaySound(GameData.SOUND_PLAYER_WADE);
				RoomControl.RemoveTile(this);
			}
		}

		// Called when the tile is pushed into lava.
		public virtual void OnFallInLava() {
			if (fallsInHoles) {
				RoomControl.SpawnEntity(new Effect(GameData.ANIM_EFFECT_LAVA_SPLASH, DepthLayer.EffectSplash, true), Center);
				AudioSystem.PlaySound(GameData.SOUND_PLAYER_WADE);
				RoomControl.RemoveTile(this);
			}
		}

		// Called when another tile covers this tile.
		public virtual void OnCover(Tile tile) { }

		// Called when this tile is uncovered.
		public virtual void OnUncover(Tile tile) { }


		//-----------------------------------------------------------------------------
		// Mutators
		//-----------------------------------------------------------------------------

		// Break the tile, destroying it.
		public virtual void Break(bool spawnDrops) {
			// Spawn the break effect.
			if (breakAnimation != null) {
				Effect breakEffect = new Effect(breakAnimation, DepthLayer.EffectTileBreak, true);
				RoomControl.SpawnEntity(breakEffect, Center);
			}

			if (breakSound != null)
				AudioSystem.PlaySound(breakSound);

			// Spawn drops.
			if (spawnDrops)
				SpawnDrop();
			
			// Destroy the tile.
			if (properties.GetBoolean("disable_on_destroy", false))
				Properties.SetBase("enabled", false);
			RoomControl.RemoveTile(this);
		}

		// Spawn a drop entity based on the random drop-list.
		public Entity SpawnDrop() {
			Entity dropEntity = null;
			
			// Choose a random drop (or none) from the list.
			if (dropList != null)
				dropEntity = dropList.CreateDropEntity(GameControl);

			// Spawn the drop if there is one.
			if (dropEntity != null) {
				dropEntity.SetPositionByCenter(Center);
				dropEntity.Physics.ZVelocity = GameSettings.DROP_ENTITY_SPAWN_ZVELOCITY;
				RoomControl.SpawnEntity(dropEntity);
			}

			return dropEntity;
		}


		//-----------------------------------------------------------------------------
		// Simulation
		//-----------------------------------------------------------------------------

		public virtual void OnInitialize() {}

		public virtual void OnRemoveFromRoom() {}

		public virtual void Update() {
			// Velocity must be applied on the next frame in order to syncronize
			// entities moving on this tile, because entities are updated before tiles.
			if (isMoving)
				offset += velocity;
			else
				velocity = Vector2F.Zero;

			// Update movement.
			if (isMoving) {
				// Set the amount to move for the next frame.
				if (isMoving)
					velocity = (Vector2F) moveDirection * movementSpeed;
				
				if (offset.Dot(moveDirection) >= 0.0f) {
					currentMoveDistance++;
					offset -= (Vector2F) (moveDirection * GameSettings.TILE_SIZE);

					if (currentMoveDistance >= moveDistance) {
						offset			= Vector2F.Zero;
						velocity		= Vector2F.Zero;
						moveDirection	= Point2I.Zero;
						isMoving		= false;
						OnCompleteMovement();
					}
					else {
						roomControl.MoveTile(this, location + moveDirection, layer);
					}
				}
				else if (currentMoveDistance + 1 >= moveDistance) {
					// Don't overshoot the destination.
					float overshoot = (offset + velocity).Dot(moveDirection);
					if (overshoot >= 0.0f) {
						velocity -= overshoot * (Vector2F) moveDirection;
					}
				}
			}
			// Update path following.
			else if (path != null) {
				TilePathMove move = path.Moves[pathMoveIndex];
					
				// Begin the next move in the path after the delay has been passed.
				if (pathTimer >= move.Delay) {
					Move(move.Direction, move.Distance, move.Speed);
					pathTimer = 0;
					pathMoveIndex++;
					if (pathMoveIndex >= path.Moves.Count) {
						if (path.Repeats)
							pathMoveIndex = 0;
						else
							path = null;
					}
				}

				pathTimer++;
			}
		}

		public virtual void UpdateGraphics() {

		}

		public virtual void Draw(Graphics2D g) {
			SpriteAnimation sprite = (!customSprite.IsNull ? customSprite : CurrentSprite);
			if (isMoving && !spriteAsObject.IsNull)
				sprite = spriteAsObject;

			if (animationPlayer != null) {
				g.DrawAnimation(animationPlayer.SubStrip, Zone.ImageVariantID,
					animationPlayer.PlaybackTime, Position);
			}
			else if (sprite.IsAnimation) {
				// Draw as an animation.
				g.DrawAnimation(sprite.Animation, Zone.ImageVariantID,
					RoomControl.GameControl.RoomTicks, Position);
			}
			else if (sprite.IsSprite) {
				// Draw as a sprite.
				g.DrawSprite(sprite.Sprite, Zone.ImageVariantID, Position);
			}
		}


		//-----------------------------------------------------------------------------
		// Static Methods
		//-----------------------------------------------------------------------------

		// Construct a tile from the given tile-data.
		public static Tile CreateTile(TileData data) {
			return CreateTile(new TileDataInstance(data, 0, 0, 0));
		}

		// Construct a tile from the given tile-data.
		public static Tile CreateTile(TileDataInstance data) {
			Tile tile;
			
			// Construct the tile.
			if (data.Type == null)
				tile = new Tile();
			else
				tile = (Tile) data.Type.GetConstructor(Type.EmptyTypes).Invoke(null);
			
			tile.location			= data.Location;
			tile.layer				= data.Layer;

			tile.tileData			= data;
			tile.flags				= data.Flags;
			tile.spriteAsObject		= data.SpriteAsObject;
			tile.breakAnimation		= data.BreakAnimation;
			tile.breakSound			= data.BreakSound;
			tile.collisionModel		= data.CollisionModel;
			tile.size				= data.Size;
			
			int conveyorAngle = data.ConveyorAngle;
			if (conveyorAngle >= 0)
				tile.conveyorVelocity = Angles.ToVector(conveyorAngle, true) * data.ConveyorSpeed;

			//tile.properties.SetAll(data.BaseProperties);
			//tile.properties.SetAll(data.Properties);
			tile.properties.BaseProperties	= data.Properties;
			
			return tile;
		}

		public static Type GetType(string typeName, bool ignoreCase) {
			StringComparison comparision = StringComparison.Ordinal;
			if (ignoreCase)
				comparision = StringComparison.OrdinalIgnoreCase;

			return Assembly.GetExecutingAssembly().GetTypes()
				.FirstOrDefault(t => t.Name.Equals(typeName, comparision));
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------
		
		// Returns the room control this tlie belongs to.
		public RoomControl RoomControl {
			get { return roomControl; }
			set { roomControl = value; }
		}
		
		public GameControl GameControl {
			get { return roomControl.GameControl; }
		}

		public Zone Zone {
			get { return roomControl.Room.Zone; }
		}

		public Vector2F Position {
			get { return (location * GameSettings.TILE_SIZE) + offset; }
		}

		public Vector2F Center {
			get { return Position + ((Vector2F) size * (0.5f * GameSettings.TILE_SIZE)); }
		}

		public Vector2F Offset {
			get { return offset; }
			set { offset = value; }
		}

		public Point2I Location {
			get { return location; }
			set { location = value; }
		}
		
		public int Layer {
			get { return layer; }
			set { layer = value; }
		}

		public Point2I Size {
			get { return size; }
			set { size = value; }
		}
		
		public int Width {
			get { return size.X; }
			set { size.X = value; }
		}
		
		public int Height {
			get { return size.Y; }
			set { size.Y = value; }
		}

		public TileFlags Flags {
			get { return flags; }
		}

		public SpriteAnimation CustomSprite {
			get { return customSprite; }
			set {
				if (value == null)
					customSprite.SetNull();
				else
					customSprite.Set(value);
			}
		}

		public SpriteAnimation SpriteAsObject {
			get { return spriteAsObject; }
			set {
				if (value == null)
					spriteAsObject.SetNull();
				else
					spriteAsObject.Set(value);
			}
		}

		public SpriteAnimation CurrentSprite {
			get {
				if (tileData != null && tileData.SpriteList.Length > 0)
					return tileData.SpriteList[properties.GetInteger("sprite_index")];
				return new SpriteAnimation();
			}
		}

		public int SpriteIndex {
			get { return properties.GetInteger("sprite_index"); }
			set { properties.Set("sprite_index", value); }
		}

		public Animation BreakAnimation {
			get { return breakAnimation; }
			set { breakAnimation = value; }
		}

		public Sound BreakSound {
			get { return breakSound; }
			set { breakSound = value; }
		}

		public CollisionModel CollisionModel {
			get { return collisionModel; }
			set { collisionModel = value; }
		}

		public int PushDelay {
			get { return pushDelay; }
			set { pushDelay = value; }
		}

		public bool IsMoving {
			get { return isMoving; }
		}

		public int MoveDirection {
			get { return Directions.FromPoint(moveDirection); }
		}

		public Properties Properties {
			get { return properties; }
		}
		
		// Get the original tile data from which this was created.
		public TileDataInstance TileData {
			get { return tileData; }
		}

		public bool IsCoverableByBlock {
			get { return (!IsNotCoverable && !IsSolid && !IsStairs && !IsLadder); }
		}

		public DropList DropList {
			get { return dropList; }
			set { dropList = value; }
		}


		//-----------------------------------------------------------------------------
		// Flag Properties
		//-----------------------------------------------------------------------------

		public bool IsNotCoverable {
			get { return Flags.HasFlag(TileFlags.NotCoverable); }
		}

		public bool IsDigable {
			get { return flags.HasFlag(TileFlags.Digable); }
		}

		public bool IsSwitchable {
			get { return flags.HasFlag(TileFlags.Switchable); }
		}

		public bool StaysOnSwitch {
			get { return flags.HasFlag(TileFlags.SwitchStays); }
		}

		public bool BreaksOnSwitch {
			get { return !flags.HasFlag(TileFlags.SwitchStays); }
		}

		public bool IsBoomerangable {
			get { return flags.HasFlag(TileFlags.Boomerangable); }
		}
		
		public bool IsHole {
			get { return EnvironmentType == TileEnvironmentType.Hole ||
						 EnvironmentType == TileEnvironmentType.Whirlpool; }
		}
		
		public bool IsWater {
			get { return EnvironmentType == TileEnvironmentType.Water; }
		}
		
		public bool IsLava {
			get { return EnvironmentType == TileEnvironmentType.Lava; }
		}
		
		public bool IsHoleWaterOrLava {
			get { return (IsHole || IsWater || IsLava); }
		}
		
		public bool IsSolid {
			get { return isSolid; }
			set { isSolid = value; }
		}

		public bool IsHalfSolid {
			get { return (SolidType == TileSolidType.HalfSolid); }
		}

		public bool IsLedge {
			get { return (SolidType == TileSolidType.Ledge); }
		}
		
		public bool IsStairs {
			get { return EnvironmentType == TileEnvironmentType.Stairs; }
		}
		
		public bool IsLadder {
			get { return EnvironmentType == TileEnvironmentType.Ladder; }
		}

		// Returns true if the tile is not alive.
		public bool IsDestroyed {
			get { return !isAlive; }
		}

		// Returns true if the tile is still alive.
		public bool IsAlive {
			get { return isAlive; }
			set { isAlive = value; }
		}

		public AnimationPlayer AnimationPlayer {
			get { return animationPlayer; }
		}

		public TileEnvironmentType EnvironmentType {
			get { return properties.GetEnum<TileEnvironmentType>("environment_type", TileEnvironmentType.Normal); }
			set { properties.Set("environment_type", (int) value); }
		}

		public TileSolidType SolidType {
			get { return properties.GetEnum<TileSolidType>("solidity", TileSolidType.NotSolid); }
			set { properties.Set("solidity", (int) value); }
		}

		public int LedgeDirection {
			get { return properties.GetInteger("ledge_direction", Directions.Down); }
		}

		public bool ClingWhenStabbed {
			get { return properties.GetBoolean("cling_on_stab", true); }
			set { properties.Set("cling_on_stab", value); }
		}

		public Vector2F Velocity {
			get { return velocity; }
		}

		public Vector2F ConveyorVelocity {
			get { return conveyorVelocity; }
		}

		public virtual TileDataInstance TileDataOwner {
			get { return tileData; }
		}


		//-----------------------------------------------------------------------------
		// Scripting API
		//-----------------------------------------------------------------------------

		string ZeldaAPI.Tile.Id {
			get { return properties.GetString("id", ""); }
		}
	}
}
