﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Game.Tiles;
using ZeldaOracle.Game.Items.Weapons;

namespace ZeldaOracle.Game.Entities.Effects {
	public class EffectCreateSomariaBlock : Effect {

		private Point2I tileLocation;
		private ItemCane itemCane;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		public EffectCreateSomariaBlock(Point2I tileLocation, float zPosition, ItemCane itemCane) :
			base(GameData.ANIM_EFFECT_SOMARIA_BLOCK_CREATE)
		{
			this.itemCane		= itemCane;
			this.tileLocation	= tileLocation;
			this.position		= (tileLocation * GameSettings.TILE_SIZE) + new Vector2F(8, 8);
			this.zPosition		= zPosition;

			if (IsOnGround) {
				// Make this effect solid to the player (as if the somaria block has already been spawned).
				Physics.CollisionBox = new Rectangle2F(-8, -8, 16, 16);
				Physics.SoftCollisionBox = new Rectangle2F(-8, -8, 16, 16);
				EnablePhysics(PhysicsFlags.Solid);
			}
			
			Graphics.DepthLayer	= DepthLayer.EffectSomariaBlockPoof;
		}
		

		//-----------------------------------------------------------------------------
		// Internal methods
		//-----------------------------------------------------------------------------

		// Return true if a somaria block would break when spawned at the given tile location.
		private bool CanBlockSpawnAtLocation(Point2I location) {
			if (!RoomControl.IsTileInBounds(location))
				return false;
			Tile checkTile = RoomControl.GetTopTile(location);
			if (checkTile == null)
				return true;
			return (checkTile.Layer != RoomControl.Room.TopLayer &&
					checkTile.IsCoverableByBlock && !checkTile.IsHoleWaterOrLava);
		}
		

		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------

		public override void OnDestroy() {
			base.OnDestroy();

			if (IsOnGround && CanBlockSpawnAtLocation(tileLocation)) {
				// Create the somaria block.
				TileSomariaBlock tile = (TileSomariaBlock) Tile.CreateTile(itemCane.SomariaBlockTileData);
				RoomControl.PlaceTileOnHighestLayer(tile, tileLocation);
				itemCane.SomariaBlockTile = tile;
			}
			else {
				// Spawn a poof effect.
				RoomControl.SpawnEntity(
					new Effect(GameData.ANIM_EFFECT_SOMARIA_BLOCK_DESTROY, DepthLayer.EffectSomariaBlockPoof),
					position, zPosition);
			}
		}
	}
}