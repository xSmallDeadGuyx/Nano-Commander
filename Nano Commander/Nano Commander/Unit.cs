using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nano_Commander {
	public class Unit : GridBasedMover {

		public int frameTime = 3;
		public int frameTimer = 0;
		public int frame = 0;

		public int targetTimer = 0;

		public int shootTime = 20;
		public int maxRange = 100;
		public int maxHealth = 100;
		public int damage = 4;

		public int wanderTime = 50;
		public int wanderTimer = 0;

		public int health = 100;

		public Game game;

		public List<Vector2> path;

		public Unit attackTarget = null;
		public int shootTimer = 0;
		
		public bool isEnemy = false;

		public Unit(Game g) : base() {
			game = g;
		}

		public void moveToTarget(Vector2 target) {
			path = game.playingField.pathfinder.FindPath(new Vector2((int) Position.X / 16, (int) Position.Y / 16), new Vector2((int) target.X / 16, (int) target.Y / 16), this);
		}

		public void removeTarget() {
			path = null;
			moving = false;
			NextDir = Dir;
		}

		public Vector2 directionToNextPoint() {
			float x = path[0].X - Position.X / 16.0F;
			float y = path[0].Y - Position.Y / 16.0F;
			return new Vector2(Math.Sign(x), Math.Sign(y));
		}

		public override bool canMoveTo(Vector2 pos) {
			return !game.playingField.mapData[(int) pos.X / 16, (int) pos.Y / 16] && !game.playingField.anyUnitsInSpaceExcluding(pos, this);
		}

		public override void update() {
			if(Position.X < 0 || Position.X > game.playingField.mapData.GetLength(0) * 16 - 16 || Position.Y < 0 || Position.Y > game.playingField.mapData.GetLength(1) * 16 - 16) {
				game.playingField.deadUnits.Add(this);
				return;
			}
			if(!isEnemy) {
				if(path != null && path.Count > 0) {
					moving = true;
					if(Math.Abs(Math.Atan2(directionToNextPoint().Y, directionToNextPoint().X) % 90) < 5)
						NextDir = directionToNextPoint();
					if(NextDir == Vector2.Zero) {
						path.RemoveAt(0);
						if(path != null && path.Count > 0) NextDir = directionToNextPoint();
					}
				}
				else
					moving = false;
			}
			else {
				if(wanderTimer > 0) {
					moving = false;
					wanderTimer--;
				}
				else {
					wanderTimer = wanderTime + game.playingField.rand.Next(20);
					moving = true;
					while(NextDir == Dir)
						NextDir = new Vector2(game.playingField.rand.Next(-1, 2), game.playingField.rand.Next(-1, 2));
				}
			}

			UpdateMovement();


			List<Unit> enemyList = isEnemy ? game.playingField.friendlyUnits : game.playingField.enemyUnits;
			if(attackTarget == null || !enemyList.Contains(attackTarget) || Vector2.Distance(attackTarget.Position, Position) > maxRange || targetTimer > 200) {
				List<Unit> inRange = game.playingField.getEnemyUnitsInRange(this);
				if(inRange != null && inRange.Count > 0) {
					int num = game.playingField.rand.Next(inRange.Count);
					attackTarget = inRange[num];
				}
				targetTimer = 0;
			}
			else {
				targetTimer++;
				if(shootTimer == 0) {
					game.playingField.createBullet(this, (float) Math.Atan2(attackTarget.Position.Y - Position.Y, attackTarget.Position.X - Position.X));
					shootTimer = shootTime;
				}
			}
			if(shootTimer > 0)
				shootTimer--;
		}

		public override void draw() {
			if(frameTimer > 0) frameTimer--;
			else {
				frame++;
				if(frame >= game.playingField.unitTex.Width / 16)
					frame = 0;
				frameTimer = frameTime;
			}

			game.camera.Draw(game.playingField.unitTex, Position, new Rectangle(frame * 16, 0, 16, 16), isEnemy ? Color.Red : game.playingField.playerCol);
		}
	}
}
