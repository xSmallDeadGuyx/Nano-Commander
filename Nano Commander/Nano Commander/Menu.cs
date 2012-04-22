using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Nano_Commander {
	public class MapData {
		public Vector2 position;
		public string name;

		public MapData(string s) {
			name = s;
			position = new Vector2(0, 0);
		}
	}

	public class Menu {

		public readonly Color[] playerCols = { Color.DarkRed, Color.IndianRed, Color.OrangeRed, Color.Orange, Color.LightGoldenrodYellow, Color.Yellow, Color.YellowGreen, Color.Lime, Color.ForestGreen, Color.DarkSeaGreen, Color.DeepSkyBlue, Color.Blue, Color.BlueViolet, Color.MediumPurple, Color.HotPink, Color.Indigo};

		public Game game;
		public Texture2D titleTex;
		public Texture2D squareTex;
		public Texture2D titleBgTex;
		public Texture2D completeTex;
		public Texture2D selectTex;

		public int playerCol = 8;

		public Dictionary<MapData, Texture2D> maps;

		int w = 128;
		int h = 64;

		public Vector2 center = new Vector2(0, 0);
		public int radius = 200;
		public int squishyness = 4;
		public float acceleration = 0.07F;
		
		private float targetSpeed = 0.0F;
		private float speed = 0.0F;
		private float rotation = 0.0F;

		public Dictionary<string, Texture2D> buildMapFromContent() {
			Dictionary<string, Texture2D> d = new Dictionary<string, Texture2D>();

			DirectoryInfo contentDir = new DirectoryInfo(game.Content.RootDirectory + "\\maps");
			foreach(FileInfo file in contentDir.EnumerateFiles()) {
				string s = "maps\\" + file.Name.Remove(file.Name.Length - 4);
				d.Add(s, game.Content.Load<Texture2D>(s));
			}

			return d;
		}


		public Menu(Game g) {
			game = g;

			titleTex = game.Content.Load<Texture2D>("title");
			squareTex = game.Content.Load<Texture2D>("square");
			titleBgTex = game.Content.Load<Texture2D>("titlebg");
			completeTex = game.Content.Load<Texture2D>("complete");
			selectTex = game.Content.Load<Texture2D>("select");

			Dictionary<string, Texture2D> m = buildMapFromContent();
			maps = new Dictionary<MapData, Texture2D>();
			for(int i = 0; i < m.Count; i++)
				maps.Add(new MapData(m.Keys.ElementAt<string>(i)), m.Values.ElementAt<Texture2D>(i));
		}

		public void updateMenu() {
			MouseState m = Mouse.GetState();
			float mx = m.X - center.X;
			float my = m.Y - center.Y;

			if(my > -radius / squishyness - h / 2&& my < radius / squishyness + h / 2&& mx > -radius - w / 2 && mx < radius + w / 2)
				targetSpeed = (5 * mx / (float) -radius);
			else targetSpeed = 0;

			speed = speed > targetSpeed + acceleration ? speed - acceleration : (speed < targetSpeed - acceleration ? speed + acceleration : targetSpeed);
			rotation += speed;
			for(int i = 0; i < maps.Count; i++) {
				float rot = ((360.0F / (float) maps.Count * i) - rotation) * (float) Math.PI / 180.0F;
				MapData md = maps.Keys.ElementAt<MapData>(i);
				md.position = new Vector2(radius * (float) Math.Cos(rot) + center.X, radius * (float) Math.Sin(rot) / (float) squishyness + center.Y);

				if(m.LeftButton == ButtonState.Pressed && m.X >= md.position.X - w / 2 && m.X < md.position.X + w / 2 && m.Y >= md.position.Y - h / 2 && m.Y < md.position.Y + h / 2)
					game.beginGame(md.name, playerCols[playerCol]);
			}

			if(m.LeftButton == ButtonState.Pressed && m.X >= game.windowWidth / 2 - playerCols.Length * 8 && m.X < game.windowWidth / 2 + playerCols.Length * 8 && m.Y >= game.windowHeight - 32 && m.Y < game.windowHeight - 16)
				if((m.X - game.windowWidth / 2 + playerCols.Length * 8) / 16 != playerCol) {
					playerCol = (m.X - game.windowWidth / 2 + playerCols.Length * 8) / 16;
					game.save();
				}
		}

		public void drawMenu() {
			MouseState m = Mouse.GetState();
			float minY = center.Y - radius / (float) squishyness;
			float height = 2 * radius / (float) squishyness;

			game.spriteBatch.Begin();
			
			game.spriteBatch.Draw(titleTex, new Vector2(0, 0), Color.White);
			game.spriteBatch.Draw(titleBgTex, new Vector2(0, 0), Color.White);

			for(int i = 0; i < playerCols.Length; i++)
				game.spriteBatch.Draw(squareTex, new Rectangle(game.windowWidth / 2 + i * 16 - playerCols.Length * 8, game.windowHeight - 32, 16, 16), playerCols[i]);

			game.spriteBatch.Draw(selectTex, new Vector2(playerCol * 16 + game.windowWidth / 2 - playerCols.Length * 8, game.windowHeight - 32), Color.Black);

			if(m.X >= game.windowWidth / 2 - playerCols.Length * 8 && m.X < game.windowWidth / 2 + playerCols.Length * 8 && m.Y >= game.windowHeight - 32 && m.Y < game.windowHeight - 16) {
				int i = (m.X - game.windowWidth / 2 + playerCols.Length * 8) / 16;
				game.spriteBatch.Draw(squareTex, new Rectangle(game.windowWidth / 2 - playerCols.Length * 8 + i * 16, game.windowHeight - 32, 16, 16), Color.White * 0.5F);
			}
			game.spriteBatch.End();
			game.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
			foreach(KeyValuePair<MapData, Texture2D> d in maps) {
				MapData md = d.Key;
				Texture2D t = d.Value;
				float layer = 1 - ((md.position.Y - minY) / height) * 0.95F;
				game.spriteBatch.Draw(t, new Rectangle((int) md.position.X - w / 2, (int) md.position.Y - h / 2, w, h), null, Color.White, 0.0F, new Vector2(0, 0), SpriteEffects.None, layer);

				if(m.X >= md.position.X - w / 2 && m.X < md.position.X + w / 2 && m.Y >= md.position.Y - h / 2 && m.Y < md.position.Y + h / 2)
					game.spriteBatch.Draw(squareTex, new Rectangle((int) md.position.X - w / 2, (int) md.position.Y - h / 2, w, h), null, Color.White * 0.5F, 0.0F, new Vector2(0, 0), SpriteEffects.None, layer - 0.02F);

				if(game.levelsComplete.Contains(md.name))
					game.spriteBatch.Draw(completeTex, new Rectangle((int) md.position.X + w / 2 - 3 * completeTex.Width / 4, (int) md.position.Y - h / 2 - completeTex.Height / 4, completeTex.Width, completeTex.Height), null, Color.White, 0.0F, new Vector2(0, 0), SpriteEffects.None, layer - 0.02F);
			}
			game.spriteBatch.End();
		}
	}
}
