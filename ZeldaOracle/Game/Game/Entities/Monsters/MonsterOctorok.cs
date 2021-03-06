﻿using ZeldaOracle.Game.Entities.Projectiles.MonsterProjectiles;

namespace ZeldaOracle.Game.Entities.Monsters {

	public class MonsterOctorok : BasicMonster {
		
		public MonsterOctorok() {
			// General.
			MaxHealth		= 1;
			ContactDamage	= 1;
			Color			= MonsterColor.Blue;
			animationMove	= GameData.ANIM_MONSTER_OCTOROK;
			
			// Movement.
			moveSpeed					= 0.5f;
			changeDirectionsOnCollide	= true;
			syncAnimationWithDirection	= true;
			stopTime.Set(30, 60);
			moveTime.Set(30, 50);

			// Projectiles.
			projectileType		= typeof(OctorokRock);
			shootType			= ShootType.OnStop;
			aimType				= AimType.Forward;
			shootSpeed			= 2.0f;
			projectileShootOdds	= 5; // 1 in 5 chance to shoot after stopping.
			shootPauseDuration	= 30;
			
			// Interactions.
			Interactions.SetReaction(InteractionType.Sword, MonsterReactions.DamageByLevel(1, 2, 2));
			Interactions.SetReaction(InteractionType.BiggoronSword, MonsterReactions.Damage2);

			/*
			// DEBUG MONSTER SETTINGS.
			stopTime.Set(0, 0);
			moveTime.Set(30, 80);
			syncAnimationWithDirection	= true;
			isMovementDirectionBased	= true;
			numMoveAngles = 16;
			
			shootType = ShootType.None;

			Physics.ReboundRoomEdge	= true;
			Physics.ReboundSolid	= true;
			Physics.Bounces			= true;
			
			shootType = ShootType.WhileMoving;
			projectileShootOdds = 60; // once every 60 seconds.
			aimType = AimType.Forward;
			aimType = AimType.FaceRandom;
			
			Graphics.DrawOffset			= new Point2I(-8, -14) + new Point2I(0, 3);
			centerOffset				= new Point2I(0, -6) + new Point2I(0, 3);
			*/
		}

		public override void Initialize() {
			base.Initialize();

			if (Color == MonsterColor.Red) {
				MaxHealth		= 1;
				ContactDamage	= 1;
				aimType			= AimType.Forward;
			}
			else if (Color == MonsterColor.Blue) {
				MaxHealth		= 2;
				ContactDamage	= 2;
				aimType			= AimType.FacePlayer;
			}
			else if (Color == MonsterColor.Orange) {
				// Gold.
				MaxHealth		= 25; // Observations: 25 hits sword level 1, 11 hits with sword level 3.
				ContactDamage	= 4;
				aimType			= AimType.FacePlayer;
			}
		}
	}
}
