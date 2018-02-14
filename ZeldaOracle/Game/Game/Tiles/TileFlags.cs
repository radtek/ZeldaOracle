﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ZeldaOracle.Game.Tiles {

	public enum TileSolidType {
		NotSolid = 0,
		Solid,
		/// <summary>Not solid to units, and certain projectiles can pass over half solids. Half solids include railings.</summary>
		HalfSolid,
		/// <summary>A ledge the player can jump off of.</summary>
		Ledge,
		/// <summary>A ledge where the player can leap to an opposite leap ledge.</summary>
		LeapLedge,
		/// <summary>A ledge that affects altitude but cannot be jumped off of by the player.</summary>
		BasicLedge,
	}
	
	public enum TileEnvironmentType {
		Normal = 0,
		Stairs,
		Ladder,
		Ice,
		Puddle,
		Grass,
		Hole,
		Water,
		DeepWater,	// (+Water)
		Ocean,		// (+Water)
		Waterfall,
		Lava,
		Lavafall,
		Whirlpool,	// (+Water +Hole)
	}

	public enum TileResetCondition {
		LeaveRoom = 0,	// Tile resets its state upon leaving the room (default).
		LeaveArea,		// Tile resets its state upon leaving the area.
		Never,			// Tile's state is persistant and will never be reset.
	}

	public struct TileSpawnOptions {
		public bool PoofEffect { get; set; }
		public int SpawnDelayAfterPoof { get; set; }
	}

	[Flags]
	public enum TileFlags {
		[Browsable(false)]
		None			= 0,
		[Browsable(false)]
		/// <summary>Default tile flags (Flags are designed so this value is 0)</summary>
		Default			= 0,
				
		// General.
		/// <summary>Tile can be pushed around.</summary>
		Movable			= 0x1,
		/// <summary>Tile can be picked up and carried.</summary>
		Pickupable		= 0x2,
		/// <summary>Destroyed by fire.</summary>
		Burnable		= 0x4,
		/// <summary>Destroyed by sword.</summary>
		Cuttable		= 0x8,
		/// <summary>Destroyed by bomb explosions.</summary>
		Bombable		= 0x10,
		/// <summary>Destroyed by boomerang.</summary>
		Boomerangable	= 0x20,
		/// <summary>Can be switched with using the Switch Hook.</summary>
		Switchable		= 0x40,
		/// <summary>Won't be destroyed when switched using the Switch Hook.</summary>
		SwitchStays		= 0x80,
		/// <summary>Can be dug with a shovel.</summary>
		Digable			= 0x100,

		/// <summary>Tile can't be covered by movable blocks.</summary>
		NotCoverable	= 0x200,
		/// <summary>Player will not use the pushing animation when walking into the tile.</summary>
		NotPushable		= 0x400,

		// Item/player interactions.
		/// <summary>The player cannot grab the tile with the power bracelet.</summary>
		NotGrabbable	= 0x800,
		/// <summary>Seeds will be destroyed when hitting this tile instead of bouncing off.</summary>
		AbsorbSeeds		= 0x1000,
		/// <summary>The tile is picked up instantly, without having to pull on it.</summary>
		InstantPickup	= 0x2000,
		/// <summary>Hurt the player when he touches the tile.</summary>
		HurtPlayer		= 0x4000,
		/// <summary>The tile will be skipped when checking for surfaces.</summary>
		NotSurface		= 0x8000,
	}
}
