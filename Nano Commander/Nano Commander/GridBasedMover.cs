using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

public class GridBasedMover {
	public Vector2 Dir = Direction.Down;

	public int GSize = 16;

	private Vector2 position;
	public Vector2 Position {
		get { return position; }
		set { position = value; }
	}

	public bool moving = false;
	public int speed = 1;

	public bool isSolid = true;

	public virtual void update() {}
	public virtual void draw() {}

	private Vector2 targetSpace;
	public Vector2 TargetSpace {
		get { return targetSpace; }
		set { if(value.X % GSize == 0 && value.Y % GSize == 0) targetSpace = value; }
	}

	public Vector2 NextDir;

	public GridBasedMover() {
		NextDir = Dir;
		targetSpace = position;
	}

	public void SetPositionAndSnap(Vector2 pos) {
		if(pos.X % GSize != 0) pos.X = (float) Math.Round(pos.X / GSize) * GSize;
		if(pos.Y % GSize != 0) pos.Y = (float) Math.Round(pos.Y / GSize) * GSize;
		position = targetSpace = pos;
	}

	public void RecalcTarget() {
		Vector2 newPos = (position + (Dir * speed)) / GSize + Dir;
		targetSpace = new Vector2((float) Math.Round(newPos.X) * GSize, (float) Math.Round(newPos.Y) * GSize);
	}

	public bool AtOrPastTargetSpace(Vector2 pos) {
		return (Dir == Direction.Left && pos.X <= targetSpace.X) ||
				(Dir == Direction.Right && pos.X >= targetSpace.X) ||
				(Dir == Direction.Up && pos.Y <= targetSpace.Y) ||
				(Dir == Direction.Down && pos.Y >= targetSpace.Y);
	}

	public virtual bool canMoveTo(Vector2 pos) {
		return true; // to be overwritten
	}

	public void UpdateMovement() {
		if((position.Equals(targetSpace) || AtOrPastTargetSpace(position)) && (NextDir != Dir || moving)) {
			Dir = NextDir;
			RecalcTarget();

			if(!canMoveTo(targetSpace)) {
				moving = false;
				targetSpace = position;
			}
		}
		if(!position.Equals(targetSpace)) moving = true;

		if(moving) {
			Vector2 movement = speed * Dir;
			Vector2 newPosition = position + movement;

			if(AtOrPastTargetSpace(newPosition)) position = targetSpace;
			else position = newPosition;
		}
	}
}

public class Direction {
	public static readonly Vector2 Down = new Vector2(0, 1);
	public static readonly Vector2 Up = new Vector2(0, -1);
	public static readonly Vector2 Left = new Vector2(-1, 0);
	public static readonly Vector2 Right = new Vector2(1, 0);
}