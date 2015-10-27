﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Content;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Input;
using ZeldaOracle.Game.Main;

namespace ZeldaOracle.Game.Entities.Units {

	// NOTE: These will probably be changed
	[Flags]
	public enum UnitFlags {
		Hurt,
		Bumpable,
		InGrass,
		Passable,
		FallInHoles,
	}

	
	public class Unit : Entity {
		
		// List of tools that the unit is carrying/holding.
		private HashSet<UnitTool> tools;

		// The direction the unit is facing.
		protected int		direction;
		protected bool		syncAnimationWithDirection;

		protected int		health;
		protected int		healthMax;
		
		protected float		knockbackSpeed;
		protected int		knockbackDuration;
		protected int		hurtInvincibleDuration;
		protected int		hurtFlickerDuration;
		
		protected int		hurtKnockbackDuration;
		protected int		bumpKnockbackDuration;

		private int			knockbackTimer;
		private int			invincibleTimer;
		private int			hurtFlickerTimer;
		private Vector2F	knockbackVelocity;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------
		
		public Unit() {
			EnablePhysics();

			knockbackSpeed			= GameSettings.UNIT_KNOCKBACK_SPEED;
			knockbackDuration		= GameSettings.UNIT_KNOCKBACK_DURATION;
			hurtInvincibleDuration	= GameSettings.UNIT_HURT_INVINCIBLE_DURATION;
			hurtFlickerDuration		= GameSettings.UNIT_HURT_FLICKER_DURATION;

			knockbackTimer			= 0;
			hurtFlickerTimer		= 0;
			invincibleTimer			= 0;
			knockbackVelocity		= Vector2F.Zero;
			tools					= new HashSet<UnitTool>();
			
			syncAnimationWithDirection = false;
			direction		= Directions.Right;
			health			= 1;
			healthMax		= 1;
			direction		= Directions.Right;
			centerOffset	= new Point2I(8, 8);
		}


		//-----------------------------------------------------------------------------
		// Tools
		//-----------------------------------------------------------------------------

		public bool IsToolEquipped(UnitTool tool) {
			return tools.Contains(tool);
		}

		public void EquipTool(UnitTool tool) {
			if (tools.Add(tool)) {
				tool.Unit = this;
				tool.IsEquipped = true;
				tool.OnEquip();
			}
		}
		
		public void UnequipTool(UnitTool tool) {
			if (tools.Remove(tool)) {
				tool.IsEquipped = false;
				tool.OnUnequip();
			}
		}


		//-----------------------------------------------------------------------------
		// Virtual methods
		//-----------------------------------------------------------------------------
		
		public void Knockback(int duration, float speed, Vector2F sourcePosition) {
			knockbackDuration	= duration;
			knockbackTimer		= duration;
			knockbackVelocity	= (Center - sourcePosition).Normalized;
			knockbackVelocity  *= speed;
			knockbackVelocity	= Vector2F.SnapDirectionByCount(
				knockbackVelocity, GameSettings.UNIT_KNOCKBACK_ANGLE_SNAP_COUNT);
		}
		
		public void Bump(Vector2F sourcePosition) {
			Knockback(bumpKnockbackDuration, knockbackSpeed, sourcePosition);
		}

		public void Hurt(int damage) {
			Hurt(new DamageInfo(damage));
		}
		
		public void Hurt(int damage, Vector2F sourcePosition) {
			Hurt(new DamageInfo(damage, sourcePosition));
		}

		public void Hurt(DamageInfo damage) {
			if (IsInvincible || IsBeingKnockedBack)
				return;

			// Knockback.
			if (damage.ApplyKnockBack) {
				Vector2F damageSourcePos = Center;
				int duration = hurtKnockbackDuration;
				if (damage.HasSource)
					damageSourcePos = damage.SourcePosition;
				if (damage.KnockbackDuration >= 0)
					duration = damage.KnockbackDuration;
				Knockback(duration, knockbackSpeed, damageSourcePos);
			}

			// Damage.
			if (damage.Amount > 0) {
				health				= GMath.Max(0, health - damage.Amount);
				invincibleTimer		= hurtInvincibleDuration;
				if (damage.Flicker) {
					hurtFlickerTimer = hurtFlickerDuration;
					graphics.IsHurting = true;
				}
			}
		}

		public virtual void RespawnDeath() {
			
		}

		public virtual void Death() {

		}

		public virtual void Die() {
			Destroy();
		}


		public virtual void OnKnockbackEnd() {
			physics.Velocity = Vector2F.Zero;
		}


		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------
		
		public override void Initialize() {
			base.Initialize();
		}

		public override void Update() {
			
			// Update knockback.
			if (knockbackTimer > 0) {
				knockbackTimer--;
				if (knockbackTimer == 0)
					OnKnockbackEnd();
			}
			if (IsBeingKnockedBack)
				Physics.Velocity = knockbackVelocity; // TODO: player can move while being knocked back.
			
			// Update hurt flickering.
			if (hurtFlickerTimer > 0)
				hurtFlickerTimer--;
			Graphics.IsHurting = IsHurtFlickering;
			
			// Update invinciblity timer.
			if (invincibleTimer > 0) {
				invincibleTimer--;
				if (invincibleTimer == 0) {
					if (health == 0) {
						Die();
					}
				}
			}

			// Update tools.
			foreach (UnitTool tool in tools)
				tool.Update();
			
			if (syncAnimationWithDirection)
				Graphics.SubStripIndex = direction;

			base.Update();
		}

		public override void Draw(RoomGraphics g) {
			DepthLayer depthLayer = Graphics.CurrentDepthLayer;

			// Draw tools under.
			foreach (UnitTool tool in tools) {
				if (!tool.DrawAboveUnit) {
					Vector2F drawPosition = position - new Vector2F(0, zPosition) + Graphics.DrawOffset + tool.DrawOffset;
					g.DrawAnimation(tool.AnimationPlayer, drawPosition, depthLayer);
				}
			}

			// Draw entity.
			base.Draw(g);

			// Draw tools over.
			foreach (UnitTool tool in tools) {
				if (tool.DrawAboveUnit) {
					Vector2F drawPosition = position - new Vector2F(0, zPosition) + Graphics.DrawOffset + tool.DrawOffset;
					g.DrawAnimation(tool.AnimationPlayer, drawPosition, depthLayer);
				}
			}
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public int Health {
			get { return health; }
			set { health = GMath.Clamp(value, 0, healthMax); }
		}
		
		public int MaxHealth {
			get { return healthMax; }
			set { healthMax = GMath.Max(value, 0); }
		}

		public bool IsAtFullHealth {
			get { return (health == healthMax); }
		}

		public bool IsBeingKnockedBack {
			get { return (knockbackTimer > 0); }
		}
		
		public bool IsInvincible {
			get { return (invincibleTimer > 0); }
		}
		
		public bool IsHurtFlickering {
			get { return (hurtFlickerTimer > 0); }
		}
		
		public int Direction {
			get { return direction; }
			set {
				direction = value;
				if (syncAnimationWithDirection)
					graphics.SubStripIndex = direction;
			}
		}
		
		public bool SyncAnimationWithDirection {
			get { return syncAnimationWithDirection; }
			set { syncAnimationWithDirection = value; }
		}
		
		public HashSet<UnitTool> Tools {
			get { return tools; }
		}
	}
}