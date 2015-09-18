﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Game.Control;

namespace ZeldaOracle.Game.Items {
	public class Inventory {

		// The game control for the current game session.
		private GameControl gameControl;
		// The list of items in the game.
		private List<Item> items;
		// The list of ammos in the game.
		private List<Ammo> ammo;
		// The player's equip slots.
		private UsableItem[] equippedUsableItems;

		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		// The number of slots to equip player items.
		public const int NumEquipSlots = 2;

		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public Inventory(GameControl gameControl) {
			this.gameControl			= gameControl;
			this.items					= new List<Item>();
			this.ammo					= new List<Ammo>();
			this.equippedUsableItems	= new UsableItem[NumEquipSlots];
		}


		//-----------------------------------------------------------------------------
		// Items
		//-----------------------------------------------------------------------------

		// Equip a usable item into the given slot (slot 0 (A) or 1 (B)).
		public void EquipUsableItem(Item item, int slot) {
			UsableItem usableItem = item as UsableItem;
			if (usableItem.HasFlag(ItemFlags.TwoHanded)) {
				// Unequip the current items.
				if (equippedUsableItems[0] != null)
					equippedUsableItems[0].Unequip();
				if (equippedUsableItems[1] != null)
					equippedUsableItems[1].Unequip();

				equippedUsableItems[0] = usableItem;
				equippedUsableItems[1] = usableItem;
			}
			else {
				// Unequip the current item.
				if (equippedUsableItems[slot] != null)
					equippedUsableItems[slot].Unequip();

				equippedUsableItems[slot] = usableItem;
			}

			// Equip the new item.
			if (usableItem != null)
				usableItem.Equip();
		}

		// Equips a non-usable item.
		public void EquipEquippableItem(Item item) {
			EquippableItem equippableItem = item as EquippableItem;
		}

		// Adds the item to the list
		public void AddItem(Item item) {
			this.items.Add(item);
			item.OnAdded(this);
		}

		// Gets the item at the specified index
		public Item GetItem(int index) {
			return items[index];
		}

		// Gets the item with the specified id
		public Item GetItem(string id) {
			foreach (Item item in items) {
				if (item.ID == id)
					return item;
			}
			return null;
		}

		// Checks if the item exists
		public bool ItemExists(string id) {
			foreach (Item item in items) {
				if (item.ID == id)
					return true;
			}
			return false;
		}

		// Checks if the item has been obtained
		public bool IsItemObtained(string id) {
			foreach (Item item in items) {
				if (item.ID == id)
					return item.IsObtained;
			}
			return false;
		}

		// Checks if the item has been obtained and is not stolen
		public bool IsItemAvailable(string id) {
			foreach (Item item in items) {
				if (item.ID == id)
					return item.IsObtained && !item.IsStolen;
			}
			return false;
		}


		//-----------------------------------------------------------------------------
		// Ammo
		//-----------------------------------------------------------------------------

		// Adds the ammo type to the list.
		public void AddAmmo(Ammo ammo) {
			this.ammo.Add(ammo);
		}

		// Gets the ammo class with the specified id.
		public Ammo GetAmmo(string id) {
			foreach (Ammo ammo in this.ammo) {
				if (ammo.ID == id)
					return ammo;
			}
			return null;
		}

		// Checks if the ammo exists.
		public bool AmmoExists(string id) {
			foreach (Ammo ammo in this.ammo) {
				if (ammo.ID == id)
					return true;
			}
			return false;
		}

		// Checks if the ammo has been obtained.
		public bool IsAmmoObtained(string id) {
			foreach (Ammo ammo in this.ammo) {
				if (ammo.ID == id)
					return ammo.IsObtained;
			}
			return false;
		}

		// Checks if the ammo has been obtained and is not stolen.
		public bool IsAmmoAvailable(string id) {
			foreach (Ammo ammo in this.ammo) {
				if (ammo.ID == id)
					return ammo.IsObtained && !ammo.IsStolen;
			}
			return false;
		}

		// Fills all the ammo in the player's inventory.
		public void FillAllAmmo() {
			foreach (Ammo ammo in this.ammo) {
				ammo.Amount = ammo.MaxAmount;
			}
		}

		// Empties all the ammo from the player's inventory.
		public void EmptyAllAmmo() {
			foreach (Ammo ammo in this.ammo) {
				ammo.Amount = 0;
			}
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		// Gets the equipped usable items list.
		public UsableItem[] EquippedUsableItems {
			get { return equippedUsableItems; }
		}

		// Gets if a two handed weapon is equipped.
		public bool IsTwoHandedEquipped {
			get { return (equippedUsableItems[0] != null ? equippedUsableItems[0].HasFlag(ItemFlags.TwoHanded) : false); }
		}
	}
}
