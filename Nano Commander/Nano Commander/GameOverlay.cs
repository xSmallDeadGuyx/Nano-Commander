using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Nano_Commander {
	public class GameOverlay {

		public bool leftPrevPressed = false;
		
		public int height = 96;

		public Texture2D overlayTex;
		public Texture2D healthTex;
		public Texture2D rangeTex;
		public Texture2D damageTex;

		public int healthCost = 10;
		public int rangeCost = 15;
		public int damageCost = 20;

		public int healthUp = 10;
		public int rangeUp = 5;
		public int damageUp = 1;

		public Game game;

		public GameOverlay(Game g) {
			game = g;
		}

		public void loadContent() {
			healthTex = game.Content.Load<Texture2D>("pu_health");
			rangeTex = game.Content.Load<Texture2D>("pu_range");
			damageTex = game.Content.Load<Texture2D>("pu_damage");
			overlayTex = game.Content.Load<Texture2D>("overlay");
		}

		public void update() {
			float discount = game.playingField.selectedUnits.Count > 20 ? 0.5F : game.playingField.selectedUnits.Count > 10 ? 0.75F : game.playingField.selectedUnits.Count > 5 ? 0.9F : 1.0F;

			MouseState m = Mouse.GetState();
			if(m.LeftButton == ButtonState.Pressed && !leftPrevPressed) {
				if(m.X >= 43 && m.X < 43 + healthTex.Width && m.Y >= game.windowHeight - height / 2 - healthTex.Height / 2 && m.Y < game.windowHeight - height / 2 - healthTex.Height / 2 + healthTex.Height)
					if(game.playingField.monies >= (int) (discount * game.playingField.selectedUnits.Count * healthCost)) {
						foreach(Unit u in game.playingField.selectedUnits) {
							u.maxHealth += healthUp;
							u.health += healthUp;
						}
						game.playingField.monies -= (int) (discount * game.playingField.selectedUnits.Count * healthCost);
					}
				if(m.X >= 171 && m.X < 171 + rangeTex.Width && m.Y >= game.windowHeight - height / 2 - rangeTex.Height / 2 && m.Y < game.windowHeight - height / 2 - rangeTex.Height / 2 + rangeTex.Height)
					if(game.playingField.monies >= (int) (discount * game.playingField.selectedUnits.Count * rangeCost)) {
						foreach(Unit u in game.playingField.selectedUnits)
							u.maxRange += rangeUp;
						game.playingField.monies -= (int) (discount * game.playingField.selectedUnits.Count * rangeCost);
					}
				if(m.X >= 299 && m.X < 299 + damageTex.Width && m.Y >= game.windowHeight - height / 2 - damageTex.Height / 2 && m.Y < game.windowHeight - height / 2 - damageTex.Height / 2 + damageTex.Height)
					if(game.playingField.monies >= (int) (discount * game.playingField.selectedUnits.Count * damageCost)) {
						foreach(Unit u in game.playingField.selectedUnits)
							u.damage += damageUp;
						game.playingField.monies -= (int) (discount * game.playingField.selectedUnits.Count * damageCost);
					}
			}
			leftPrevPressed = m.LeftButton == ButtonState.Pressed;
		}

		public void draw() {
			float discount = game.playingField.selectedUnits.Count > 20 ? 0.5F : game.playingField.selectedUnits.Count > 10 ? 0.75F : game.playingField.selectedUnits.Count > 5 ? 0.9F : 1.0F;

			MouseState m = Mouse.GetState();
			game.spriteBatch.Draw(overlayTex, new Vector2(0, game.windowHeight - height), Color.White);

			game.spriteBatch.DrawString(game.textFont, "Health++", new Vector2(16, game.windowHeight - height / 2 - healthTex.Height - 8), Color.White);
			game.spriteBatch.Draw(healthTex, new Vector2(43, game.windowHeight - height / 2 - healthTex.Height / 2), Color.White * (game.playingField.monies >= (int) (discount * game.playingField.selectedUnits.Count * healthCost) ? 1.0F : 0.5F));

			game.spriteBatch.DrawString(game.textFont, "Range++", new Vector2(148, game.windowHeight - height / 2 - healthTex.Height - 8), Color.White);
			game.spriteBatch.Draw(rangeTex, new Vector2(171, game.windowHeight - height / 2 - rangeTex.Height / 2), Color.White * (game.playingField.monies >= (int) (discount * game.playingField.selectedUnits.Count * rangeCost) ? 1.0F : 0.5F));

			game.spriteBatch.DrawString(game.textFont, "Damage++", new Vector2(272, game.windowHeight - height / 2 - healthTex.Height - 8), Color.White);
			game.spriteBatch.Draw(damageTex, new Vector2(299, game.windowHeight - height / 2 - damageTex.Height / 2), Color.White * (game.playingField.monies >= (int) (discount * game.playingField.selectedUnits.Count * damageCost) ? 1.0F : 0.5F));

			if(m.X >= 43 && m.X < 43 + healthTex.Width && m.Y >= game.windowHeight - height / 2 - healthTex.Height / 2 && m.Y < game.windowHeight - height / 2 - healthTex.Height / 2 + healthTex.Height)
				if(game.playingField.monies >= (int) (discount * game.playingField.selectedUnits.Count * healthCost))
					game.spriteBatch.Draw(game.playingField.squareTex, new Rectangle(43, game.windowHeight - height / 2 - healthTex.Height / 2, healthTex.Width, healthTex.Height), Color.White * 0.5F);
			
			if(m.X >= 171 && m.X < 171 + rangeTex.Width && m.Y >= game.windowHeight - height / 2 - rangeTex.Height / 2 && m.Y < game.windowHeight - height / 2 - rangeTex.Height / 2 + rangeTex.Height)
				if(game.playingField.monies >= (int) (discount * game.playingField.selectedUnits.Count * rangeCost))
					game.spriteBatch.Draw(game.playingField.squareTex, new Rectangle(171, game.windowHeight - height / 2 - rangeTex.Height / 2, rangeTex.Width, rangeTex.Height), Color.White * 0.5F);
			
			if(m.X >= 299 && m.X < 299 + damageTex.Width && m.Y >= game.windowHeight - height / 2 - damageTex.Height / 2 && m.Y < game.windowHeight - height / 2 - damageTex.Height / 2 + damageTex.Height)
				if(game.playingField.monies >= (int) (discount * game.playingField.selectedUnits.Count * damageCost))
					game.spriteBatch.Draw(game.playingField.squareTex, new Rectangle(299, game.windowHeight - height / 2 - damageTex.Height / 2, damageTex.Width, damageTex.Height), Color.White * 0.5F);

			game.spriteBatch.DrawString(game.textFont, "Monies: " + game.playingField.monies, new Vector2(380, game.windowHeight - height / 2 - 16), Color.White);

			if(game.playingField.selectedUnits.Count > 0) {
				game.spriteBatch.DrawString(game.textFont, "Cost: " + ((int) (discount * game.playingField.selectedUnits.Count * healthCost)), new Vector2(16, game.windowHeight - height / 2 + healthTex.Height / 2), Color.White);
				game.spriteBatch.DrawString(game.textFont, "Cost: " + ((int) (discount * game.playingField.selectedUnits.Count * rangeCost)), new Vector2(144, game.windowHeight - height / 2 + rangeTex.Height / 2), Color.White);
				game.spriteBatch.DrawString(game.textFont, "Cost: " + ((int) (discount * game.playingField.selectedUnits.Count * damageCost)), new Vector2(272, game.windowHeight - height / 2 + damageTex.Height / 2), Color.White);
			}

			game.spriteBatch.DrawString(game.textFont, "Avg. Health:", new Vector2(504, game.windowHeight - height / 2 - 32), Color.White);
			game.spriteBatch.Draw(game.playingField.squareTex, new Rectangle(520, game.windowHeight - height / 2, 100, 16), Color.Black);

			float averageHealth = 0;
			foreach(Unit u in game.playingField.friendlyUnits)
				averageHealth += (float) u.health / (float) u.maxHealth;
			int drawWidth = (int) (96.0F * averageHealth / (float) game.playingField.friendlyUnits.Count);
			game.spriteBatch.Draw(game.playingField.squareTex, new Rectangle(522, game.windowHeight - height / 2 + 2, drawWidth, 12), new Color(1.0F - (drawWidth / 96.0F), drawWidth / 96.0F, 0.0F));
		}
	}
}
