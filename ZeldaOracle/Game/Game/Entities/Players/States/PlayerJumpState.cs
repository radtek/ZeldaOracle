﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Content;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Input;
using ZeldaOracle.Game.Main;
using ZeldaOracle.Game.Entities.Effects;
using ZeldaOracle.Game.Entities.Projectiles;
using ZeldaOracle.Game.Entities.Players;
using ZeldaOracle.Game.Items;
using ZeldaOracle.Game.Items.Weapons;
using ZeldaOracle.Game.Control;
using ZeldaOracle.Game.Tiles;

namespace ZeldaOracle.Game.Entities.Players {
	public class PlayerJumpState : PlayerState {


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		public PlayerJumpState() {
			isNaturalState = true;
		}
		

		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------

		public override void OnBegin() {
			base.OnBegin();
			
			if (player.IsOnGround) {
				// Jump!
				player.Physics.ZVelocity = GameSettings.PLAYER_JUMP_SPEED;
				player.Graphics.PlayAnimation(GameData.ANIM_PLAYER_JUMP);
			}
			else {
				player.Graphics.PlayAnimation(GameData.ANIM_PLAYER_DEFAULT);
			}
		}
		
		public override void OnEnd() {
			base.OnEnd();
		}

		public override void Update() {
			base.Update();

			// Only allow movement control on the descent.
			player.Movement.AllowMovementControl = (player.Physics.ZVelocity < 0.1f);

			// Update items
			Player.UpdateEquippedItems();
		}
	}
}