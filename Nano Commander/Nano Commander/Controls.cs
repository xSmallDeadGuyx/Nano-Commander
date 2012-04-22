using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Nano_Commander {
	public class Controls {

		public static bool leftPressed {
			get {
				KeyboardState kb = Keyboard.GetState();
				return kb.IsKeyDown(leftKey);
			}
		}
		public static bool rightPressed {
			get {
				KeyboardState kb = Keyboard.GetState();
				return kb.IsKeyDown(rightKey);
			}
		}
		public static bool upPressed {
			get {
				KeyboardState kb = Keyboard.GetState();
				return kb.IsKeyDown(upKey);
			}
		}
		public static bool downPressed {
			get {
				KeyboardState kb = Keyboard.GetState();
				return kb.IsKeyDown(downKey);
			}
		}
		public static bool screenshotPressed {
			get {
				KeyboardState kb = Keyboard.GetState();
				return kb.IsKeyDown(screenshotKey);
			}
		}

		public static Keys leftKey = Keys.A;
		public static Keys rightKey = Keys.D;
		public static Keys upKey = Keys.W;
		public static Keys downKey = Keys.S;
		public static Keys screenshotKey = Keys.F2;
	}
}
