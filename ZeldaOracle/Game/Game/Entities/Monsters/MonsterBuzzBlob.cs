﻿using System;
using ZeldaOracle.Common.Geometry;

namespace ZeldaOracle.Game.Entities.Monsters {

	public class MonsterBuzzBlob : BasicMonster {
		
		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public MonsterBuzzBlob() {
			// General
			MaxHealth		= 1;
			ContactDamage	= 2;
			Color			= MonsterColor.Green;
			
			// Movement
			moveSpeed					= 0.25f;
			numMoveAngles				= 8;
			changeDirectionsOnCollide	= true;
			movesInAir					= false;
			facePlayerOdds				= 0;
			stopTime.Set(0, 0);
			moveTime.Set(50, 80);
			
			// Physics
			Physics.Bounces				= true; // This is here for when the monster is digged up or dropped from the ceiling
			Physics.ReboundRoomEdge		= true;
			Physics.ReboundSolid		= true;
			Physics.SoftCollisionBox	= new Rectangle2I(-4, -10, 8, 9);
			
			// Graphics
			animationMove				= GameData.ANIM_MONSTER_BUZZ_BLOB;
			syncAnimationWithDirection	= false;
			
			// Interactions (Tools)
			Interactions.SetReaction(InteractionType.Sword,			Reactions.Electrocute);
			Interactions.SetReaction(InteractionType.SwordSpin,		Reactions.Electrocute);
			Interactions.SetReaction(InteractionType.BiggoronSword,	Reactions.Electrocute);
			// Interactions (Projectiles)
			Interactions.SetReaction(InteractionType.Arrow,			SenderReactions.Intercept, Reactions.Damage);
			Interactions.SetReaction(InteractionType.SwordBeam,		SenderReactions.Intercept, Reactions.Kill);
			Interactions.SetReaction(InteractionType.SwitchHook,		SenderReactions.Intercept, Reactions.Electrocute);
			Interactions.SetReaction(InteractionType.MysterySeed,	SenderReactions.Intercept, TransformIntoCukeman);
			Interactions.SetReaction(InteractionType.BombExplosion,	Reactions.Kill);
		}


		//-----------------------------------------------------------------------------
		// Overridden Methods
		//-----------------------------------------------------------------------------

		private void TransformIntoCukeman(Entity sender, EventArgs args) {
			MonsterCukeman cukeman = new MonsterCukeman() {
				Position = position,
				ZPosition = ZPosition,
			};
			DestroyAndTransform(cukeman);
			RoomControl.SpawnEntity(cukeman);
		}

		public override void OnElectrocute() {
			Graphics.PlayAnimation(GameData.ANIM_MONSTER_BUZZ_BLOB_ELECTROCUTE);
		}

		public override void OnElectrocuteComplete() {
			StartMoving();
		}
	}
}
