﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Audio;
using ZeldaOracle.Common.Content;
using ZeldaOracle.Common.Content.ResourceBuilders;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Game;

namespace ZeldaOracle.Common.Scripts {

	public class SoundsSR : NewScriptReader {

		private TemporaryResources resources;
		private bool useTemporary;


		//-----------------------------------------------------------------------------
		// Override
		//-----------------------------------------------------------------------------

		public SoundsSR(TemporaryResources resources = null) {

			this.resources		= resources;
			this.useTemporary	= resources != null;

			// Sound <name> <path> <volume=1> <pitch=0> <pan=0> <muted=false>
			AddCommand("Sound",
				"string name, string path, float volume = 1, float pitch = 0, float pan = 0, bool muted = false",
			delegate(CommandParam parameters) {
				string name = parameters.GetString(0);
				string path = parameters.GetString(1);
				Sound sound	= Resources.LoadSound(name, Resources.SoundDirectory + path);
				sound.name	= name;
				if (parameters.ChildCount > 2)
					sound.Volume = parameters.GetFloat(2);
				if (parameters.ChildCount > 3)
					sound.Pitch = parameters.GetFloat(3);
				if (parameters.ChildCount > 4)
					sound.Pan = parameters.GetFloat(4);
				if (parameters.ChildCount > 5)
					sound.IsMuted = parameters.GetBool(5);
			});
		}

		//-----------------------------------------------------------------------------
		// Overridden Methods
		//-----------------------------------------------------------------------------

		// Begins reading the script.
		protected override void BeginReading() {
		}

		// Ends reading the script.
		protected override void EndReading() {
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public bool UseTemporaryResources {
			get { return useTemporary; }
			set { useTemporary = value; }
		}
	}
}