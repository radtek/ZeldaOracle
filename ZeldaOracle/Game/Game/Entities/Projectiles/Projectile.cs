﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Audio;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Graphics.Sprites;
using ZeldaOracle.Game.Entities.Collisions;
using ZeldaOracle.Game.Entities.Effects;
using ZeldaOracle.Game.Entities.Monsters;
using ZeldaOracle.Game.Entities.Players;
using ZeldaOracle.Game.Entities.Units;
using ZeldaOracle.Game.Items;
using ZeldaOracle.Game.Tiles;

namespace ZeldaOracle.Game.Entities.Projectiles {
	
	public delegate void CollisionResponse();

	public enum ProjectileType {
		Physical = 0,	// Can always be deflected
		Magic,			// Can only be deflected by shields
		Beam,			// Can only be deflected by the Mirror Shield
		NotDeflectable	// Can NEVER be deflected.
	}

	public class Projectile : Entity, IInterceptable {

		protected Angle		angle;
		protected Direction	direction;
		protected Entity	owner;
		protected Tile      tileOwner;
		protected bool		syncAnimationWithAngle;
		protected bool		syncAnimationWithDirection;
		protected Animation crashAnimation;
		protected bool		bounceOnCrash;
		protected ProjectileType projectileType;

		private event Action eventCollision;
		private event Action eventLand;
		
		private Point2I tileLocation;
		

		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------
		
		public Projectile() {
			// Graphics
			Graphics.IsShadowVisible		= true;
			Graphics.IsGrassEffectVisible	= false;
			Graphics.IsRipplesEffectVisible	= false;

			// Physics
			Physics.Enable(PhysicsFlags.DisableSurfaceContact);
			Physics.CustomTileIsNotSolidCondition = (Tile tile) => {
				// Disable collisions with owner
				return (tile == null || tile != tileOwner);
			};

			// Interactions
			Interactions.Enable();
			Interactions.SetReaction(InteractionType.Sword,			Deflect);
			Interactions.SetReaction(InteractionType.SwordSpin,		Deflect);
			Interactions.SetReaction(InteractionType.BiggoronSword,	Deflect);
			Interactions.SetReaction(InteractionType.Shield,		Deflect);
			Interactions.SetReaction(InteractionType.PlayerContact,
				delegate(Entity sender, EventArgs args)
			{
				OnCollidePlayer(RoomControl.Player);
			});

			// Projectile
			syncAnimationWithAngle		= false;
			syncAnimationWithDirection	= false;
			owner			= null;
			tileOwner		= null;
			eventCollision	= null;
			eventLand		= null;
			angle			= 0;
			direction		= 0;
			crashAnimation	= null;
			bounceOnCrash	= false;
			projectileType	= ProjectileType.Physical;
		}

		public void Deflect(Entity sender, EventArgs args) {
			WeaponInteractionEventArgs weaponArgs = (WeaponInteractionEventArgs) args;
			if (projectileType == ProjectileType.NotDeflectable)
				return;
			if (weaponArgs.Weapon.Level == Item.Level2)
				return;
			Deflect();
		}


		//-----------------------------------------------------------------------------
		// Virtual Methods
		//-----------------------------------------------------------------------------

		public virtual void Deflect() {
			if (projectileType != ProjectileType.NotDeflectable) {
				AudioSystem.PlaySound(GameData.SOUND_KEY_BOUNCE);
				Intercept();
			}
		}

		public virtual void Intercept() {
			Destroy();
		}

		protected virtual void OnCrash() { }


		//-----------------------------------------------------------------------------
		// Projectile Methods
		//-----------------------------------------------------------------------------

		protected void Crash(bool rebounded = false) {
			if (crashAnimation != null) {
				// Create crash effect.
				Effect effect;

				if (bounceOnCrash) {
					effect = new Effect();
					effect.CreateDestroyTimer(32);
					effect.Physics.Enable(PhysicsFlags.HasGravity);
					if (rebounded)
						effect.Physics.Velocity = Angle.Reverse().ToVector(0.25f);
					effect.Physics.ZVelocity	= 1.0f;
					effect.Physics.Gravity		= 0.07f;
					effect.Graphics.PlayAnimation(crashAnimation);
				}
				else {
					effect = new Effect(crashAnimation, Graphics.DepthLayer);
				}
				
				effect.Graphics.IsShadowVisible = false;
				effect.Graphics.DepthLayer = Graphics.DepthLayer;

				RoomControl.SpawnEntity(effect, position);
				DestroyAndTransform(effect);
			}
			else {
				Destroy();
			}

			OnCrash();
		}


		//-----------------------------------------------------------------------------
		// Virtual Methods
		//-----------------------------------------------------------------------------

		public virtual void OnCollideSolid(Collision collision) { }

		public virtual void OnCollideMonster(Monster monster) { }

		public virtual void OnCollidePlayer(Player player) { }


		//-----------------------------------------------------------------------------
		// Overridden Methods
		//-----------------------------------------------------------------------------

		public override void OnLand() {
			if (eventLand != null)
				eventLand();
		}

		public override void Initialize() {
			base.Initialize();

			if (owner is Player) {
				// Disable interactions with the player
				Reactions[InteractionType.Sword].Clear();
				Reactions[InteractionType.SwordSpin].Clear();
				Reactions[InteractionType.BiggoronSword].Clear();
				Reactions[InteractionType.Shield].Clear();
				Reactions[InteractionType.PlayerContact].Clear();
			}

			if (syncAnimationWithDirection)
				Graphics.SubStripIndex = direction;
			else if (syncAnimationWithAngle)
				Graphics.SubStripIndex = angle;
			
			tileLocation = new Point2I(-1, -1);
		}

		public override void Update() {
			Interactions.InteractionBox = Physics.SoftCollisionBox;

			// Check if collided
			if (physics.IsColliding) {
				eventCollision?.Invoke();
				if (IsDestroyed)
					return;

				Collision solidCollision = physics.GetCollisionInDirection(direction);
				if (solidCollision == null && Physics.Collisions.Any())
					solidCollision = Physics.Collisions.First();

				if (solidCollision.SolidObject != owner &&
					solidCollision.SolidObject != tileOwner)
				{
					OnCollideSolid(solidCollision);
					if (IsDestroyed)
						return;
				}
			}

			// Notify surface tiles the projectile is hovering over.
			Point2I tileLoc = RoomControl.GetTileLocation(position);
			if (tileLoc != tileLocation && RoomControl.IsTileInBounds(tileLoc) && zPosition < 10.0f) { // TODO: magic number
				Tile tile = RoomControl.GetTopTile(tileLoc);
				if (tile != null) {
					tile.OnHitByProjectile(this);
					if (IsDestroyed)
						return;
				}
			}
			tileLocation = tileLoc;

			/*
			if (owner is Player) {
				// Collide with monster tools.
				foreach (Monster monster in RoomControl.GetEntitiesOfType<Monster>()) {
					if (monster.IsPassable)
						continue;
					foreach (UnitTool tool in monster.EquippedTools) {
						if (Physics.PositionedCollisionBox.Intersects(tool.PositionedCollisionBox)) {
							tool.OnHitProjectile(this);
							if (IsDestroyed)
								return;
						}
					}
				}

				// Collide with monsters.
				foreach (Monster monster in Physics.GetEntitiesMeeting<Monster>(CollisionBoxType.Soft)) {
					if (monster.IsPassable)
						continue;
					OnCollideMonster(monster);
					if (IsDestroyed)
						return;
				}
			}
			else {
				Player player = RoomControl.Player;
				
				
				// Collide with the player's tools.
				//foreach (UnitTool tool in player.EquippedTools) {
				//	if (Physics.PositionedCollisionBox.Intersects(tool.PositionedCollisionBox)) {
				//		tool.OnHitProjectile(this);
				//		if (IsDestroyed)
				//			return;
				//	}
				//}
				
				// Collide with the player.
				//if (!player.IsPassable && Physics.IsMeetingEntity(player, CollisionBoxType.Soft)) {
				//	OnCollidePlayer(player);
				//	if (IsDestroyed)
				//		return;
				//}
			}
			*/

			// Collide with solid entities
			//foreach (Entity entity in Physics.GetSolidEntitiesMeeting<Entity>(CollisionBoxType.Hard)) {
			//	OnCollideSolidEntity(entity);
			//	if (IsDestroyed)
			//		return;
			//}
			


			if (syncAnimationWithDirection)
				Graphics.SubStripIndex = direction;
			else if (syncAnimationWithAngle)
				Graphics.SubStripIndex = angle;
			
			base.Update();
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public Angle Angle {
			get { return angle; }
			set {
				angle = value;
				if (angle.IsAxisAligned)
					direction = angle.ToDirection();
			}
		}
		
		public Direction Direction {
			get { return direction; }
			set {
				direction = value;
				angle = direction.ToAngle();
			}
		}

		public Entity Owner {
			get { return owner; }
			set { owner = value; }
		}

		public Tile TileOwner {
			get { return tileOwner; }
			set { tileOwner = value; }
		}

		public ProjectileType ProjectileType {
			get { return projectileType; }
			set { projectileType = value; }
		}

		public event Action EventCollision {
			add { eventCollision += value; }
			remove { eventCollision -= value; }
		}

		public event Action EventLand {
			add { eventLand += value; }
			remove { eventLand -= value; }
		}
	}
}
