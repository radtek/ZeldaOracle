﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeldaOracle.Common.Graphics {
	
	// Modes for how looping should be handled for an animation.
	public enum LoopMode {
		Clamp,	// Remain on the last frame when completed.
		Reset,	// Reset back to the beginning and stop.
		Repeat,	// Keep looping back and playing from the beginning endlessly.
	}


	public class Animation {

		// The list of frames.
		private List<AnimationFrame> frames;
		// Duratin in ticks.
		private int duration;
		// This creates a linked list of animations for its variations (like for different directions).
		private Animation nextStrip;
		// How looping should be handled.
		private LoopMode loopMode;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		public Animation() {
			this.frames		= new List<AnimationFrame>();
			this.duration	= 0;
			this.nextStrip	= null;
			this.loopMode	= LoopMode.Repeat;
		}


		//-----------------------------------------------------------------------------
		// Mutators
		//-----------------------------------------------------------------------------

		public void AddFrame(AnimationFrame frame) {
			int index = 0;
			while (index < frames.Count && frame.StartTime > frames[index].StartTime)
				++index;
			frames.Insert(index, frame);
			duration = Math.Max(duration, frame.StartTime + frame.Duration);
		}

		public void AddFrame(int startTime, int duration, Sprite sprite) {
			AddFrame(new AnimationFrame(startTime, duration, sprite));
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public AnimationFrame this[int index] {
			get { return frames[index]; }
		}

		public int Duration {
			get { return duration; }
			set { duration = value; }
		}

		public List<AnimationFrame> Frames {
			get { return frames; }
			set { frames = value; }
		}

		public Animation NextStrip {
			get { return nextStrip; }
			set { nextStrip = value; }
		}

		public LoopMode LoopMode {
			get { return loopMode; }
			set { loopMode = value; }
		}
	}
}
