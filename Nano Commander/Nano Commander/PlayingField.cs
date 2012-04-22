using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Nano_Commander {
	public class Bullet {
		public Vector2 position;
		public Vector2 speed;
		public bool isEnemy;
		public int damage;
	}
	public class PlayingField {

		public int monies = 0;

		public float bgMult = 1.0F;

		public Color playerCol = Color.ForestGreen;

		public bool firstTick = true;

		public Texture2D bgTex;
		public Texture2D wallTex;
		public Texture2D selectOverlay;
		public Texture2D unitTex;
		public Texture2D squareTex;
		public Texture2D moveTex;

		public int moveTimer = 0;
		public Vector2 lastMove = Vector2.Zero;

		public Game game;

		public string mapName;
		public bool[,] mapData;
		public Pathfinder pathfinder;

		public List<Unit> friendlyUnits;
		public List<Unit> enemyUnits;
		public List<Unit> selectedUnits;
		public List<Bullet> bullets;

		public List<Unit> deadUnits;
		public List<Bullet> deadBullets;

		private bool selectingUnits = false;
		private Vector2 selectStart;
		private Rectangle selectRect;

		public const int cameraThreshold = 96;
		public const int cameraDivisor = 24;
		public const float bulletSpeed = 7.5F;
		public GameOverlay gameOverlay;

		public bool rightPrevPressed = false;

		public Random rand = new Random();

		public PlayingField(Game g) {
			game = g;
			friendlyUnits = new List<Unit>();
			enemyUnits = new List<Unit>();
			selectedUnits = new List<Unit>();
			bullets = new List<Bullet>();

			deadUnits = new List<Unit>();
			deadBullets = new List<Bullet>();

			gameOverlay = new GameOverlay(game);
		}

		public void loadContent(string map) {
			mapName = map;
			bgTex = game.Content.Load<Texture2D>("background");
			wallTex = game.Content.Load<Texture2D>("walls");
			selectOverlay = game.Content.Load<Texture2D>("select");
			unitTex = game.Content.Load<Texture2D>("unit");
			squareTex = game.Content.Load<Texture2D>("square");
			moveTex = game.Content.Load<Texture2D>("move");

			gameOverlay.loadContent();

			Texture2D t = game.Content.Load<Texture2D>(map);
			Color[] colours = new Color[t.Width * t.Height];

			t.GetData(colours);
			mapData = new bool[t.Width, t.Height];

			for(int i = 0; i < t.Width; i++) for(int j = 0; j < t.Height; j++) {
				if(colours[i + j * t.Width] == Color.Lime) createUnit(new Vector2(i, j) * 16, false);
				if(colours[i + j * t.Width] == Color.Red) createUnit(new Vector2(i, j) * 16, true);
				mapData[i, j] = colours[i + j * t.Width] == Color.Black;
			}

			bgMult = 1.0F / Math.Max(mapData.GetLength(0) * 16.0F / (float) bgTex.Width, mapData.GetLength(1) * 16.0F / (float) bgTex.Height);

			pathfinder = new Pathfinder(mapData);
		}

		public void createUnit(Vector2 pos, bool enemy) {
			Unit u = new Unit(game);
			u.isEnemy = enemy;
			u.SetPositionAndSnap(pos);
			u.frame = rand.Next(unitTex.Width / 16);
			if(enemy) enemyUnits.Add(u);
			else friendlyUnits.Add(u);
		}

		public void createBullet(Unit u, float direction) {
			Bullet b = new Bullet();
			b.position = u.Position + new Vector2(8, 8);
			b.speed = bulletSpeed * new Vector2((float) Math.Cos(direction), (float) Math.Sin(direction));
			b.isEnemy = u.isEnemy;
			b.damage = u.damage;
			bullets.Add(b);
			game.shootEffect.Play();
		}

		public bool anyUnitsInSpaceExcluding(Vector2 pos, Unit me) {
			foreach(Unit u in friendlyUnits)
				if((new Rectangle((int) pos.X, (int) pos.Y, 16, 16)).Intersects(new Rectangle((int) u.Position.X, (int) u.Position.Y, 16, 16)) && u != me) return true;
			foreach(Unit u in enemyUnits)
				if((new Rectangle((int) pos.X, (int) pos.Y, 16, 16)).Intersects(new Rectangle((int) u.Position.X, (int) u.Position.Y, 16, 16)) && u != me) return true;
			return false;
		}

		public bool unitInRectangle(Unit u, Rectangle r) {
			Rectangle uRect = new Rectangle((int) u.Position.X, (int) u.Position.Y, 16, 16);
			return r.Intersects(uRect);
		}

		public Vector2 getCenterInGroup(List<Unit> us) {
			Vector2 center = Vector2.Zero;
			foreach(Unit u in us) center += u.Position;
			return center / us.Count;
		}

		public bool isSpaceEmptyForUnit(Vector2 pos, Unit me) {
			return !mapData[(int) pos.X, (int) pos.Y]/* && !anyUnitsInSpaceExcludingSelected(pos * 16)*/;
		}

		public List<Unit> getEnemyUnitsInRange(Unit me) {
			List<Unit> found = new List<Unit>();
			List<Unit> units = me.isEnemy ? friendlyUnits : enemyUnits;
			foreach(Unit u in units) if(Vector2.Distance(u.Position, me.Position) <= me.maxRange && u.isEnemy != me.isEnemy) found.Add(u);
			return found;
		}

		public void update(GameTime gt) {
			if(firstTick)
				game.camera.position = getCenterInGroup(friendlyUnits) - new Vector2(game.windowWidth / 2, game.windowHeight / 2 - gameOverlay.height / 2);
			firstTick = false;

			MouseState ms = Mouse.GetState();
			Vector2 m = new Vector2(ms.X, ms.Y) + game.camera.position;

			if(ms.LeftButton == ButtonState.Pressed && ms.Y < game.windowHeight - gameOverlay.height) {
				if(!selectingUnits) {
					selectStart = new Vector2(m.X, m.Y);
					selectRect = new Rectangle();
				}

				selectRect.X = (int) (selectStart.X > m.X ? m.X : selectStart.X);
				selectRect.Y = (int) (selectStart.Y > m.Y ? m.Y : selectStart.Y);
				selectRect.Width = (int) Math.Abs(selectStart.X - m.X);
				selectRect.Height = (int) Math.Abs(selectStart.Y - m.Y);

				selectingUnits = true;
			}
			else if(selectingUnits && ms.Y < game.windowHeight - gameOverlay.height) {
				selectedUnits.Clear();

				foreach(Unit u in friendlyUnits)
					if(unitInRectangle(u, selectRect) && !u.isEnemy)
						selectedUnits.Add(u);
				selectingUnits = false;
			}

			foreach(Unit u in friendlyUnits) u.update();
			foreach(Unit u in enemyUnits) u.update();

			foreach(Bullet b in bullets) {
				b.position += b.speed;
				List<Unit> units = b.isEnemy ? friendlyUnits : enemyUnits;
				foreach(Unit u in units)
					if((new Rectangle((int) b.position.X, (int) b.position.Y, 4, 4)).Intersects(new Rectangle((int) u.Position.X, (int) u.Position.Y, 16, 16))) {
						deadBullets.Add(b);
						u.health -= b.damage;
						if(u.health <= 0) {
							deadUnits.Add(u);
							if(u.isEnemy) monies += 10;
							game.dieEffect.Play();
						}
					}
				if(b.position.X < 0 || b.position.X > mapData.GetLength(0) * 16 - 16 || b.position.Y < 0 || b.position.Y > mapData.GetLength(1) * 16 - 16 || mapData[(int) b.position.X / 16, (int) b.position.Y / 16]) deadBullets.Add(b);
			}

			foreach(Bullet b in deadBullets) bullets.Remove(b);
			foreach(Unit u in deadUnits) {
				if(u.isEnemy) enemyUnits.Remove(u);
				else friendlyUnits.Remove(u);
				if(selectedUnits.Contains(u)) selectedUnits.Remove(u);
			}
			deadBullets.Clear();
			deadUnits.Clear();

			if(enemyUnits.Count == 0)
				game.endGame(mapName, true);
			if(friendlyUnits.Count == 0)
				game.endGame(mapName, false);

			if(ms.RightButton == ButtonState.Pressed && !rightPrevPressed) {
				Vector2 target = new Vector2((int) ((ms.X + game.camera.position.X) / 16), (int) ((ms.Y + game.camera.position.Y) / 16)) * 16;
				Vector2 center = getCenterInGroup(selectedUnits);
				foreach(Unit u in selectedUnits) {
					if(!mapData[(int) target.X / 16, (int) target.Y / 16]) {
						u.moveToTarget(m);
						if(lastMove != target / 16) {
							moveTimer = 10;
							lastMove = target / 16;
						}
					}
					else
						u.removeTarget();
				}
			}

			if(moveTimer > 0) moveTimer--;

			int dx = ms.X - game.windowWidth / 2;
			int dy = ms.Y - (game.windowHeight - gameOverlay.height) / 2;
			if((ms.X <= cameraThreshold || ms.X > game.windowWidth - cameraThreshold || ms.Y <= cameraThreshold || ms.Y > game.windowHeight - cameraThreshold - gameOverlay.height) && ms.Y <= game.windowHeight - gameOverlay.height)
				game.camera.position += new Vector2(dx / cameraDivisor, dy / cameraDivisor);

			gameOverlay.update();
			rightPrevPressed = ms.RightButton == ButtonState.Pressed;
		}

		public int getSides(Vector2 pos) {
			int sides = 0;
			sides += pos.Y == 0 || mapData[(int) pos.X, (int) pos.Y - 1] ? 1 : 0;
			sides += pos.X == mapData.GetLength(0) - 1 || mapData[(int) pos.X + 1, (int) pos.Y] ? 2 : 0;
			sides += pos.Y == mapData.GetLength(1) - 1 || mapData[(int) pos.X, (int) pos.Y + 1] ? 4 : 0;
			sides += pos.X == 0 || mapData[(int) pos.X - 1, (int) pos.Y] ? 8: 0;
			return sides;
		}

		public void draw() {
			game.camera.Draw(bgTex, game.camera.position * bgMult, Color.White);

			for(int i = (int) game.camera.position.X / 16; i <= game.camera.position.X / 16 + game.windowWidth / 16; i++) for(int j = (int) game.camera.position.Y / 16; j <= game.camera.position.Y / 16 + game.windowHeight / 16 - gameOverlay.height / 16; j++)
				if(mapData[i, j]) game.camera.Draw(wallTex, new Vector2(i * 16, j * 16), new Rectangle(getSides(new Vector2(i, j)) * 16, 0, 16, 16), Color.White);

			foreach(Unit u in friendlyUnits) {
				u.draw();
				if(selectedUnits.Contains(u))
					game.camera.Draw(selectOverlay, u.Position, Color.Yellow);
			}
			foreach(Unit u in enemyUnits)
				u.draw();

			foreach(Bullet b in bullets)
				game.camera.Draw(squareTex, new Rectangle((int) b.position.X, (int) b.position.Y, 4, 4), Color.Blue);

			if(selectingUnits)
				game.camera.Draw(squareTex, selectRect, new Color(0.0F, 0.5F, 0.0F, 0.25F));

			if(moveTimer > 0)
				game.camera.Draw(moveTex, lastMove * 16, new Color(1.0F, 1.0F, 1.0F, 1.0F - moveTimer / 10.0F));

			gameOverlay.draw();
		}
	}
}
