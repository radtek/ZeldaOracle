﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Graphics.Sprites;
using ZeldaOracle.Game.Entities;

namespace ZeldaOracle.Game.Tiles {

	public class TileGraphicsComponent {
		
		private Tile				tile;						// The tile this component belongs to.
		private AnimationPlayer		animationPlayer;

		private	bool				isVisible;
		private DepthLayer			depthLayer;
		private Point2I				drawOffset;
		private Point2I				raisedDrawOffset;			// Offset to draw tiles that are slightly raised (EX: pushing pots onto a button)
		private	bool				syncPlaybackWithRoomTicks;
		private bool				isAnimatedWhenPaused;		// True if the tile updates its graphics while the room is paused.
		private Vector2F			absoluteDrawPosition;
		private bool				useAbsoluteDrawPosition;
		private ColorDefinitions	colors;
		/// <summary>Draws above the player/monsters when
		/// the player is above and vice-versa.</summary>
		private bool				useDynamicDepth;
		/// <summary>The y offset from the position of the tile to the origin.
		/// Only needed for dynamic depth.</summary>
		private int					dynamicOriginY;


		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public TileGraphicsComponent(Tile tile) {
			this.tile						= tile;
			this.animationPlayer			= new AnimationPlayer();
			this.isVisible					= true;
			this.depthLayer					= DepthLayer.TileLayer1;
			this.raisedDrawOffset			= Point2I.Zero;
			this.drawOffset					= Point2I.Zero;
			this.syncPlaybackWithRoomTicks	= true;
			this.isAnimatedWhenPaused		= false;
			this.absoluteDrawPosition		= Vector2F.Zero;
			this.useAbsoluteDrawPosition	= false;
			this.colors                     = new ColorDefinitions();
			this.useDynamicDepth			= false;
			this.dynamicOriginY				= 0;
		}

		
		//-----------------------------------------------------------------------------
		// Animation & Sprite Interface
		//-----------------------------------------------------------------------------

		public void PlayAnimation() {
			animationPlayer.Play();
		}

		public void PlayAnimation(ISprite sprite) {
			animationPlayer.Play(sprite);
		}
		
		public void SetAnimation(Animation animation) {
			animationPlayer.SetAnimation(animation);
		}
		
		public void StopAnimation() {
			animationPlayer.Stop();
		}
		
		public void PauseAnimation(bool isPaused) {
			animationPlayer.Pause(isPaused);
		}
		
		public void PauseAnimation() {
			animationPlayer.Pause();
		}
		
		public void ResumeAnimation() {
			animationPlayer.Resume();
		}
		
		public void ClearAnimation() {
			animationPlayer.Clear();
		}
		

		//-----------------------------------------------------------------------------
		// Draw Settings
		//-----------------------------------------------------------------------------

		public void SetAbsoluteDrawPosition(Vector2F drawPosition) {
			absoluteDrawPosition = drawPosition;
			useAbsoluteDrawPosition = true;
		}

		
		//-----------------------------------------------------------------------------
		// Update/Draw
		//-----------------------------------------------------------------------------

		public void Update() {
			// Update the animation player.
			if (tile.GameControl.AnimateRoom && (tile.GameControl.UpdateRoom || isAnimatedWhenPaused)) {
				animationPlayer.Update();
			}
		}

		public void Draw(RoomGraphics g) {
			if (!isVisible)
				return;

			// Determine draw position.
			Vector2F drawPosition = (useAbsoluteDrawPosition ?
				absoluteDrawPosition : tile.Position);

			// Determine the depth layer based on the tile grid layer.
			if (useDynamicDepth) {
				float playerY = Tile.RoomControl.Player.Position.Y;
				if (GMath.Round(playerY) < GMath.Round(drawPosition.Y + dynamicOriginY)) {
					if (tile.Layer == 2)
						depthLayer = DepthLayer.DynamicDepthAboveTileLayer3;
					else
						depthLayer = DepthLayer.DynamicDepthAboveTileLayer2;
				}
				else {
					if (tile.Layer == 2)
						depthLayer = DepthLayer.DynamicDepthBelowTileLayer3;
					else
						depthLayer = DepthLayer.DynamicDepthBelowTileLayer2;
				}
			}
			else if (tile.Layer == 0)
				depthLayer = DepthLayer.TileLayer1;
			else if (tile.Layer == 1)
				depthLayer = DepthLayer.TileLayer2;
			else if (tile.Layer == 2)
				depthLayer = DepthLayer.TileLayer3;

			Draw(g, depthLayer);
		}

		public void Draw(RoomGraphics g, DepthLayer depthLayer) {
			if (!isVisible)
				return;

			// Determine draw position.
			Vector2F drawPosition = (useAbsoluteDrawPosition ?
				absoluteDrawPosition : tile.Position);

			drawPosition += (raisedDrawOffset + drawOffset);

			// Draw the tile's as-object sprite.
			if (tile.IsMoving && tile.SpriteAsObject != null) {
				g.DrawSprite(tile.SpriteAsObject, new SpriteSettings(colors,
					tile.RoomControl.GameControl.RoomTicks), drawPosition, depthLayer, tile.Position);
			}
			// Draw the animation player.
			else {
				float playbackTime;
				if (syncPlaybackWithRoomTicks)
					playbackTime = tile.RoomControl.GameControl.RoomTicks;
				else
					playbackTime = animationPlayer.PlaybackTime;
				
				g.DrawAnimationPlayer(animationPlayer, playbackTime,
					new SpriteSettings(colors), drawPosition, depthLayer, tile.Position);
			}

		}

		public void DrawAbove(RoomGraphics g) {
			if (!isVisible)
				return;

			if (tile.TileData.TileData.SpriteAbove != null) {
				// Determine the depth layer based on the tile grid layer.
				if (tile.Layer == 0)
					depthLayer = DepthLayer.TileLayer1;
				else if (tile.Layer == 1)
					depthLayer = DepthLayer.TileLayer2;
				else if (tile.Layer == 2)
					depthLayer = DepthLayer.TileLayer3;

				// Determine draw position.
				Vector2F drawPosition = (useAbsoluteDrawPosition ?
					absoluteDrawPosition : tile.Position);
				drawPosition += (raisedDrawOffset + drawOffset);

				g.DrawSprite(tile.TileData.TileData.SpriteAbove,
					new SpriteSettings(colors,
						tile.RoomControl.GameControl.RoomTicks),
						drawPosition, DepthLayer.TileAbove, tile.Position);
			}
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public Tile Tile {
			get { return tile; }
		}

		// Animation Player -----------------------------------------------------------

		public AnimationPlayer AnimationPlayer {
			get { return animationPlayer; }
		}

		public int SubStripIndex {
			get { return animationPlayer.SubStripIndex; }
			set { animationPlayer.SubStripIndex = value; }
		}

		public bool IsAnimationDone {
			get { return animationPlayer.IsDone; }
		}

		// Graphics Settings ----------------------------------------------------------
		
		public bool IsVisible {
			get { return isVisible; }
			set { isVisible = value; }
		}

		public DepthLayer DepthLayer {
			get { return depthLayer; }
			set { depthLayer = value; }
		}

		public Point2I DrawOffset {
			get { return drawOffset; }
			set { drawOffset = value; }
		}

		public Point2I RaisedDrawOffset {
			get { return raisedDrawOffset; }
			set { raisedDrawOffset = value; }
		}
		
		public bool IsAnimatedWhenPaused {
			get { return isAnimatedWhenPaused; }
			set { isAnimatedWhenPaused = value; }
		}
		
		public Vector2F AbsoluteDrawPosition {
			get { return absoluteDrawPosition; }
			set { absoluteDrawPosition = value; }
		}
		
		public bool UseAbsoluteDrawPosition {
			get { return useAbsoluteDrawPosition; }
			set { useAbsoluteDrawPosition = value; }
		}
		
		public bool SyncPlaybackWithRoomTicks {
			get { return syncPlaybackWithRoomTicks; }
			set { syncPlaybackWithRoomTicks = value; }
		}

		public ColorDefinitions Colors {
			get { return colors; }
		}

		/// <summary>Gets if the t draws above the player/monsters
		/// when the player is above and vice-versa.</summary>
		public bool UseDynamicDepth {
			get { return useDynamicDepth; }
			set { useDynamicDepth = value; }
		}

		/// <summary>The y offset from the position of the tile to the origin.
		/// Only needed for dynamic depth.</summary>
		public int DynamicOriginY {
			get { return dynamicOriginY; }
			set { dynamicOriginY = value; }
		}
	}
}
