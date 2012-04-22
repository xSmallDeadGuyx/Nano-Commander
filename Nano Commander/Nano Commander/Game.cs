using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace Nano_Commander {
	public class Game : Microsoft.Xna.Framework.Game {

		public bool helpPrevPressed = false;

		public SoundEffectInstance gameMusic;
		public SoundEffectInstance menuMusic;
		public SoundEffectInstance shootEffect;
		public SoundEffectInstance dieEffect;

		private float elapsedTime;
		private float totalFrames;
		private float fps;

		public int endTimer = 0;
		public bool won = false;
		public Texture2D winTex;
		public Texture2D loseTex;
		public Texture2D helpTex;

		public List<string> levelsComplete = new List<string>();

		public int windowWidth {
			get { return GraphicsDevice.PresentationParameters.BackBufferWidth; }
		}
		public int windowHeight {
			get { return GraphicsDevice.PresentationParameters.BackBufferHeight; }
		}

		public GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;

		public bool screenshotPrev = false;

		public SpriteFont textFont;

		public PlayingField playingField;
		public Camera camera;

		public Menu menu;

		public enum GameState { Menu, InGame, Help };

		public GameState state = GameState.Menu;

		public int ssCount = 0;

		public Game() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			graphics.PreferredBackBufferWidth = 640;
			graphics.PreferredBackBufferHeight = 480;

			IsMouseVisible = true;
		}

		public void save() {
			BinaryWriter bw = new BinaryWriter(new FileStream("save.dat", FileMode.Create));
			bw.Write((byte) menu.playerCol);
			foreach(String s in levelsComplete) {
				bw.Write((byte) s.Length);
				foreach(char c in s.ToCharArray())
					bw.Write((byte) (c + 83));
			}
			bw.BaseStream.Close();
			bw.Close();
		}

		public void load() {
			if(File.Exists("save.dat")) {
				BinaryReader br = new BinaryReader(new FileStream("save.dat", FileMode.Open));
				menu.playerCol = (int) br.ReadByte();
				byte[] data = br.ReadBytes((int) br.BaseStream.Length - 1);

				bool readingName = false;
				string name = "";
				byte nameLength = 0;
				byte count = 0;
				levelsComplete.Clear();

				foreach(Byte b in data) {
					if(readingName) {
						if(count < nameLength)
							name += (char) (b - 83);
						else {
							readingName = false;
							levelsComplete.Add(name);
						}
					}
					if(!readingName) {
						readingName = true;
						nameLength = b;
						count = 0;
						name = "";
					}
				}
				if(name != "")
					levelsComplete.Add(name);
				br.BaseStream.Close();
				br.Close();
			}
		}

		public void endGame(string map, bool win) {
			if(win) levelsComplete.Add(map);
			won = win;
			endTimer++;
			save();
		}

		public void beginGame(string map, Color col) {
			state = GameState.InGame;
			playingField = new PlayingField(this);
			playingField.loadContent(map);
			playingField.playerCol = col;
		}

		protected override void Initialize() {
			base.Initialize();
		}
		
		protected override void LoadContent() {
			spriteBatch = new SpriteBatch(GraphicsDevice);
			camera = new Camera(this);

			SoundEffect menuSe = Content.Load<SoundEffect>("menu");
			SoundEffect mainSe = Content.Load<SoundEffect>("main");
			SoundEffect shootSe = Content.Load<SoundEffect>("shoot");
			SoundEffect dieSe = Content.Load<SoundEffect>("die");

			winTex = Content.Load<Texture2D>("youwin");
			loseTex = Content.Load<Texture2D>("youlose");
			helpTex = Content.Load<Texture2D>("help");

			menuMusic = menuSe.CreateInstance();
			menuMusic.Volume = 0.25F;
			menuMusic.IsLooped = true;
			gameMusic = mainSe.CreateInstance();
			gameMusic.Volume = 0.25F;
			gameMusic.IsLooped = true;

			shootEffect = shootSe.CreateInstance();
			shootEffect.Volume = 0.1F;
			dieEffect = dieSe.CreateInstance();
			dieEffect.Volume = 0.25F;

			textFont = Content.Load<SpriteFont>("Text");
			
			menu = new Menu(this);
			load();
			menu.center = new Vector2(320, 320);
		}

		protected override void UnloadContent() {}

		protected override void Update(GameTime gt) {
			KeyboardState kb = Keyboard.GetState();

			if(state == GameState.Menu) {
				if(menuMusic.State != SoundState.Playing) menuMusic.Play();
				if(gameMusic.State == SoundState.Playing) gameMusic.Stop();
				menu.updateMenu();

				if(kb.IsKeyDown(Keys.F1) && !helpPrevPressed) state = GameState.Help;
			}
			else if(endTimer == 0 && state == GameState.InGame) {
				if(menuMusic.State == SoundState.Playing) menuMusic.Stop();
				if(gameMusic.State != SoundState.Playing) gameMusic.Play();
				playingField.update(gt);
			}
			else if(endTimer == 0) {
				if(kb.IsKeyDown(Keys.F1) && !helpPrevPressed) state = GameState.Menu;
			}
			else {
				endTimer++;
				if(endTimer > 100) {
					playingField = null;
					state = GameState.Menu;
					endTimer = 0;
				}
			}

			if(Controls.screenshotPressed && !screenshotPrev) {
				ssCount += 1;

				int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
				int h = GraphicsDevice.PresentationParameters.BackBufferHeight;

				Draw(new GameTime());

				int[] backBuffer = new int[w * h];
				GraphicsDevice.GetBackBufferData(backBuffer);

				Texture2D texture = new Texture2D(GraphicsDevice, w, h, false, GraphicsDevice.PresentationParameters.BackBufferFormat);
				texture.SetData(backBuffer);

				Stream stream = File.OpenWrite("ss" + ssCount + ".png");

				texture.SaveAsPng(stream, w, h);
				stream.Dispose();
				texture.Dispose();
			}

			elapsedTime += (float) gt.ElapsedGameTime.TotalSeconds;
			totalFrames++;

			if(elapsedTime >= 1.0f) {
				fps = totalFrames;
				totalFrames = 0;
				elapsedTime = 0;
			}

			Window.Title = "Nano Commander : " + fps + " fps";

			screenshotPrev = Controls.screenshotPressed;
			helpPrevPressed = kb.IsKeyDown(Keys.F1);
			base.Update(gt);
		}

		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.CornflowerBlue);

			if(state == GameState.Menu) {
				menu.drawMenu();
				spriteBatch.Begin();
				spriteBatch.DrawString(textFont, "Press F1\nfor help", new Vector2(16, 400), Color.Black);
				spriteBatch.End();
			}
			else if(state == GameState.InGame) {
				spriteBatch.Begin();
				playingField.draw();

				if(endTimer > 0)
					spriteBatch.Draw(won ? winTex : loseTex, Vector2.Zero, Color.White);
				spriteBatch.End();
			}
			else {
				spriteBatch.Begin();
				spriteBatch.Draw(menu.titleBgTex, Vector2.Zero, Color.White);
				spriteBatch.Draw(helpTex, Vector2.Zero, Color.White);
				spriteBatch.End();
			}

			base.Draw(gameTime);
		}
	}
}
