using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MazeGenerator {
	public bool[,] Generate(int w, int h) {
		Maze m = new Maze(w, h);
		return m.Generate();
	}
}

internal class Maze {
	private bool[,] blocks;
	private bool[,] visited;

	private Random rand = new Random();

	public Maze(int w, int h) {
		blocks = new bool[w * 2 + 1, h * 2 + 1];
		visited = new bool[w + 2, h + 2];

		for(int i = 0; i < w * 2 + 1; i++) for(int j = 0; j < h * 2 + 1; j++) blocks[i, j] = true;
		for(int i = 0; i < w + 2; i++) for(int j = 0; j < h + 2; j++) visited[i, j] = i == 0 || i == w + 1 || j == 0 || j == h + 1;
	}

	public bool[,] Generate() {
		gen(0, 0);
		return blocks;
	}

	private void gen(int x, int y) {
		int bx = x * 2 + 1;
		int by = y * 2 + 1;
		int vx = x + 1;
		int vy = y + 1;

		blocks[bx, by] = false;
		visited[vx, vy] = true;

		while(!(visited[vx - 1, vy] && visited[vx + 1, vy] && visited[vx, vy - 1] && visited[vx, vy + 1])) {
			int n = rand.Next(4);
			if(n == 0 && !visited[vx - 1, vy]) {
				blocks[bx - 1, by] = false;
				gen(x - 1, y);
			}
			else if(n == 1 && !visited[vx + 1, vy]) {
				blocks[bx + 1, by] = false;
				gen(x + 1, y);
			}
			else if(n == 2 && !visited[vx, vy - 1]) {
				blocks[bx, by - 1] = false;
				gen(x, y - 1);
			}
			else if(n == 3 && !visited[vx, vy + 1]) {
				blocks[bx, by + 1] = false;
				gen(x, y + 1);
			}
		}
	}
}