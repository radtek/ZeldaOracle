﻿using ZeldaOracle.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Properties;
using ZeldaOracle.Game.Tiles;
using ZeldaOracle.Game.Tiles.EventTiles;

namespace ZeldaOracle.Game.Worlds {
	public class Room {
		
		private Level							level;		// The level this room is in.
		private Point2I							location;	// Location within the level.
		private TileDataInstance[,,]			tileData;	// 3D grid of tile data (x, y, layer)
		private List<EventTileDataInstance>		eventData;
		private Zone							zone;
		private Properties						properties;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		public Room(Level level, int x, int y) {
			this.level		= level;
			this.location	= new Point2I(x, y);
			this.tileData	= new TileDataInstance[level.RoomSize.X, level.RoomSize.Y, level.RoomLayerCount];
			this.eventData	= new List<EventTileDataInstance>();
			this.zone		= null;
			this.properties	= new Properties();
		}
		

		//-----------------------------------------------------------------------------
		// Accessors
		//-----------------------------------------------------------------------------
		
		public TileDataInstance GetTile(Point2I location, int layer) {
			return tileData[location.X, location.Y, layer];
		}

		public TileDataInstance GetTile(int x, int y, int layer) {
			return tileData[x, y, layer];
		}


		//-----------------------------------------------------------------------------
		// Mutators
		//-----------------------------------------------------------------------------
		
		public void SetTile(TileDataInstance tile, int x, int y, int layer) {
			tileData[x, y, layer] = tile;
			tile.Location	= new Point2I(x, y);
			tile.Layer		= layer;
			tile.Room		= this;
		}

		public void RemoveTile(int x, int y, int layer) {
			tileData[x, y, layer] = null;
		}

		public TileDataInstance CreateTile(TileData data, Point2I location, int layer) {
			return CreateTile(data, location.X, location.Y, layer);
		}

		public TileDataInstance CreateTile(TileData data, int x, int y, int layer) {
			TileDataInstance dataInstance = new TileDataInstance(data, x, y, layer);
			dataInstance.Room = this;
			tileData[x, y, layer] = dataInstance;
			return dataInstance;
		}

		public EventTileDataInstance CreateEventTile(EventTileData data, int x, int y) {
			return CreateEventTile(data, new Point2I(x, y));
		}

		public EventTileDataInstance CreateEventTile(EventTileData data, Point2I position) {
			EventTileDataInstance dataInstance = new EventTileDataInstance(data, position);
			AddEventTile(dataInstance);
			return dataInstance;
		}
		
		public void AddEventTile(EventTileDataInstance eventTile) {
			eventData.Add(eventTile);
			eventTile.Room = this;
		}



		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public TileDataInstance[,,] TileData {
			get { return tileData; }
			set { tileData = value; }
		}

		public List<EventTileDataInstance> EventData {
			get { return eventData; }
			set { eventData = value; }
		}

		public Level Level {
			get { return level; }
			set { level = value; }
		}

		public Point2I Location {
			get { return location; }
			set { location = value; }
		}

		public Point2I Size {
			get { return level.RoomSize; }
		}

		public int Width {
			get { return level.RoomWidth; }
		}

		public int Height {
			get { return level.RoomHeight; }
		}

		public int LayerCount {
			get { return level.RoomLayerCount; }
		}

		public Zone Zone {
			get { return zone; }
			set { zone = value; }
		}

		public Properties Properties {
			get { return properties; }
			set { properties = value; }
		}
	}
}
