﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Input;
using ZeldaOracle.Game.Entities;
using ZeldaOracle.Game.Entities.Projectiles;
using ZeldaOracle.Game.Entities.Effects;
using ZeldaOracle.Game.Entities.Players;

namespace ZeldaOracle.Game.Items.Weapons {
	public class ItemFeather : ItemWeapon {


		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public ItemFeather(int level = 0) {
			this.id				= "item_feather";
			this.name			= new string[] { "Roc's Feather", "Roc's Cape" };
			this.description	= new string[] { "A nice lift.", "A wing-riding cape." };
			this.level			= level;
			this.maxLevel		= Item.Level2;
			this.flags			= ItemFlags.UsableWhileJumping | ItemFlags.UsableWithSword;

			sprite = new Sprite[] {
				GameData.SPR_ITEM_ICON_FEATHER,
				GameData.SPR_ITEM_ICON_CAPE
			};
		}


		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------

		public override bool IsUsable() {
			return Player.Movement.CanJump;
		}

		// Deploy cape.
		public override void OnButtonDown() {
			if (level == ItemWeapon.Level2) {
				// TODO: deploy cape.
			}
		}

		// Jump/deploy cape.
		public override void OnButtonPress() {
			if (Player.IsOnGround && !Player.Physics.IsInHole) {
				Player.Movement.Jump();
			}
			else if (level == ItemWeapon.Level2) {
				// TODO: deploy cape.
			}
		}

		// Draws the item inside the inventory.
		public override void DrawSlot(Graphics2D g, Point2I position, int lightOrDark) {
			DrawSprite(g, position, lightOrDark);
			DrawLevel(g, position, lightOrDark);
		}

	}
}
