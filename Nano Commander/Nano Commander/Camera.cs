using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nano_Commander {
	public class Camera {
		public Game game;

		private Vector2 pos;
		public Vector2 position {
			get { return pos; }
			set { pos = positionToLimit(value); }
		}

		public Vector2 positionToLimit(Vector2 pos) {
			if(game.playingField == null) return pos;
			float x = Math.Min(Math.Max(pos.X, 0), game.playingField.mapData.GetLength(0) * 16 - 16 - game.windowWidth);
			float y = Math.Min(Math.Max(pos.Y, 0), game.playingField.mapData.GetLength(1) * 16 - 16 - game.windowHeight + game.playingField.gameOverlay.height);
			return new Vector2(x, y);
		}

		public Camera(Game g) {
			game = g;
			position = new Vector2(0, 0);
		}

		public bool IsOnScreen(Texture2D tex, Vector2 pos) {
			Vector2 finalPos = pos - position;
			return finalPos.X + tex.Width >= 0 && finalPos.Y + tex.Height >= 0 && finalPos.X <= game.windowWidth && finalPos.Y <= game.windowHeight;
		}

		public bool IsOnScreen(Rectangle rect, Vector2 pos) {
			Vector2 finalPos = pos - position;
			return finalPos.X + rect.Width >= 0 && finalPos.Y + rect.Height >= 0 && finalPos.X <= game.windowWidth && finalPos.Y <= game.windowHeight;
		}

		public bool IsOnScreen(Rectangle r) {
			Rectangle rect = new Rectangle(r.X - (int) position.X, r.Y - (int) position.Y, r.Width, r.Height);
			return rect.X + rect.Width >= 0 && rect.Y + rect.Height >= 0 && rect.X <= game.windowWidth && rect.Y <= game.windowHeight;
		}

		public void Draw(Texture2D tex, Vector2 pos, Color col) {
			if(IsOnScreen(tex, pos)) game.spriteBatch.Draw(tex, pos - position, col);
		}

		public void Draw(Texture2D tex, Vector2 pos, Rectangle rect, Color col) {
			if(IsOnScreen(rect, pos)) game.spriteBatch.Draw(tex, pos - position, rect, col);
		}

		public void Draw(Texture2D tex, Rectangle rect, Color col) {
			if(IsOnScreen(rect)) game.spriteBatch.Draw(tex, new Rectangle(rect.X - (int) position.X, rect.Y - (int) position.Y, rect.Width, rect.Height), col);
		}
	}
}
