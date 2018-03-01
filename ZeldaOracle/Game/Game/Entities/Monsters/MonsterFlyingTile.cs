﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeldaOracle.Common.Audio;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Game.Entities.Effects;

namespace ZeldaOracle.Game.Entities.Monsters {
	public class MonsterFlyingTile : Monster {

		private int launchTimer;
		private Vector2F launchVector;

		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public MonsterFlyingTile() {
			// General.
			MaxHealth       = 1;
			ContactDamage   = 2;
			Color           = MonsterColor.DarkRed;

			// Physics
			Physics.CollisionBox			= new Rectangle2F(0, -6, 1, 1);
			Physics.SoftCollisionBox		= new Rectangle2F(-4, -10, 8, 8);
			Physics.AutoDodges				= false;
			Physics.HasGravity				= false;
			Physics.IsDestroyedInHoles		= false;
			physics.DisableSurfaceContact	= true;

			// Graphics
			Graphics.DepthLayer         = DepthLayer.Monsters;
			Graphics.DepthLayerInAir    = DepthLayer.Monsters;
			
			// Monster & unit settings
			isKnockbackable			= false;
			isStunnable				= false;
			isGaleable				= false;
			isBurnable				= false;
			ignoreZPosition			= true;

			// Weapon interations
			Interactions.SetReaction(InteractionType.Sword,			SenderReactions.Intercept, Reactions.Kill);
			Interactions.SetReaction(InteractionType.SwordSpin,		Reactions.Kill);
			Interactions.SetReaction(InteractionType.BiggoronSword,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.Shield,			SenderReactions.Bump,		Reactions.Kill);
			// Seed interations
			Interactions.SetReaction(InteractionType.EmberSeed,		SenderReactions.Destroy,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.ScentSeed,		SenderReactions.Destroy,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.PegasusSeed,	SenderReactions.Destroy,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.GaleSeed,		SenderReactions.Intercept,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.MysterySeed,	SenderReactions.Destroy,	Reactions.Kill);
			// Projectile interations
			Interactions.SetReaction(InteractionType.Arrow,			SenderReactions.Destroy,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.SwordBeam,		SenderReactions.Intercept,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.RodFire,		SenderReactions.Destroy,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.Boomerang,		SenderReactions.Intercept,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.SwitchHook,		SenderReactions.Intercept,	Reactions.Kill);
			// Environment interations
			Interactions.SetReaction(InteractionType.Fire,			Reactions.Kill);
			Interactions.SetReaction(InteractionType.Gale,			Reactions.Kill);
			Interactions.SetReaction(InteractionType.BombExplosion,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.ThrownObject,	Reactions.Kill);
			Interactions.SetReaction(InteractionType.MineCart,		Reactions.Kill);
			Interactions.SetReaction(InteractionType.Block,			Reactions.Kill);
		}


		//-----------------------------------------------------------------------------
		// Overridden Methods
		//-----------------------------------------------------------------------------

		public override void Initialize() {
			base.Initialize();

			launchTimer = 0;

			// Begin rising
			Physics.ZVelocity = GameSettings.MONSTER_FLYING_TILE_RISE_SPEED;

			// Target the player
			Vector2F vectorToPlayer = RoomControl.Player.Position - Center;
			int launchAngleCount = GameSettings.MONSTER_FLYING_TILE_LAUNCH_ANGLE_COUNT;
			int launchAngle = Orientations.NearestFromVector(vectorToPlayer, launchAngleCount);
			launchVector = Orientations.ToVector(launchAngle, launchAngleCount);
			launchVector *= GameSettings.MONSTER_FLYING_TILE_LAUNCH_SPEED;
			
			Graphics.PlayAnimation(GameData.ANIM_MONSTER_FLYING_TILE);
		}

		public override void UpdateAI() {
			if (ZPosition < GameSettings.MONSTER_FLYING_TILE_HOVER_ALTITUDE) {
				Physics.ZVelocity = GameSettings.MONSTER_FLYING_TILE_RISE_SPEED;
			}
			else {
				Physics.ZVelocity = 0f;
			}
			if (launchTimer >= GameSettings.MONSTER_FLYING_TILE_HOVER_DURATION) {
				Physics.Velocity = launchVector;

				// Kill when colliding with solid objects
				if (Physics.IsColliding) {
					Kill();
					return;
				}
			}
			launchTimer++;
		}

		public override void OnTouchPlayer(Entity sender, EventArgs args) {
			base.OnTouchPlayer(sender, args);
			Kill();
		}

		public override void OnHurt(DamageInfo damage) {
			Kill();
		}

		public override void CreateDeathEffect() {
			Effect effect = new Effect(
				GameData.ANIM_EFFECT_ROCK_BREAK,
				DepthLayer.EffectTileBreak);
			AudioSystem.PlaySound(GameData.SOUND_ROCK_SHATTER);
			RoomControl.SpawnEntity(effect, Center);
		}
	}
}
